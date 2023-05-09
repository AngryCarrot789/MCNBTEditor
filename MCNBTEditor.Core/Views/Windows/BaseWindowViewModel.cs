using System.Windows.Input;

namespace MCNBTEditor.Core.Views.Windows {
    public class BaseWindowViewModel : BaseViewModel {
        public IWindow Window { get; set; }

        public ICommand CloseCommand { get; }

        public BaseWindowViewModel() {
            this.CloseCommand = new RelayCommand(this.CloseDialogAction, this.CanCloseDialog);
        }

        protected virtual bool CanCloseDialog() {
            return this.Window != null;
        }

        protected virtual void CloseDialogAction() {
            this.Window?.CloseWindow();
        }
    }
}