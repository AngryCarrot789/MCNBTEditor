using System.Collections.Generic;
using System.Threading.Tasks;

namespace MCNBTEditor.Core.Explorer {
    public interface IExtendedTree {
        BaseTreeItemViewModel GetSelectedItem();
        bool IsItemExpanded(BaseTreeItemViewModel item);

        void SetExpanded(BaseTreeItemViewModel nbt);
        bool IsExpanded(BaseTreeItemViewModel nbt);

        /// <summary>
        /// Expands the given collection of items (in which the first item is the root item) all the way down to the last item
        /// <para>
        /// The repeat functionality will repeat it N number of items due to WPF item container generation issues
        /// </para>
        /// <para>
        /// This is a fairly expensive operation!
        /// </para>
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        Task<bool> RepeatExpandHierarchyFromRootAsync(IEnumerable<BaseTreeItemViewModel> items);
        Task ExpandItemSubTree(BaseTreeItemViewModel item);
        bool ExpandHierarchyFromRoot(IEnumerable<BaseTreeItemViewModel> items, bool select = true);

        event SelectionChangedEventHandler<BaseTreeItemViewModel> SelectionChanged;
    }
}