using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using MCNBTEditor.Core;
using MCNBTEditor.Core.Explorer;
using MCNBTEditor.Core.Utils;
using MCNBTEditor.Shortcuts;

namespace MCNBTEditor.Controls {
    public class ExtendedTreeView : TreeView, IExtendedTree {
        public static readonly DependencyProperty UseItemCommandProperty = DependencyProperty.Register("UseItemCommand", typeof(ICommand), typeof(ExtendedTreeView), new PropertyMetadata(null));

        public static readonly DependencyProperty IsNavigatingProperty = DependencyProperty.Register("IsNavigating", typeof(bool), typeof(ExtendedTreeView), new PropertyMetadata(BoolBox.False));

        public ICommand UseItemCommand {
            get => (ICommand) this.GetValue(UseItemCommandProperty);
            set => this.SetValue(UseItemCommandProperty, value);
        }

        public bool IsNavigating {
            get => (bool) this.GetValue(IsNavigatingProperty);
            set => this.SetValue(IsNavigatingProperty, value);
        }

        public event SelectionChangedEventHandler<BaseTreeItemViewModel> SelectionChanged;
        private ScrollViewer PART_ScrollViewier;

        public ExtendedTreeView() {

        }

        public const string BaseViewModelControlKey = "ExTree_ItemHandle";

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

        protected override DependencyObject GetContainerForItemOverride() {
            return new ExtendedTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is ExtendedTreeViewItem;
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e) {
            if (this.PART_ScrollViewier != null && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
                if (e.Delta < 0) {
                    // scroll right
                    this.PART_ScrollViewier.LineRight();
                    this.PART_ScrollViewier.LineRight();
                    this.PART_ScrollViewier.LineRight();
                }
                else if (e.Delta > 0) {
                    this.PART_ScrollViewier.LineLeft();
                    this.PART_ScrollViewier.LineLeft();
                    this.PART_ScrollViewier.LineLeft();
                }
                else {
                    return;
                }

                e.Handled = true;
                return;
            }

            base.OnPreviewMouseWheel(e);
        }

        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e) {
            base.OnSelectedItemChanged(e);
            if (e.Handled) {
                return;
            }

            this.SelectionChanged?.Invoke(e.OldValue as BaseTreeItemViewModel, e.NewValue as BaseTreeItemViewModel);
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.PART_ScrollViewier = (ScrollViewer) this.GetTemplateChild("_tv_scrollviewer_");
            // this.RequestBringIntoView += this.OnRequestBringIntoView;
        }

        public async Task<bool> NavigateToItemAsync(BaseTreeItemViewModel item) {
            if (this.IsNavigating) {
                return false;
            }

            // TreeViewItem container = BaseViewModel.GetInternalData<TreeViewItem>(item, BaseViewModelControlKey);
            // if (container != null) {
            //     if (VisualTreeUtils.FindVisualParent<TreeViewItem>(container) is TreeViewItem parentItem) {
            //         parentItem.IsExpanded = true;
            //     }
            //     await this.Dispatcher.InvokeAsync(() => container.IsSelected = true, DispatcherPriority.Background);
            //     return true;
            // }

            List<BaseTreeItemViewModel> list = new List<BaseTreeItemViewModel>();
            list.Add(item);

            for (BaseTreeItemViewModel parent = item.ParentItem; parent != null; parent = parent.ParentItem) {
                list.Add(parent);
            }

            list.Reverse();
            return await this.NavigateUsingRootPathAsync(this.ItemContainerGenerator, list);
        }

        private static readonly MethodInfo AddContainerFromGeneratorMethodInfo;
        // private static readonly MethodInfo GetCountMethodInfo;

        static ExtendedTreeView() {
            AddContainerFromGeneratorMethodInfo = typeof(VirtualizingStackPanel).GetMethod("AddContainerFromGenerator", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
            // GetCountMethodInfo = typeof(ItemContainerGenerator).GetMethod("GetCount", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
        }

        private async Task<TreeViewItem> YuckyGenerateContainer(ItemContainerGenerator generator, int index) {
            ScrollViewer scroller = VisualTreeUtils.FindDescendant<ScrollViewer>(this);
            if (scroller == null)
                return null;
            bool? direction = null; // up = false, down = true
            bool foundFirst = false;
            for (int i = 0, len = generator.Items.Count; i < len; i++) {
                if (generator.ContainerFromIndex(i) is TreeViewItem) {
                    if (i <= index) {
                        direction = true;
                        break;
                    }
                    else {
                        foundFirst = true;
                    }
                }
                else if (foundFirst) {
                    direction = i <= index;
                    break;
                }
            }

            TreeViewItem treeItem = null;
            if (direction == null || direction == true) { // true == down
                if (direction == null) {
                    this.PART_ScrollViewier.ScrollToVerticalOffset(0);
                    await this.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
                }

                double lastOffset = this.PART_ScrollViewier.VerticalOffset;
                while (treeItem == null && this.PART_ScrollViewier.VerticalOffset < (this.PART_ScrollViewier.ExtentHeight - this.PART_ScrollViewier.ViewportHeight)) {
                    this.PART_ScrollViewier.ScrollToVerticalOffset(this.PART_ScrollViewier.VerticalOffset + (this.PART_ScrollViewier.ViewportHeight / 2d));
                    treeItem = await this.Dispatcher.InvokeAsync(() => generator.ContainerFromIndex(index) as TreeViewItem, DispatcherPriority.Render);
                    double newOffset = this.PART_ScrollViewier.VerticalOffset;
                    if (Math.Abs(lastOffset - newOffset) < 0.1d) { // just in case we get stuck in an infinite loop
                        break;
                    }

                    lastOffset = newOffset;
                }
            }
            else { // up
                while (treeItem == null && this.PART_ScrollViewier.VerticalOffset > 0d) {
                    this.PART_ScrollViewier.ScrollToVerticalOffset(Math.Max(this.PART_ScrollViewier.VerticalOffset - (this.PART_ScrollViewier.ViewportHeight / 2d), 0));
                    treeItem = await this.Dispatcher.InvokeAsync(() => generator.ContainerFromIndex(index) as TreeViewItem, DispatcherPriority.Render);
                }
            }

            return treeItem;
        }

        /// <summary>
        /// Navigates to the last item in the list, where element 0 is expected to be the "root" item in the
        /// tree (which isn't typically even in the tree, only the items source is bound)
        /// </summary>
        /// <param name="root"></param>
        /// <param name="rootHierarchy"></param>
        /// <returns></returns>
        public async Task<bool> NavigateUsingRootPathAsync(ItemContainerGenerator root, List<BaseTreeItemViewModel> rootHierarchy) {
            if (this.IsNavigating) {
                return false;
            }

            try {
                this.IsNavigating = true;
                TreeViewItem containerItem = null;
                ItemContainerGenerator generator = root;
                for (int i = 1; i < rootHierarchy.Count; i++) {
                    BaseTreeItemViewModel item = rootHierarchy[i];
                    TreeViewItem container = BaseViewModel.GetInternalData<TreeViewItem>(item, BaseViewModelControlKey);
                    if (container == null) {
                        int index = rootHierarchy[i - 1].Children.IndexOf(item);
                        if (index == -1) {
                            return false;
                        }

                        if (containerItem != null) {
                            containerItem.IsExpanded = true;
                            containerItem.UpdateLayout();
                            // containerItem.BringIntoView();
                        }

                        container = await this.Dispatcher.InvokeAsync(() => generator.ContainerFromIndex(index) as TreeViewItem, DispatcherPriority.Background);
                        if (container == null) {
                            if ((container = await this.YuckyGenerateContainer(generator, index)) == null) {
                                return false;
                            }
                        }
                    }

                    containerItem = container;
                    generator = container.ItemContainerGenerator;
                }

                if (rootHierarchy.Count < 1)
                    return true;
                if (containerItem == null)
                    return false;

                containerItem.IsSelected = true;
                containerItem.BringIntoView();
                return true;
            }
            finally {
                this.IsNavigating = false;
            }
        }

        public static TreeViewItem ContainerFromItemRecursive(ItemContainerGenerator root, object item) {
            if (root == null)
                return null;

            if (root.ContainerFromItem(item) is TreeViewItem treeViewItem)
                return treeViewItem;
            foreach (object subItem in root.Items) {
                treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem;
                TreeViewItem search = ContainerFromItemRecursive(treeViewItem?.ItemContainerGenerator, item);
                if (search != null)
                    return search;
            }

            return null;
        }

        public static bool AccumulateItemContainerGeneratorChainForItem(ItemContainerGenerator root, object item, IList<ItemContainerGenerator> generators) {
            if (root.ContainerFromItem(item) is TreeViewItem treeViewItem) {
                generators.Add(root);
                return true;
            }

            foreach (object subItem in root.Items) {
                if (ReferenceEquals(subItem, item) || (subItem != null && subItem.Equals(item))) {
                    generators.Add(root);
                    return true;
                }

                if ((treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem) == null) {
                    continue;
                }

                if (AccumulateItemContainerGeneratorChainForItem(treeViewItem.ItemContainerGenerator, item, generators)) {
                    return true;
                }
            }

            return false;
        }

        public static TreeViewItem GetAndGenerateContainerUntilItemFound(UIElement rootElement, ItemContainerGenerator root, object item) {
            if (root == null) {
                return null;
            }

            if (root.ContainerFromItem(item) is TreeViewItem treeViewItem)
                return treeViewItem;

            foreach (object subItem in root.Items) {
                if ((treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem) == null) {
                    rootElement.UpdateLayout();
                    GenerateChildren(root);
                    rootElement.UpdateLayout();
                    if ((treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem) == null) {
                        continue;
                    }
                }

                if (ReferenceEquals(subItem, item) || (subItem != null && subItem.Equals(item))) {
                    return treeViewItem;
                }

                TreeViewItem search = GetAndGenerateContainerUntilItemFound(treeViewItem, treeViewItem.ItemContainerGenerator, item);
                if (search != null)
                    return search;
            }

            return null;

            // if (root.ContainerFromItem(item) is TreeViewItem treeViewItem)
            //     return treeViewItem;
            // foreach (object subItem in root.Items) {
            //     if ((treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem) != null) {
            //         TreeViewItem search = ContainerFromItemRecursive(treeViewItem.ItemContainerGenerator, item);
            //         if (search != null)
            //             return search;
            //     }
            // }
            // return null;
        }

        public static void GenerateChildren(IItemContainerGenerator generator) {
            using (generator.StartAt(new GeneratorPosition(-1, 0), GeneratorDirection.Forward)) {
                while (generator.GenerateNext() is UIElement next) {
                    generator.PrepareItemContainer(next);
                }
            }
        }

        BaseTreeItemViewModel IExtendedTree.GetSelectedItem() {
            return this.SelectedItem as BaseTreeItemViewModel;
        }

        public bool? IsItemExpanded(BaseTreeItemViewModel item) {
            if (item != null && BaseViewModel.TryGetInternalData(item, BaseViewModelControlKey, out TreeViewItem treeItem)) {
                return treeItem.IsExpanded;
            }

            return null;
        }

        public async Task<bool> NavigateAsync(IEnumerable<BaseTreeItemViewModel> items) {
            if (this.IsNavigating) {
                return false;
            }

            return await this.NavigateUsingRootPathAsync(this.ItemContainerGenerator, items.ToList());
        }
    }
}