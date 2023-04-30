using System.Collections.Generic;
using System.Threading.Tasks;
using MCNBTEditor.Core.Views.ViewModels;

namespace MCNBTEditor.Core.Views.Dialogs {
    public class BaseConfirmableDialogViewModel : BaseDialogViewModel, IErrorInfoHandler {
        protected bool HasErrors { get; private set; }

        public RelayCommand ConfirmCommand { get; }
        public RelayCommand CancelCommand { get; }

        public BaseConfirmableDialogViewModel() {
            this.ConfirmCommand = new RelayCommand(async () => await this.ConfirmAction(), this.CanConfirm);
            this.CancelCommand = new RelayCommand(async () => await this.CancelAction());
        }

        public BaseConfirmableDialogViewModel(IDialog dialog) : this() {
            this.Dialog = dialog;
        }

        public virtual async Task ConfirmAction() {
            if (await this.CanConfirmAsync()) {
                await this.Dialog.CloseDialogAsync(true);
                await this.OnDialogClosedAsync();
            }
        }

        public virtual async Task CancelAction() {
            if (await this.CanCancelAsync()) {
                await this.Dialog.CloseDialogAsync(false);
                await this.OnDialogClosedAsync();
            }
        }

        public virtual void OnErrorsUpdated(Dictionary<string, object> errors) {
            this.HasErrors = errors.Count > 0;
            this.ConfirmCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Called just before the confirm action is executed, to check if this
        /// dialog actually can close. If this returns true, the dialog closes
        /// <para>
        /// This method can also be used to set some final state in the dialog
        /// </para>
        /// </summary>
        public virtual Task<bool> CanConfirmAsync() {
            return Task.FromResult(this.CanConfirm());
        }

        protected virtual bool CanConfirm() {
            return !this.HasErrors;
        }

        /// <summary>
        /// Called just before the cancel action is executed, to check if this
        /// dialog actually can close. This should never really return anything except true,
        /// otherwise the user will be unable to close the dialog (except for clicking the X button)
        /// </summary>
        public virtual Task<bool> CanCancelAsync() {
            return Task.FromResult(true);
        }

        public virtual Task OnDialogClosedAsync() {
            return Task.CompletedTask;
        }
    }
}