using System.Collections.Generic;

namespace MCNBTEditor.Core.Explorer {
    public interface IExtendedList {
        IEnumerable<BaseTreeItemViewModel> SelectedItems { get; }

        event SelectionChangedEventHandler<IEnumerable<BaseTreeItemViewModel>> SelectionChanged;
    }
}