using System.Collections.Generic;
using MCNBTEditor.Core.Actions;

namespace MCNBTEditor.Core.AdvancedContextService {
    /// <summary>
    /// The default implementation for a context entry (aka menu item), which also supports modifying the header,
    /// input gesture text, command and command parameter to reflect the UI menu item
    /// </summary>
    public class ActionContextEntry : BaseContextEntry {
        private string actionId;
        public string ActionId {
            get => this.actionId;
            set => this.RaisePropertyChanged(ref this.actionId, value);
        }

        private ActionManager manager = ActionManager.Instance;
        public ActionManager Manager {
            get => this.manager;
            set => this.RaisePropertyChanged(ref this.manager, value ?? ActionManager.Instance);
        }

        public ActionContextEntry(object dataContext, string actionId, IEnumerable<IContextEntry> children = null) : base(dataContext, children) {
            this.actionId = actionId;
        }

        public ActionContextEntry(object dataContext, IEnumerable<IContextEntry> children = null) : this(dataContext, null, children) {

        }
    }
}