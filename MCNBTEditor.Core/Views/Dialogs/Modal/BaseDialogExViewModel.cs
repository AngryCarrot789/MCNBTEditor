using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MCNBTEditor.Core.Utils;
namespace MCNBTEditor.Core.Views.Dialogs.Modal {
    /// <summary>
    /// A helper view model for managing message dialogs that can have multiple buttons
    /// </summary>
    public abstract class BaseDialogExViewModel : BaseViewModel {
        protected string titlebar;
        protected string header;
        protected string message;
        protected bool showAlwaysUseNextResultOption;
        protected bool isAlwaysUseThisOptionChecked;
        protected bool canShowAlwaysUseNextResultForCurrentQueueOption;
        protected bool isAlwaysUseThisOptionForCurrentQueueChecked;
        protected string automaticResult;
        protected string defaultResult;
        protected string primaryResult;
        protected readonly ObservableCollectionEx<DialogButton> buttons;

        /// <summary>
        /// Whether or not this dialog's core behaviour is locked or not. The message and caption can still be modified, but pretty
        /// much everything else cannot, unless marked as not read only
        /// <para>
        /// This is mainly just used to prevent accidentally modifying a "template" instance, because templates should
        /// be cloned (via <see cref="Clone"/>) and then furtuer modified
        /// </para>
        /// </summary>
        public bool IsReadOnly { get; protected set; }

        public IDialog Dialog { get; set; }

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

        /// <summary>
        /// Whether or not to show the "always use next result option" in the GUI
        /// </summary>
        public bool ShowAlwaysUseNextResultOption { // dialog will show "Always use this option"
            get => this.showAlwaysUseNextResultOption;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.showAlwaysUseNextResultOption, value);
                if (!value && this.IsAlwaysUseThisOptionChecked) {
                    this.IsAlwaysUseThisOptionChecked = false;
                }
            }
        }

        /// <summary>
        /// Whether or not the GUI option to use the next outcome as an automatic result is checked
        /// </summary>
        [Bindable(true)]
        public bool IsAlwaysUseThisOptionChecked {
            get => this.isAlwaysUseThisOptionChecked;
            set {
                this.EnsureNotReadOnly();
                this.isAlwaysUseThisOptionChecked = value && this.ShowAlwaysUseNextResultOption;
                this.RaisePropertyChanged();
                if (!this.isAlwaysUseThisOptionChecked && this.IsAlwaysUseThisOptionForCurrentQueueChecked) {
                    this.IsAlwaysUseThisOptionForCurrentQueueChecked = false;
                }

                this.UpdateButtons();
            }
        }

        public bool CanShowAlwaysUseNextResultForCurrentQueueOption {
            get => this.canShowAlwaysUseNextResultForCurrentQueueOption;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.canShowAlwaysUseNextResultForCurrentQueueOption, value);
                if (!value && this.IsAlwaysUseThisOptionForCurrentQueueChecked) {
                    this.IsAlwaysUseThisOptionForCurrentQueueChecked = false;
                }
            }
        }

        /// <summary>
        /// Whether or not the GUI option to use the next outcome as an automatic result, but only for the current queue/usage, is checked
        /// </summary>
        [Bindable(true)]
        public bool IsAlwaysUseThisOptionForCurrentQueueChecked {
            get => this.isAlwaysUseThisOptionForCurrentQueueChecked;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.isAlwaysUseThisOptionForCurrentQueueChecked, value && this.CanShowAlwaysUseNextResultForCurrentQueueOption);
                this.UpdateButtons();
            }
        }

        public string AutomaticResult {
            get => this.automaticResult;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.automaticResult, value);
            }
        }

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
        /// The resulting action ID that gets returned when the dialog closes with a successful result but no button was explicitly clicked
        /// </summary>
        public string PrimaryResult {
            get => this.primaryResult;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.primaryResult, value);
            }
        }

        /// <summary>
        /// The buttons for this message dialog. This list is ordered left to right, meaning that the first element will be on the very left.
        /// <para>
        /// On windows, the UI list is typically aligned to the right, meaning the last element is on the very right. So to have the typical
        /// Yes/No/Cancel buttons, you would add them to this list in that exact order; left to right
        /// </para>
        /// </summary>
        public ReadOnlyObservableCollection<DialogButton> Buttons { get; }

        public bool HasNoButtons => this.Buttons.Count < 1;

        public bool HasButtons => this.Buttons.Count > 0;

        protected DialogButton lastClickedButton;

        public BaseDialogExViewModel(string primaryResult = null, string defaultResult = null) {
            this.buttons = new ObservableCollectionEx<DialogButton>();
            this.Buttons = new ReadOnlyObservableCollection<DialogButton>(this.buttons);
            this.primaryResult = primaryResult;
            this.defaultResult = defaultResult;
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
            this.UpdateButtons();
            bool? result = await this.ShowDialogAsync();
            DialogButton button = this.lastClickedButton;
            this.lastClickedButton = null;
            if (result == true) {
                if (button != null) {
                    output = button.ActionType;
                    if (output != null && this.IsAlwaysUseThisOptionChecked) { // (output != null || this.AllowNullButtonActionForAutoResult)
                        this.AutomaticResult = output;
                    }
                }
                else {
                    output = this.PrimaryResult;
                }
            }
            else {
                output = this.DefaultResult;
            }
            return output;
        }

        protected abstract Task<bool?> ShowDialogAsync();

        public async Task OnButtonClicked(DialogButton button) {
            this.lastClickedButton = button;
            await this.Dialog.CloseDialogAsync(button?.ActionType != null);
        }

        public DialogButton InsertButton(int index, string msg, string actionType, bool canUseAsAutoResult = true) {
            this.EnsureNotReadOnly();
            DialogButton button = new DialogButton(this, actionType, msg, canUseAsAutoResult);
            this.buttons.Insert(index, button);
            return button;
        }

        public DialogButton ReplaceButton(int index, string msg, string actionType, bool canUseAsAutoResult = true) {
            this.EnsureNotReadOnly();
            this.buttons.RemoveAt(index);
            return this.InsertButton(index, msg, actionType, canUseAsAutoResult);
        }

        public DialogButton AddButton(string msg, string actionType, bool canUseAsAutoResult = true) {
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

        public void UpdateButtons() {
            foreach (DialogButton button in this.Buttons) {
                button.UpdateState();
            }
        }

        /// <summary>
        /// Creates a clone of this dialog. The returned instance will not be read only, allowing it to be further modified
        /// </summary>
        /// <returns></returns>
        public abstract BaseDialogExViewModel CloneCore();

        /// <summary>
        /// Marks this dialog as read-only, meaning most properties cannot be modified (apart from header, message, etc)
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void MarkReadOnly() {
            if (this.ShowAlwaysUseNextResultOption) {
                throw new InvalidOperationException($"Cannot set read-only when {nameof(this.ShowAlwaysUseNextResultOption)}");
            }

            this.IsReadOnly = true;
        }

        protected void EnsureNotReadOnly() {
            if (this.IsReadOnly) {
                throw new InvalidOperationException("This message dialog instance is read-only. Create a clone to modify it");
            }
        }
    }
}