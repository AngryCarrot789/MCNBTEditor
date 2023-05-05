using System.Windows.Input;

namespace MCNBTEditor.Core.Shortcuts {
    public interface IShortcutToCommand {
        ICommand GetCommandForShortcut(string shortcutId);
    }
}