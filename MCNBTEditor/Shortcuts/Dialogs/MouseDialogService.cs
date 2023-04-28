using MCNBTEditor.Core.Shortcuts.Dialogs;
using MCNBTEditor.Core.Shortcuts.Inputs;

namespace MCNBTEditor.Shortcuts.Dialogs {
    public class MouseDialogService : IMouseDialogService {
        public MouseStroke? ShowGetMouseStrokeDialog() {
            MouseStrokeInputWindow window = new MouseStrokeInputWindow();
            if (window.ShowDialog() != true || window.Stroke.Equals(default)) {
                return null;
            }
            else {
                return window.Stroke;
            }
        }
    }
}