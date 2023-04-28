using System.Collections.Generic;
using System.Threading.Tasks;

namespace MCNBTEditor.Core.Explorer {
    public interface IExtendedTree {
        BaseTreeItemViewModel GetSelectedItem();
        bool IsItemExpanded(BaseTreeItemViewModel item);

        void SetExpanded(BaseTreeItemViewModel nbt);
        bool IsExpanded(BaseTreeItemViewModel nbt);

        Task<bool> RepeatExpandHierarchyFromRootAsync(IEnumerable<BaseTreeItemViewModel> items, bool select = true);
        Task ExpandItemHierarchy(BaseTreeItemViewModel item);
        bool ExpandHierarchyFromRoot(IEnumerable<BaseTreeItemViewModel> items, bool select = true);

        event SelectionChangedEventHandler<BaseTreeItemViewModel> SelectionChanged;
    }
}