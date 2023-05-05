using MCNBTEditor.Core.Actions.Contexts;
using MCNBTEditor.Core.Explorer.NBT;

namespace MCNBTEditor.Core.Explorer.Actions {
    public static class NBTActions {
        public static bool FindTag(IDataContext context, out BaseTagViewModel tag) {
            return context.TryGetContext(out tag);
        }
    }
}