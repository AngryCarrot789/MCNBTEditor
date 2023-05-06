using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MCNBTEditor.Core.Actions.Contexts;

namespace MCNBTEditor.Core.Actions {
    public class ActionCommand : BaseAsyncRelayCommand {
        public string ActionId { get; }

        public DataContext Context { get; }

        public ActionCommand(string actionId, DataContext context = null) {
            if (string.IsNullOrEmpty(actionId))
                throw new ArgumentException("ActionId cannot be null or empty", nameof(actionId));
            this.ActionId = actionId;
            this.Context = context ?? new DataContext();
        }

        public AnAction GetAction(ActionManager manager) {
            return manager.GetAction(this.ActionId);
        }

        public override bool CanExecute(object parameter) {
            if (!base.CanExecute(parameter))
                return false;
            Presentation p = ActionManager.Instance.GetPresentation(this.ActionId, this.Context);
            return p.IsVisible && p.IsEnabled;
        }

        protected override Task ExecuteCoreAsync(object parameter) {
            return ActionManager.Instance.Execute(this.ActionId, this.Context);
        }
    }
}
