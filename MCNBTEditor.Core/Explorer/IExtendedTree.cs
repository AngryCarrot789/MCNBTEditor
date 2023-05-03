using System.Collections.Generic;
using System.Threading.Tasks;

namespace MCNBTEditor.Core.Explorer {
    public interface IExtendedTree {
        /// <summary>
        /// Whether an async navigation is currently being processed
        /// </summary>
        bool IsNavigating { get; }

        BaseTreeItemViewModel GetSelectedItem();

        bool? IsItemExpanded(BaseTreeItemViewModel item);

        Task<bool> NavigateAsync(IEnumerable<BaseTreeItemViewModel> items);

        Task<bool> NavigateToItemAsync(BaseTreeItemViewModel item);

        event SelectionChangedEventHandler<BaseTreeItemViewModel> SelectionChanged;
    }
}