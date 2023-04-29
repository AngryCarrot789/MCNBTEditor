using System.Threading.Tasks;
using MCNBTEditor.Core.Views.Dialogs.Message;

namespace MCNBTEditor.Core.Utils {
    public static class ClipboardUtils {
        public static async Task<bool> SetClipboardOrShowErrorDialog(string text) {
            if (IoC.Clipboard == null) {
                await MessageDialogs.ClipboardUnavailableDialog.ShowAsync("No clipboard", "Clipboard is unavailable.\n" + text);
                return false;
            }
            else {
                IoC.Clipboard.ReadableText = text;
                return true;
            }
        }
    }
}