using System.Collections.Generic;
using MCNBTEditor.Core.Actions.Contexts;

namespace MCNBTEditor.Core.AdvancedContextService {
    /// <summary>
    /// Base class for context entries, supporting custom data context
    /// </summary>
    public abstract class BaseContextEntry : BaseViewModel, IContextEntry {
        private readonly DataContext context;

        public IDataContext Context => this.context;

        public IEnumerable<IContextEntry> Children { get; }

        protected BaseContextEntry(object dataContext, IEnumerable<IContextEntry> children = null) {
            this.context = new DataContext();
            if (dataContext != null)
                this.context.AddContext(dataContext);
            this.Children = children;
        }

        protected BaseContextEntry(IEnumerable<IContextEntry> children = null) : this(null, children) {

        }

        protected void SetContextKey(string key, object value) {
            this.context.Set(key, value);
        }
    }
}