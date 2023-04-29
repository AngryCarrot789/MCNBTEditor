using System;
using System.Threading.Tasks;

namespace MCNBTEditor.Core.Views.Dialogs.Message {
    public class DialogButton : BaseViewModel {
        /// <summary>
        /// The dialog that owns this button
        /// </summary>
        public MessageDialog Dialog { get; }

        /// <summary>
        /// A command that fires when this dialog is clicked
        /// </summary>
        public AsyncRelayCommand Command { get; }

        /// <summary>
        /// The action that clicking the command results in
        /// </summary>
        public string ActionType { get; }

        private string text;
        public string Text {
            get => this.text;
            set => this.RaisePropertyChanged(ref this.text, value);
        }

        private string toolTip;
        public string ToolTip {
            get => this.toolTip;
            set => this.RaisePropertyChanged(ref this.toolTip, value);
        }

        public bool IsEnabled {
            get => this.Command.IsEnabled;
            set {
                this.Command.IsEnabled = value;
                this.RaisePropertyChanged();
            }
        }

        private bool canUseAsAutomaticResult;
        public bool CanUseAsAutomaticResult {
            get => this.canUseAsAutomaticResult;
            set => this.RaisePropertyChanged(ref this.canUseAsAutomaticResult, value);
        }

        public DialogButton(MessageDialog dialog, string actionType, string text, bool canUseAsAutomaticResult) {
            this.Dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            this.text = text ?? "";
            this.ActionType = actionType;
            this.Command = new AsyncRelayCommand(this.OnClickedAction);
            this.canUseAsAutomaticResult = canUseAsAutomaticResult;
        }

        public virtual Task OnClickedAction() {
            if (this.IsEnabled) {
                return this.Dialog.OnButtonClicked(this);
            }

            return Task.CompletedTask;
        }

        public virtual DialogButton Clone(MessageDialog dialog) {
            return new DialogButton(dialog, this.ActionType, this.Text, this.CanUseAsAutomaticResult) {
                IsEnabled = this.IsEnabled, ToolTip = this.ToolTip
            };
        }

        public void UpdateState() {
            if (this.Dialog.IsAlwaysUseNextResultChecked || this.Dialog.IsAlwaysUseNextResultForCurrentQueueChecked) {
                this.IsEnabled = this.CanUseAsAutomaticResult;
            }
            else {
                this.IsEnabled = true;
            }
        }
    }
}