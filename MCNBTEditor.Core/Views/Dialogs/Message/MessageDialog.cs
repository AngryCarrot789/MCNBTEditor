using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MCNBTEditor.Core.Utils;

namespace MCNBTEditor.Core.Views.Dialogs.Message {
    /// <summary>
    /// A helper view model for managing message dialogs that can have multiple buttons
    /// </summary>
    public class MessageDialog : BaseDialogViewModel {
        private string titlebar;
        private string header;
        private string message;
        private bool showAlwaysUseNextResultOption;
        private bool isAlwaysUseNextResultChecked;
        private bool canShowAlwaysUseNextResultForCurrentQueueOption;
        private bool isAlwaysUseNextResultForCurrentQueueChecked;
        private string automaticResult;
        private bool allowNullButtonActionForAutoResult;
        private string defaultResult;
        private readonly ObservableCollectionEx<DialogButton> buttons;

        /// <summary>
        /// Whether or not this dialog's core behaviour is locked or not. The message and caption can still be modified, but pretty
        /// much everything else cannot, unless marked as not read only
        /// <para>
        /// This is mainly just used to prevent accidentally modifying a "template" instance, because templates should
        /// be cloned (via <see cref="Clone"/>) and then furtuer modified
        /// </para>
        /// </summary>
        public bool IsReadOnly { get; private set; }

        public string Titlebar {
            get => this.titlebar;
            set => this.RaisePropertyChanged(ref this.titlebar, value);
        }

        public string Header {
            get => this.header;
            set => this.RaisePropertyChanged(ref this.header, value);
        }

        public string Message {
            get => this.message;
            set => this.RaisePropertyChanged(ref this.message, value);
        }

        public bool ShowAlwaysUseNextResultOption {
            get => this.showAlwaysUseNextResultOption;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.showAlwaysUseNextResultOption, value);
                if (!value && this.IsAlwaysUseNextResultChecked) {
                    this.IsAlwaysUseNextResultChecked = false;
                }
            }
        }

        public bool IsAlwaysUseNextResultChecked { // dialog will show "Always use this option"
            get => this.isAlwaysUseNextResultChecked;
            set {
                this.EnsureNotReadOnly();
                this.isAlwaysUseNextResultChecked = value && this.ShowAlwaysUseNextResultOption;
                this.RaisePropertyChanged();
                if (!this.isAlwaysUseNextResultChecked && this.IsAlwaysUseNextResultForCurrentQueueChecked) {
                    this.IsAlwaysUseNextResultForCurrentQueueChecked = false;
                }

                foreach (DialogButton button in this.Buttons) {
                    button.UpdateState();
                }
            }
        }

        public bool CanShowAlwaysUseNextResultForCurrentQueueOption {
            get => this.canShowAlwaysUseNextResultForCurrentQueueOption;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.canShowAlwaysUseNextResultForCurrentQueueOption, value);
                if (!value && this.IsAlwaysUseNextResultForCurrentQueueChecked) {
                    this.IsAlwaysUseNextResultForCurrentQueueChecked = false;
                }
            }
        }

        public bool IsAlwaysUseNextResultForCurrentQueueChecked {
            get => this.isAlwaysUseNextResultForCurrentQueueChecked;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.isAlwaysUseNextResultForCurrentQueueChecked, value && this.CanShowAlwaysUseNextResultForCurrentQueueOption);

                foreach (DialogButton button in this.Buttons) {
                    button.UpdateState();
                }
            }
        }

        public string AutomaticResult {
            get => this.automaticResult;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.automaticResult, value);
            }
        }

        // There is literally no point in this... not really at least
        // /// <summary>
        // /// Whether or not to allow the automatic result to be set when a button's action is null
        // /// </summary>
        // public bool AllowNullButtonActionForAutoResult {
        //     get => this.allowNullButtonActionForAutoResult;
        //     set {
        //         this.EnsureNotReadOnly();
        //         this.allowNullButtonActionForAutoResult = value;
        //     }
        // }

        /// <summary>
        /// This dialog's default result, which is the result used if the dialog closed without a button (e.g. clicking esc or some dodgy Win32 usage)
        /// </summary>
        public string DefaultResult {
            get => this.defaultResult;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.defaultResult, value);
            }
        }

        /// <summary>
        /// The buttons for this message dialog. This list is ordered left to right, meaning that the first element will be on the very left.
        /// <para>
        /// On windows, the UI list is typically aligned to the right, meaning the last element is on the very right. So to have the typical
        /// Yes/No/Cancel buttons, you would add them to this list in that exact order
        /// </para>
        /// </summary>
        public ReadOnlyObservableCollection<DialogButton> Buttons { get; }

        public bool HasNoButtons => this.Buttons.Count < 1;

        public bool HasButtons => this.Buttons.Count > 0;

        private DialogButton lastClickedButton;

        public MessageDialog(IEnumerable<DialogButton> buttons = null) {
            this.buttons = new ObservableCollectionEx<DialogButton>(buttons ?? Enumerable.Empty<DialogButton>());
            this.Buttons = new ReadOnlyObservableCollection<DialogButton>(this.buttons);
            this.buttons.CollectionChanged += (sender, args) => {
                // Shouldn't be read only, because buttons is private
                this.RaisePropertyChanged(nameof(this.HasNoButtons));
                this.RaisePropertyChanged(nameof(this.HasButtons));
            };
        }

        public Task<string> ShowAsync(string titlebar, string header, string message) {
            if (this.AutomaticResult != null) {
                return Task.FromResult(this.AutomaticResult);
            }

            if (titlebar != null)
                this.Titlebar = titlebar;
            if (header != null)
                this.Header = header;
            if  (message != null)
                this.Message = message;
            return this.ShowAsync();
        }

        public Task<string> ShowAsync(string titlebar, string message) {
            return this.ShowAsync(titlebar, null, message);
        }

        public Task<string> ShowAsync(string message) {
            return this.ShowAsync(null, message);
        }

        public async Task<string> ShowAsync() {
            if (this.AutomaticResult != null) {
                return this.AutomaticResult;
            }

            string output;
            bool? result = await IoC.MessageDialogs.ShowDialogAsync(this);
            if (result == true && this.lastClickedButton != null) {
                output = this.lastClickedButton.ActionType;
                if (output != null && this.IsAlwaysUseNextResultChecked) { // (output != null || this.AllowNullButtonActionForAutoResult)
                    this.AutomaticResult = output;
                }
            }
            else {
                output = this.DefaultResult;
            }

            this.lastClickedButton = null;
            return output;
        }

        public async Task OnButtonClicked(DialogButton button) {
            this.lastClickedButton = button;
            await this.Dialog.CloseDialogAsync(button?.ActionType != null);
        }

        public DialogButton InsertButton(int index, string msg, string actionType, bool canUseAsAutoResult) {
            this.EnsureNotReadOnly();
            DialogButton button = new DialogButton(this, actionType, msg, canUseAsAutoResult);
            this.buttons.Insert(index, button);
            return button;
        }

        public DialogButton ReplaceButton(int index, string msg, string actionType, bool canUseAsAutoResult) {
            this.EnsureNotReadOnly();
            this.buttons.RemoveAt(index);
            return this.InsertButton(index, msg, actionType, canUseAsAutoResult);
        }

        public DialogButton AddButton(string msg, string actionType, bool canUseAsAutoResult) {
            return this.InsertButton(this.buttons.Count, msg, actionType, canUseAsAutoResult);
        }

        public void AddButton(DialogButton button) {
            this.EnsureNotReadOnly();
            this.buttons.Add(button);
        }

        public void InsertButton(int index, DialogButton button) {
            this.EnsureNotReadOnly();
            this.buttons.Insert(index, button);
        }

        public void AddButtons(params DialogButton[] buttons) {
            this.EnsureNotReadOnly();
            foreach (DialogButton button in buttons) {
                this.buttons.Add(button);
            }
        }

        public DialogButton GetButtonAt(int index) {
            return this.buttons[index];
        }

        public DialogButton GetButtonById(string id) {
            return this.buttons.First(x => x.ActionType != null && x.ActionType == id);
        }

        public DialogButton RemoveButtonAt(int index) {
            this.EnsureNotReadOnly();
            DialogButton button = this.buttons[index];
            this.buttons.RemoveAt(index);
            return button;
        }

        public DialogButton RemoveButtonById(string id) {
            int index = this.buttons.FindIndexOf(x => x.ActionType == id);
            if (index == -1) {
                return null;
            }

            DialogButton button = this.buttons[index];
            this.buttons.RemoveAt(index);
            return button;
        }

        /// <summary>
        /// Creates a clone of this dialog. The returned instance will not be read only, allowing it to be further modified
        /// </summary>
        /// <returns></returns>
        public virtual MessageDialog Clone() {
            MessageDialog dialog = new MessageDialog() {
                Titlebar = this.Titlebar,
                Header = this.Header,
                Message = this.Message,
                AutomaticResult = this.AutomaticResult,
                IsAlwaysUseNextResultChecked = this.IsAlwaysUseNextResultChecked,
                ShowAlwaysUseNextResultOption = this.ShowAlwaysUseNextResultOption
            };
            foreach (DialogButton button in this.Buttons)
                dialog.buttons.Add(button.Clone(dialog));
            return dialog;
        }

        public void MarkReadOnly() {
            this.IsReadOnly = true;
        }

        protected void EnsureNotReadOnly() {
            if (this.IsReadOnly) {
                throw new InvalidOperationException("This message dialog instance is read-only. Create a clone to modify it");
            }
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
            private readonly bool isReadOnly;

            public MessageDialog Dialog { get; }

            public MessageDialogUsage(MessageDialog dialog) {
                this.Dialog = dialog;
                if (dialog.IsReadOnly) {
                    this.isReadOnly = true;
                    return;
                }

                // Store the current dialog state
                this.oldAlwaysUseNextResult = dialog.IsAlwaysUseNextResultChecked;
                this.oldShowAlwaysUseNextResult = this.Dialog.ShowAlwaysUseNextResultOption;
                this.oldCanShowAlwaysUseNextResultForQueue = this.Dialog.CanShowAlwaysUseNextResultForCurrentQueueOption;
                this.oldButtons = dialog.Buttons.ToList();
                this.oldAutoResult = dialog.AutomaticResult;
                this.oldDefaultResult = dialog.DefaultResult;
            }

            /// <summary>
            /// Restores the dialog to its old state
            /// </summary>
            public void Dispose() {
                if (this.isReadOnly) {
                    return;
                }

                this.Dialog.buttons.ClearAndAddRange(this.oldButtons);
                if (this.Dialog.IsAlwaysUseNextResultForCurrentQueueChecked) {
                    // We are only applying data for the current queue, and that queue is
                    // finished now, so, revert the data
                    this.Dialog.IsAlwaysUseNextResultForCurrentQueueChecked = false; // this should always be false when the usage instance is created
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
                this.Dialog.IsAlwaysUseNextResultChecked = this.oldAlwaysUseNextResult;
                this.Dialog.DefaultResult = this.oldDefaultResult;
            }
        }
    }
}