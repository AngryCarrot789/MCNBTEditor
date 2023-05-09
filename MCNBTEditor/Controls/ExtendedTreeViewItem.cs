using System.Windows;
using System.Windows.Controls;
using MCNBTEditor.Core;
using MCNBTEditor.Core.Explorer;

namespace MCNBTEditor.Controls {
    public class ExtendedTreeViewItem : TreeViewItem {
        public ExtendedTreeViewItem() {
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new ExtendedTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is ExtendedTreeViewItem;
        }

        // Storing the elements in the view model as "internal data" is just an optimisation
        // so that constantly looking up the element by view model via the ICG isn't required...
        // although virtualization will pretty much ruin this because ClearContainerForItemOverride will
        // just get called as soon as the item is offscreen so :/

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);
            if (item is BaseTreeItemViewModel treeItem) {
                BaseViewModel.SetInternalData(treeItem, ExtendedTreeView.BaseViewModelControlKey, element);
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);
            if (item is BaseTreeItemViewModel treeItem) {
                BaseViewModel.ClearInternalData(treeItem, ExtendedTreeView.BaseViewModelControlKey);
            }
        }
    }
}