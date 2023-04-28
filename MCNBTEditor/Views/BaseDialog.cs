using System.Threading.Tasks;
using System.Windows;
using MCNBTEditor.Core.Views.Dialogs;
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

        public async Task CloseDialogAsync(bool result) {
            await this.Dispatcher.InvokeAsync(() => this.CloseDialog(result));
        }
    }
}