using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MCNBTEditor.Core.Utils;
using MCNBTEditor.Core.Views.Dialogs.Modal;

namespace MCNBTEditor.Core.Views.Dialogs.Message {
    /// <summary>
    /// A helper view model for managing message dialogs that can have multiple buttons
    /// </summary>
    public class MessageDialog : BaseDialogExViewModel {
        public MessageDialog(string primaryResult = null, string defaultResult = null) : base(primaryResult, defaultResult) {
        }

        /// <summary>
        /// Creates a clone of this dialog. The returned instance will not be read only, allowing it to be further modified
        /// </summary>
        /// <returns></returns>
        public MessageDialog Clone() {
            MessageDialog dialog = new MessageDialog() {
                titlebar = this.titlebar,
                header = this.header,
                message = this.message,
                automaticResult = this.automaticResult,
                ShowAlwaysUseNextResultOption = this.ShowAlwaysUseNextResultOption,
                IsAlwaysUseThisOptionChecked = this.IsAlwaysUseThisOptionChecked,
                primaryResult = this.primaryResult,
                defaultResult = this.defaultResult
            };

            foreach (DialogButton button in this.buttons)
                dialog.buttons.Add(button.Clone(dialog));
            return dialog;
        }


        protected override Task<bool?> ShowDialogAsync() {
            return IoC.MessageDialogs.ShowDialogAsync(this);
        }

        public override BaseDialogExViewModel CloneCore() {
            return this.Clone();
        }

        /// <summary>
        /// Creates a disposable usage/state of this message dialog which, if <see cref="IsAlwaysUseNextResultForCurrentQueueChecked"/> is true,
        /// allows the buttons and auto-result to be restored once the usage instance is disposed
        /// <para>
        /// This only needs to be used if you intend on modifying the state of the current <see cref="MessageDialog"/> during some
        /// sort of "queue/collection based" work, and want to restore those changes once you're finished
        /// <para>
        /// An example is loading files; you realistically need to use this in order to restore the <see cref="AutomaticResult"/> to the previous
        /// value if <see cref="IsAlwaysUseNextResultForCurrentQueueChecked"/> is true)
        /// </para>
        /// </para>
        /// </summary>
        /// <returns></returns>
        public MessageDialogUsage Use() {
            return new MessageDialogUsage(this);
        }

        public class MessageDialogUsage : IDisposable {
            private readonly List<DialogButton> oldButtons;
            private readonly string oldAutoResult;
            private readonly bool oldAlwaysUseNextResult;
            private readonly bool oldShowAlwaysUseNextResult;
            private readonly bool oldCanShowAlwaysUseNextResultForQueue;
            private readonly string oldDefaultResult;
            private readonly string oldPrimaryResult;
            private readonly bool wasReadOnly;

            public MessageDialog Dialog { get; }

            public MessageDialogUsage(MessageDialog dialog) {
                this.Dialog = dialog;
                if (dialog.IsReadOnly) {
                    this.wasReadOnly = true;
                    dialog.IsReadOnly = false;
                }

                // Store the current dialog state
                this.oldAlwaysUseNextResult = dialog.IsAlwaysUseThisOptionChecked;
                this.oldShowAlwaysUseNextResult = this.Dialog.ShowAlwaysUseNextResultOption;
                this.oldCanShowAlwaysUseNextResultForQueue = this.Dialog.CanShowAlwaysUseNextResultForCurrentQueueOption;
                this.oldButtons = dialog.Buttons.ToList();
                this.oldAutoResult = dialog.AutomaticResult;
                this.oldDefaultResult = dialog.DefaultResult;
                this.oldPrimaryResult = dialog.PrimaryResult;
            }

            /// <summary>
            /// Restores the dialog to its old state
            /// </summary>
            public void Dispose() {
                this.Dialog.buttons.ClearAndAddRange(this.oldButtons);
                if (this.Dialog.IsAlwaysUseThisOptionForCurrentQueueChecked) {
                    // We are only applying data for the current usage and that usage is finished now, so, revert the data
                    this.Dialog.IsAlwaysUseThisOptionForCurrentQueueChecked = false; // this should always be false when the usage instance is created
                    this.Dialog.AutomaticResult = this.oldAutoResult;
                }
                else {
                    // here we handle the case where we are saving results globally for this
                    // dialog; "save these results" is checked, but
                    // "save for current queue only" is not checked
                    // -------------------------------------------------------
                    // nothing needs to be handled here
                    // -------------------------------------------------------
                }

                this.Dialog.ShowAlwaysUseNextResultOption = this.oldShowAlwaysUseNextResult;
                this.Dialog.CanShowAlwaysUseNextResultForCurrentQueueOption = this.oldCanShowAlwaysUseNextResultForQueue;
                this.Dialog.IsAlwaysUseThisOptionChecked = this.oldAlwaysUseNextResult;
                this.Dialog.DefaultResult = this.oldDefaultResult;
                this.Dialog.PrimaryResult = this.oldPrimaryResult;

                if (this.wasReadOnly) {
                    this.Dialog.IsReadOnly = true;
                }
            }
        }
    }
}