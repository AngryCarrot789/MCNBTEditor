using System.Collections.Generic;
using MCNBTEditor.Core.Explorer.Actions;

namespace MCNBTEditor.Core.AdvancedContextService {
    /// <summary>
    /// The class for action-based context entries. The header, tooltip, etc, are automatically fetched
    /// </summary>
    public class ActionContextEntry : BaseContextEntry {
        private string actionId;
        public string ActionId {
            get => this.actionId;
            set => this.RaisePropertyChanged(ref this.actionId, value);
        }

        public ActionContextEntry(object dataContext, string actionId, string header, string description, IEnumerable<IContextEntry> children = null) : base(dataContext, header, description, children) {
            this.actionId = actionId;
            if (actionId != null) {
                this.IconType = ActionIds.ResolveIcon(actionId);
            }
        }

        public ActionContextEntry(object dataContext, string actionId, string header, IEnumerable<IContextEntry> children = null) : this(dataContext, actionId, header, null, children) {

        }

        public ActionContextEntry(object dataContext, string actionId, IEnumerable<IContextEntry> children = null) : this(dataContext, actionId, null, null, children) {

        }

        public ActionContextEntry(object dataContext, IEnumerable<IContextEntry> children = null) : this(dataContext, null, null, null, children) {

        }

        public void SetActionKey(string key, object value) {
            base.SetContextKey(key, value);
        }
    }
}