using System.Collections.Generic;
using MCNBTEditor.Core.Actions;
using MCNBTEditor.Core.Utils;

namespace MCNBTEditor.Core.AdvancedContextService {
    public class ActionCheckableContextEntry : ActionContextEntry {
        private bool isChecked;
        public bool IsChecked {
            get => this.isChecked;
            set {
                this.RaisePropertyChanged(ref this.isChecked, value);
                this.SetContextKey(ToggleAction.IsToggledKey, value.Box());
            }
        }

        protected ActionCheckableContextEntry(object dataContext, string actionId, IEnumerable<IContextEntry> children = null) : base(dataContext, actionId, children) {

        }

        protected ActionCheckableContextEntry(object dataContext, IEnumerable<IContextEntry> children = null) : this(dataContext, null, children) {

        }
    }
}