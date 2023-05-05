using System.Collections.Generic;

namespace MCNBTEditor.Core.Explorer {
    public interface IExtendedList {
        IEnumerable<BaseTreeItemViewModel> SelectedItems { get; }

        int Count { get; }

        event SelectionChangedEventHandler<IEnumerable<BaseTreeItemViewModel>> SelectionChanged;

        int IndexOf(BaseTreeItemViewModel item);
    }
}