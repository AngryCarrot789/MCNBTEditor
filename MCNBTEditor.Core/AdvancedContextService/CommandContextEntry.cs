using System.Collections.Generic;
using System.Windows.Input;

namespace MCNBTEditor.Core.AdvancedContextService {
    /// <summary>
    /// The default implementation for a context entry (aka menu item), which also supports modifying the header,
    /// input gesture text, command and command parameter to reflect the UI menu item
    /// </summary>
    public class CommandContextEntry : BaseContextEntry {
        private string header;
        private string inputGestureText;
        private string toolTip;
        private ICommand command;
        private object commandParameter;

        /// <summary>
        /// The menu item's header, aka text
        /// </summary>
        public string Header {
            get => this.header;
            set => this.RaisePropertyChanged(ref this.header, value);
        }

        /// <summary>
        /// The preview input gesture text, which is typically on the right side of a menu item (used for shortcuts)
        /// </summary>
        public string InputGestureText {
            get => this.inputGestureText;
            set => this.RaisePropertyChanged(ref this.inputGestureText, value);
        }

        /// <summary>
        /// A mouse over tooltip for this entry
        /// </summary>
        public string ToolTip {
            get => this.toolTip;
            set => this.RaisePropertyChanged(ref this.toolTip, value);
        }

        public ICommand Command {
            get => this.command;
            set => this.RaisePropertyChanged(ref this.command, value);
        }

        public object CommandParameter {
            get => this.commandParameter;
            set => this.RaisePropertyChanged(ref this.commandParameter, value);
        }

        public CommandContextEntry(string header, string inputGestureText, string toolTip, ICommand command, object commandParameter, IEnumerable<IContextEntry> children = null) : base(null, children) {
            this.header = header;
            this.inputGestureText = inputGestureText;
            this.toolTip = toolTip;
            this.command = command;
            this.commandParameter = commandParameter;
        }

        public CommandContextEntry(string header, string inputGestureText, string toolTip, ICommand command, IEnumerable<IContextEntry> children = null) : this(header, inputGestureText, toolTip, command, null, children) {

        }

        public CommandContextEntry(string header, string inputGestureText, ICommand command, IEnumerable<IContextEntry> children = null) : this(header, inputGestureText, null, command, null, children) {

        }

        public CommandContextEntry(string header, string inputGestureText, ICommand command, object commandParam, IEnumerable<IContextEntry> children = null) : this(header, inputGestureText, null, command, commandParam, children) {

        }

        public CommandContextEntry(string header, ICommand command, IEnumerable<IContextEntry> children = null) : this(header, null, null, command, null, children) {

        }

        public CommandContextEntry(string header, ICommand command, object commandParam, IEnumerable<IContextEntry> children = null) : this(header, null, null, command, commandParam, children) {

        }
    }
}