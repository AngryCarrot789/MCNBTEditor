using System.Collections.Generic;
using System.Linq;
using MCNBTEditor.Core.Actions.Contexts;
using MCNBTEditor.Core.Explorer.NBT;

namespace MCNBTEditor.Core.Explorer.Actions {
    public static class NBTActionUtils {
        public static bool FindTag(IDataContext context, out BaseTagViewModel tag) {
            return context.TryGetContext(out tag);
        }

        public static bool HasSelectedItems(IDataContext context) {
            return context.HasContext<BaseTreeItemViewModel>() || context.HasContext<IExtendedList>();
        }

        public static bool GetSelectedItems(IDataContext context, out IEnumerable<BaseTreeItemViewModel> tags) {
            BaseTreeItemViewModel firstTag = context.GetContext<BaseTreeItemViewModel>();
            if (context.TryGetContext(out IExtendedList list)) {
                List<BaseTreeItemViewModel> selected = list.SelectedItems.ToList();
                if (firstTag == null || selected.Contains(firstTag)) {
                    tags = selected;
                    return true;
                }
            }

            if (firstTag != null) {
                tags = new List<BaseTreeItemViewModel>() {firstTag};
                return true;
            }

            tags = null;
            return false;
        }
    }
}