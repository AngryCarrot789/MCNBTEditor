using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MCNBTEditor.Core.Views.Dialogs;
using MCNBTEditor.Utils;
using MCNBTEditor.Views.FilePicking;

namespace MCNBTEditor.Views {
    public class BaseDialog : BaseWindowCore, IDialog {
        public BaseDialog() {
            Window owner = FolderPicker.GetCurrentActiveWindow();
            if (owner != this) {
                this.Owner = owner;
            }

            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            if (!e.Handled) {
                switch (e.Key) {
                    case Key.Escape:
                        this.DialogResult = false;
                        break;
                    default: return;
                }

                e.Handled = true;
                this.Close();
            }

            base.OnKeyDown(e);
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