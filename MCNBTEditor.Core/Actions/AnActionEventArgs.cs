using System;
using MCNBTEditor.Core.Actions.Contexts;

namespace MCNBTEditor.Core.Actions {
    /// <summary>
    /// Action event arguments for when an action is about to be executed
    /// </summary>
    public class AnActionEventArgs {
        /// <summary>
        /// The action manager associated with this event
        /// </summary>
        public ActionManager Manager { get; }

        /// <summary>
        /// The data context for this specific action execution. This will not be null, but it may be empty (contain no inner data or data context)
        /// </summary>
        public IDataContext DataContext { get; }

        /// <summary>
        /// Whether this event was originally caused by a user or not
        /// </summary>
        public bool IsUserInitiated { get; }

        /// <summary>
        /// The action ID associated with this event. Null if the action isn't a fully registered action (and therefore has no ID)
        /// </summary>
        public string ActionId { get; }

        public AnActionEventArgs(ActionManager manager, string actionId, IDataContext dataContext, bool isUserInitiated) {
            if (actionId != null && actionId.Length < 1) {
                throw new ArgumentException("ActionId must be null or a non-empty string");
            }

            this.Manager = manager ?? throw new ArgumentNullException(nameof(manager), "Action manager cannot be null");
            this.DataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext), "Data context cannot be null");
            this.IsUserInitiated = isUserInitiated;
            this.ActionId = actionId;
        }
    }
}