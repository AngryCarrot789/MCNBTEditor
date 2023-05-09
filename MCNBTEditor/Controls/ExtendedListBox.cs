using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MCNBTEditor.Core;
using MCNBTEditor.Core.Explorer;

namespace MCNBTEditor.Controls {
    public class ExtendedListBox : ListBox, IExtendedList {
        public static readonly DependencyProperty UseItemCommandProperty = DependencyProperty.Register("UseItemCommand", typeof(ICommand), typeof(ExtendedListBox), new PropertyMetadata(null));

        public ICommand UseItemCommand {
            get => (ICommand) this.GetValue(UseItemCommandProperty);
            set => this.SetValue(UseItemCommandProperty, value);
        }

        IEnumerable<BaseTreeItemViewModel> IExtendedList.SelectedItems => base.SelectedItems.OfType<BaseTreeItemViewModel>();

        int IExtendedList.Count => this.Items.Count;

        public new event SelectionChangedEventHandler<IEnumerable<BaseTreeItemViewModel>> SelectionChanged;
        
        public int IndexOf(BaseTreeItemViewModel item) {
            return this.Items.IndexOf(item);
        }

        private ScrollViewer PART_ScrollViewer;
        private SelectionAdorder adorner;
        private Point point;
        private bool isDragging;
        private bool isWatingForLargeMouseMove;

        public ExtendedListBox() {
            this.PreviewMouseWheel += this.ExtendedListBox_PreviewMouseWheel;
            // this.Loaded += this.OnLoaded;
            // this.Unloaded += this.OnUnloaded;
            // this.PreviewMouseDown += this.OnPreviewMouseDown;
            // this.MouseDown += this.OnMouseDown;
            // this.MouseMove += this.OnMouseMove;
            // this.MouseUp += this.OnMouseUp;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            if (this.adorner == null) {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
                if (layer != null) {
                    this.adorner = new SelectionAdorder(this) {
                        Background = new SolidColorBrush(Colors.DodgerBlue),
                        BorderBrush = new SolidColorBrush(Colors.SkyBlue),
                        BorderThickness = new Thickness(1),
                        Visibility = Visibility.Collapsed
                    };

                    layer.Add(this.adorner);
                    this.adorner.IsClipEnabled = true;
                    this.adorner.ClipToBounds = true;
                }
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            if (this.adorner != null) {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
                layer?.Remove(this.adorner);
                this.adorner = null;
            }
        }

        private void OnBeginDragForMouseDown(Point point) {
            Mouse.Capture((IInputElement) this, CaptureMode.SubTree);
            this.point = point;
            this.isWatingForLargeMouseMove = true;
        }

        private void OnStopDragForMouseUp() {
            this.isDragging = false;
            this.isWatingForLargeMouseMove = false;
            this.ReleaseMouseCapture();
            this.point = default;
            this.adorner.Visibility = Visibility.Collapsed;
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (e.Handled || this.adorner == null)
                return;

            if (e.OriginalSource is Border border && border.TemplatedParent is ListBoxItem lbi) {
                if (lbi.IsMouseOver) {
                    if (lbi.IsSelected) {
                        return;
                    }

                    this.OnBeginDragForMouseDown(e.GetPosition(this));
                }
            }
        }

        public const string BaseViewModelControlKey = "ExList_ItemHandle";

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);
            if (item is BaseTreeItemViewModel treeItem) {
                BaseViewModel.SetInternalData(treeItem, BaseViewModelControlKey, element);
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);
            if (item is BaseTreeItemViewModel treeItem) {
                BaseViewModel.ClearInternalData(treeItem, BaseViewModelControlKey);
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e) {
            if (e.Handled || this.adorner == null)
                return;

            this.OnBeginDragForMouseDown(e.GetPosition(this));
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e) {
            if (e.Handled || this.adorner == null)
                return;

            if (this.isDragging && !this.isWatingForLargeMouseMove) {
                this.OnStopDragForMouseUp();
                e.Handled = true;
            }
        }

        public readonly struct AABB {
            public readonly double X1;
            public readonly double Y1;
            public readonly double X2;
            public readonly double Y2;

            public AABB(double x1, double y1, double x2, double y2) {
                this.X1 = x1;
                this.Y1 = y1;
                this.X2 = x2;
                this.Y2 = y2;
            }

            public AABB(in Rect rect) {
                this.X1 = rect.X;
                this.Y1 = rect.Y;
                this.X2 = rect.X + rect.Width;
                this.Y2 = rect.Y + rect.Height;
            }

            public override string ToString() {
                return $"{Math.Round(this.X1, 2)}, {Math.Round(this.Y1, 2)} -> {Math.Round(this.X2, 2)}, {Math.Round(this.Y2, 2)}";
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e) {
            if (e.Handled || this.adorner == null)
                return;

            if (e.LeftButton != MouseButtonState.Pressed) {
                this.OnStopDragForMouseUp();
                return;
            }

            Point p1 = this.point;
            Point p2 = e.GetPosition(this);
            if (this.isWatingForLargeMouseMove) {
                double dX = Math.Abs(p1.X - p2.X);
                double dY = Math.Abs(p1.Y - p2.Y);
                if (dX >= 8d || dY >= 8d) {
                    this.isWatingForLargeMouseMove = false;
                    this.isDragging = true;
                    this.adorner.SetRect(p1.X, p1.Y, 0, 0);
                    this.adorner.Visibility = Visibility.Visible;
                    this.ReleaseMouseCapture();
                    this.CaptureMouse();
                }
                else {
                    return;
                }
            }

            if (!this.isDragging) {
                return;
            }

            e.Handled = true;
            double x, y, w, h;
            if (p1.X <= p2.X) {
                x = p1.X;
                w = p2.X - p1.X;
            }
            else {
                x = p2.X;
                w = p1.X - p2.X;
            }

            if (p1.Y <= p2.Y) {
                y = p1.Y;
                h = p2.Y - p1.Y;
            }
            else {
                y = p2.Y;
                h = p1.Y - p2.Y;
            }

            this.adorner.SetRect(x, y, w, h);
            if (this.SelectionMode == SelectionMode.Extended) {

                List<object> selection = new List<object>();
                Rect selectionRect = this.adorner.RenderRect;
                selectionRect = this.RenderTransform.TransformBounds(selectionRect);
                AABB a = new AABB(selectionRect);

                foreach (BaseTreeItemViewModel treeItem in this.Items) {
                    ListBoxItem item = (ListBoxItem) this.ItemContainerGenerator.ContainerFromItem(treeItem);
                    if (item == null) {
                        continue;
                    }

                    Rect itemSize = VisualTreeHelper.GetDescendantBounds(item);
                    AABB b = new AABB(item.TransformToAncestor(this).TransformBounds(itemSize));
                    if (a.X1 < b.X2 && a.X2 > b.X1 && a.Y1 < b.Y2 && a.Y2 > b.Y1) {
                        selection.Add(treeItem);
                    }
                }

                this.SetSelectedItems(selection);
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e) {
            base.OnSelectionChanged(e);
            this.SelectionChanged?.Invoke(e.RemovedItems.OfType<BaseTreeItemViewModel>(), e.AddedItems.OfType<BaseTreeItemViewModel>());
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.PART_ScrollViewer = this.GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
        }

        private void ExtendedListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift && this.PART_ScrollViewer != null) {
                if (e.Delta < 0) {
                    // scroll right
                    this.PART_ScrollViewer.LineRight();
                    this.PART_ScrollViewer.LineRight();
                    this.PART_ScrollViewer.LineRight();
                }
                else if (e.Delta > 0) {
                    this.PART_ScrollViewer.LineLeft();
                    this.PART_ScrollViewer.LineLeft();
                    this.PART_ScrollViewer.LineLeft();
                }
                else {
                    return;
                }

                e.Handled = true;
            }
        }

        private volatile bool isProcessingDoubleClick;
        private volatile bool isProcessingKeyDown;
        protected override async void OnMouseDoubleClick(MouseButtonEventArgs e) {
            base.OnMouseDoubleClick(e);
            if (e.Handled || this.isProcessingDoubleClick) {
                return;
            }

            this.isProcessingDoubleClick = true;
            try {
                if (this.SelectedItem is BaseTreeItemViewModel file) {
                    if (this.ItemContainerGenerator.ContainerFromItem(file) is ListBoxItem item) {
                        if (item.IsMouseOver) {
                            ICommand cmd = this.UseItemCommand;
                            if (cmd is BaseAsyncRelayCommand asyncCommand) {
                                await asyncCommand.ExecuteAsync(file);
                            }
                            else if (cmd != null) {
                                cmd.Execute(file);
                            }
                        }
                    }
                }
            }
            finally {
                this.isProcessingDoubleClick = false;
            }
        }

        protected override async void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (e.Handled || e.Key != Key.Enter || this.isProcessingKeyDown) {
                return;
            }

            this.isProcessingKeyDown = true;
            try {
                if (this.IsFocused) {
                    if (this.SelectedItem is BaseTreeItemViewModel file) {
                        ICommand cmd = this.UseItemCommand;
                        if (cmd is BaseAsyncRelayCommand asyncCommand) {
                            await asyncCommand.ExecuteAsync(file);
                        }
                        else if (cmd != null) {
                            cmd.Execute(file);
                        }
                    }
                }
            }
            finally {
                this.isProcessingKeyDown = false;
            }
        }
    }
}