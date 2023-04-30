using System.Collections.Generic;
using System.Threading.Tasks;

namespace MCNBTEditor.Core.Explorer.Dialogs {
    public interface IItemSelectorService {
        Task<BaseTreeItemViewModel> SelectItemAsync(IEnumerable<BaseTreeItemViewModel> items, string title, string message);
    }
}