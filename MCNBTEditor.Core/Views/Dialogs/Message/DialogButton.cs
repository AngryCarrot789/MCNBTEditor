using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCNBTEditor.Core.Views.Dialogs.Message {
    public class DialogButton : BaseDialogButton {
        /// <summary>
        /// A command that fires when this dialog is clicked
        /// </summary>
        public AsyncRelayCommand Command { get; }

        /// <summary>
        /// The action that clicking the command results in
        /// </summary>
        public string ActionType { get; }

        private bool canUseAsAutomaticResult = true;

        public bool CanUseAsAutomaticResult {
            get => this.canUseAsAutomaticResult;
            set => this.RaisePropertyChanged(ref this.canUseAsAutomaticResult, value);
        }

        public bool IsEnabled {
            get => this.Command.IsEnabled;
            set {
                this.Command.IsEnabled = value;
                this.RaisePropertyChanged();
            }
        }

        public DialogButton(MessageDialog dialog, string actionType, string text, bool canUseAsAutomaticResult) : base(dialog, text) {
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
    }
}