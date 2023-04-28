using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCNBTEditor.Core.Views.Dialogs.Message {
    public class BaseDialogButton : BaseViewModel {
        /// <summary>
        /// The dialog that owns this button
        /// </summary>
        public MessageDialog Dialog { get; }

        private string text;
        public string Text {
            get => this.text;
            set => this.RaisePropertyChanged(ref this.text, value);
        }

        public BaseDialogButton(MessageDialog dialog, string text) {
            this.Dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            this.text = text;
        }
    }
}