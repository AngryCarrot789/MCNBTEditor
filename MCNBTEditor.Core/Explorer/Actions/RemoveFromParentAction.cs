using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCNBTEditor.Core.Actions;

namespace MCNBTEditor.Core.Explorer.Actions {
    [ActionRegistration("actions.nbt.remove_from_parent")]
    public class RemoveFromParentAction : ExtendedListActionBase {
        public RemoveFromParentAction() : base("Remove", "Remove the tag from its parent") {

        }

        public override Presentation GetPresentationForSelection(AnActionEventArgs e, IEnumerable<BaseTreeItemViewModel> selection) {
            return selection.Any(x => x is IRemoveable rm && rm.CanRemoveFromParent()) ? Presentation.VisibleAndEnabled : Presentation.VisibleAndDisabled;
        }

        public override async Task<bool> ExecuteSelectionAsync(AnActionEventArgs e, IEnumerable<BaseTreeItemViewModel> selection) {
            foreach (BaseTreeItemViewModel item in selection) {
                if (item is IRemoveable removeable && removeable.CanRemoveFromParent()) {
                    await removeable.RemoveFromParentAsync();
                }
            }

            return true;
        }
    }
}