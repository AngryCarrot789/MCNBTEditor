using System.Threading.Tasks;
using MCNBTEditor.Core.Actions;
using MCNBTEditor.Core.Explorer.NBT;

namespace MCNBTEditor.Core.Explorer.Actions {
    [ActionRegistration("actions.nbt.remove_from_parent")]
    public class RemoveFromParentAction : AnAction {
        public RemoveFromParentAction() : base("Remove", "Remove the tag from its parent") {
        }

        public override Presentation GetPresentation(AnActionEventArgs e) {
            if (!NBTActions.FindTag(e.DataContext, out BaseTagViewModel x))
                return Presentation.Invisible;
            return x.ParentTag != null ? Presentation.VisibleAndEnabled : Presentation.VisibleAndDisabled;
        }

        public override Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!NBTActions.FindTag(e.DataContext, out var tag))
                return Task.FromResult(false);
            BaseTagCollectionViewModel parent = tag.ParentTag;
            if (parent == null)
                return Task.FromResult(false);
            parent.RemoveItem(tag);
            return Task.FromResult(true);
        }
    }
}