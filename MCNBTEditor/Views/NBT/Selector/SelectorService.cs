using System.Collections.Generic;
using System.Threading.Tasks;
using MCNBTEditor.Core.Explorer;
using MCNBTEditor.Core.Explorer.Dialog;
using MCNBTEditor.Utils;

namespace MCNBTEditor.Views.NBT.Selector {
    public class SelectorService : IItemSelectorService {
        public Task<BaseTreeItemViewModel> SelectItemAsync(IEnumerable<BaseTreeItemViewModel> items, string title, string message) {
            return DispatcherUtils.InvokeAsync(() => this.SelectItem(items, title, message));
        }

        public BaseTreeItemViewModel SelectItem(IEnumerable<BaseTreeItemViewModel> items, string title, string message = "Select an item") {
            ListSelectorWindow window = new ListSelectorWindow();
            ItemSelectorViewModel vm = new ItemSelectorViewModel(items);
            vm.Title = title ?? "Select an item";
            vm.Message = string.IsNullOrWhiteSpace(message) ? null : message;
            window.DataContext = vm;
            vm.Dialog = window;
            return window.ShowDialog() == true ? vm.SelectedItem : null;
        }
    }
}