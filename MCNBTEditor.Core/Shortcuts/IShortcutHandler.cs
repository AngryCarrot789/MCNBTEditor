using System.Threading.Tasks;
using MCNBTEditor.Core.Shortcuts.Managing;

namespace MCNBTEditor.Core.Shortcuts {
    public interface IShortcutHandler {
        Task<bool> OnShortcutActivated(ShortcutProcessor processor, GroupedShortcut shortcut);
    }
}