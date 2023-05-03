using System.Windows;
using System.Windows.Controls;
using MCNBTEditor.Core;
using MCNBTEditor.Core.Explorer;

namespace MCNBTEditor.Controls {
    public class ExtendedTreeViewItem : TreeViewItem {
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

        protected override DependencyObject GetContainerForItemOverride() {
            return new ExtendedTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is ExtendedTreeViewItem;
        }
    }
}