using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MCNBTEditor.Core.Views.Dialogs;
using MCNBTEditor.Utils;
using MCNBTEditor.Views.FilePicking;

namespace MCNBTEditor.Views {
    public class BaseDialog : WindowViewBase, IDialog {
        public BaseDialog() {
            Window owner = FolderPicker.GetCurrentActiveWindow();
            if (owner != this && owner.Owner != this) {
                this.Owner = owner;
            }

            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (!e.Handled) {
                switch (e.Key) {
                    case Key.Escape:
                        this.DialogResult = false;
                        break;
                    default:
                        return;
                }

                e.Handled = true;
                this.Close();
            }
        }

        public void CloseDialog(bool result) {
            this.DialogResult = result;
            this.Close();
        }

        public Task CloseDialogAsync(bool result) {
            this.DialogResult = result;
            return this.CloseAsync();
        }
    }
}