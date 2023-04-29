using System.Threading.Tasks;
using System.Windows;
using MCNBTEditor.Core.Views.Dialogs;
using MCNBTEditor.Utils;
using MCNBTEditor.Views.FilePicking;

namespace MCNBTEditor.Views {
    public class BaseDialog : BaseWindowCore, IDialog {
        public BaseDialog() {
            this.Owner = FolderPicker.GetCurrentActiveWindow();
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public void CloseDialog(bool result) {
            this.DialogResult = result;
            this.Close();
        }

        public Task CloseDialogAsync(bool result) {
            return DispatcherUtils.InvokeAsync(this.Dispatcher, () => this.CloseDialog(result));
        }
    }
}