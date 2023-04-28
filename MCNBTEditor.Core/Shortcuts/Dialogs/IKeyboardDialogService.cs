using MCNBTEditor.Core.Shortcuts.Inputs;

namespace MCNBTEditor.Core.Shortcuts.Dialogs {
    public interface IKeyboardDialogService {
        KeyStroke? ShowGetKeyStrokeDialog();
    }
}