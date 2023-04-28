using System.Collections.Generic;
using System.Windows.Input;

namespace MCNBTEditor.Core.AdvancedContextService {
    public class CommandCheckableContextEntry : CommandContextEntry {
        private bool isChecked;
        public bool IsChecked {
            get => this.isChecked;
            set => this.RaisePropertyChanged(ref this.isChecked, value);
        }

        public CommandCheckableContextEntry(string header, string inputGestureText, string toolTip, ICommand command, object commandParameter, IEnumerable<IContextEntry> children = null) : base(header, inputGestureText, toolTip, command, commandParameter, children) {

        }

        public CommandCheckableContextEntry(string header, string inputGestureText, string toolTip, ICommand command, IEnumerable<IContextEntry> children = null) : this(header, inputGestureText, toolTip, command, null, children) {

        }

        public CommandCheckableContextEntry(string header, string inputGestureText, ICommand command, IEnumerable<IContextEntry> children = null) : this(header, inputGestureText, null, command, null, children) {

        }

        public CommandCheckableContextEntry(string header, ICommand command, IEnumerable<IContextEntry> children = null) : this(header, null, null, command, null, children) {

        }
    }
}