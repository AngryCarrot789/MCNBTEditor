using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCNBTEditor.Core.Actions;
using MCNBTEditor.Core.Explorer.NBT;

namespace MCNBTEditor.Core.Explorer.Actions {
    [ActionRegistration("actions.nbt.rename.tag")]
    public class RenameTagAction : AnAction {
        public RenameTagAction() : base("Rename", "Renames this tag") {

        }

        public override Presentation GetPresentation(AnActionEventArgs e) {
            if (NBTActionUtils.GetSelectedItems(e.DataContext, out IEnumerable<BaseTreeItemViewModel> tags)) {
                return tags.Count() != 1 ? Presentation.VisibleAndDisabled : Presentation.VisibleAndEnabled;
            }

            return Presentation.Invisible;
        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (NBTActionUtils.FindTag(e.DataContext, out BaseTagViewModel tag)) {
                await tag.RenameAction();
                return true;
            }

            return false;
        }
    }
}