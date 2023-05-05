using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public ExtendedListBox() {
            this.PreviewMouseWheel += this.ExtendedListBox_PreviewMouseWheel;
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