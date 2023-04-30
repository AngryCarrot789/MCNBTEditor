using System.Collections.Generic;
using System.Threading.Tasks;

namespace MCNBTEditor.Core.Explorer.Dialog {
    public interface IItemSelectorService {
        Task<BaseTreeItemViewModel> SelectItemAsync(IEnumerable<BaseTreeItemViewModel> items, string title, string message);
    }
}