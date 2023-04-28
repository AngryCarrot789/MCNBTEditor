using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MCNBTEditor.Core.Views.Dialogs.Message {
    public class MessageDialog : BaseDialogViewModel {
        private string caption;
        public string Caption {
            get => this.caption;
            set => this.RaisePropertyChanged(ref this.caption, value);
        }

        private string message;
        public string Message {
            get => this.message;
            set => this.RaisePropertyChanged(ref this.message, value);
        }

        private bool alwaysUseNextResult;
        public bool AlwaysUseNextResult {
            get => this.alwaysUseNextResult;
            set => this.RaisePropertyChanged(ref this.alwaysUseNextResult, value);
        }

        private string automaticResult;
        public string AutomaticResult {
            get => this.automaticResult;
            set => this.RaisePropertyChanged(ref this.automaticResult, value);
        }

        private DialogButton lastClickedButton;

        public ObservableCollection<BaseDialogButton> Buttons { get; }

        public MessageDialog(IEnumerable<BaseDialogButton> buttons, string caption, string message) {
            this.Buttons = new ObservableCollection<BaseDialogButton>(buttons ?? Enumerable.Empty<BaseDialogButton>());
            this.caption = caption;
            this.message = message;
        }

        public MessageDialog(string body) : this(null, "Message", body) {

        }

        public async Task<string> ShowAsync() {
            if (this.AutomaticResult != null) {
                return this.AutomaticResult;
            }

            string output = null;
            bool? result = await IoC.MessageDialogs.ShowDialogAsync(this);
            if (result.HasValue && result.Value && this.lastClickedButton?.ActionType != null) {
                output = this.lastClickedButton.ActionType;
                if (this.AlwaysUseNextResult) {
                    this.AutomaticResult = output;
                }
            }

            this.lastClickedButton = null;
            return output;
        }

        public async Task OnButtonClicked(BaseDialogButton baseButton) {
            if (baseButton is DialogButton button) {
                this.lastClickedButton = button;
                await this.Dialog.CloseDialogAsync(button.ActionType != null);
            }
        }
    }
}