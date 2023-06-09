using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCNBTEditor.Core.Actions;

namespace MCNBTEditor.Core.Explorer.Actions {
    public abstract class MultiSelectionAction : AnAction {
        protected MultiSelectionAction(Func<string> header, Func<string> description) : base() {

        }

        protected MultiSelectionAction(string header, string description) : base() {

        }

        public override Presentation GetPresentation(AnActionEventArgs e) {
            if (NBTActionUtils.GetSelectedItems(e.DataContext, out IEnumerable<BaseTreeItemViewModel> tags)) {
                return this.GetPresentationForSelection(e, tags);
            }

            return Presentation.Invisible;
        }

        public virtual Presentation GetPresentationForSelection(AnActionEventArgs e, IEnumerable<BaseTreeItemViewModel> selection) {
            return selection.Any() ? Presentation.VisibleAndEnabled : Presentation.VisibleAndDisabled;
        }

        public override Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (NBTActionUtils.GetSelectedItems(e.DataContext, out IEnumerable<BaseTreeItemViewModel> tags)) {
                return this.ExecuteSelectionAsync(e, tags);
            }

            return Task.FromResult(false);
        }

        public abstract Task<bool> ExecuteSelectionAsync(AnActionEventArgs e, IEnumerable<BaseTreeItemViewModel> selection);
    }
}