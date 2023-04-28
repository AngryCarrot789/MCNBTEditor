using MCNBTEditor.Core.Shortcuts.Dialogs;
using MCNBTEditor.Core.Shortcuts.Inputs;

namespace MCNBTEditor.Shortcuts.Dialogs {
    public class KeyboardDialogService : IKeyboardDialogService {
        public KeyStroke? ShowGetKeyStrokeDialog() {
            KeyStrokeInputWindow window = new KeyStrokeInputWindow();
            if (window.ShowDialog() != true || window.Stroke.Equals(default)) {
                return null;
            }
            else {
                return window.Stroke;
            }
        }
    }
}