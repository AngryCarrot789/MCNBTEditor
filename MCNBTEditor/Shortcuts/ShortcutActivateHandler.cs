using System.Threading.Tasks;
using MCNBTEditor.Core.Shortcuts.Managing;

namespace MCNBTEditor.Shortcuts {
    public delegate Task<bool> ShortcutActivateHandler(ShortcutProcessor processor, GroupedShortcut shortcut);
}