using System;
using System.Windows.Input;

namespace MCNBTEditor.Core.Timing {
    public class TimedInputUpdateViewModel  : BaseViewModel, IDisposable {
        private string inputText;
        public string InputText {
            get => this.inputText;
            set {
                this.RaisePropertyChanged(ref this.inputText, value);
                if (this.CanSearchForInput()) {
                    this.IdleEventService.OnInput();
                }
                else {
                    this.OnInputReset();
                }
            }
        }

        public RelayCommand TriggerCommand { get; }

        public ICommand ClearInputCommand { get; }

        public IdleEventService IdleEventService { get; }

        public bool WasLastSearchForced { get; private set; }

        public TimedInputUpdateViewModel() {
            this.IdleEventService = new IdleEventService();
            this.TriggerCommand = new RelayCommand(this.ForceSearchAction, this.CanSearchForInput);
            this.ClearInputCommand = new RelayCommand(this.ClearSearchInputAction);
        }

        public virtual void ForceSearchAction() {
            this.WasLastSearchForced = true;
            try {
                this.IdleEventService.ForceAction();
            }
            finally {
                this.WasLastSearchForced = false;
            }
        }

        public virtual void ClearSearchInputAction() {
            this.InputText = "";
        }

        public virtual bool CanSearchForInput() {
            return !string.IsNullOrEmpty(this.InputText);
        }

        /// <summary>
        /// Called when the search is no longer active; reset everything to it's original state
        /// </summary>
        public virtual void OnInputReset() {
            this.IdleEventService.CanFireNextTick = false;
            this.TriggerCommand.RaiseCanExecuteChanged();
        }

        public void Dispose() {
            this.IdleEventService.Dispose();
        }
    }
}