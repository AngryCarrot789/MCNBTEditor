using System.Threading.Tasks;
using MCNBTEditor.Core.Actions;
using MCNBTEditor.Core.Actions.Contexts;
using MCNBTEditor.Core.Explorer.NBT;

namespace MCNBTEditor.Core.Explorer.Actions {
    public static class NBTActions {
        public static bool FindTag(IDataContext context, out BaseTagViewModel tag) {
            return context.TryGetContext(out tag);
        }
    }

    [ActionRegistration("actions.nbt.copy.name")]
    public class CopyNameAction : AnAction {
        public CopyNameAction() : base("Copy Name", "Copies the tag's name (if it has one) to the system clipboard") {
        }

        public override Presentation GetPresentation(AnActionEventArgs e) {
            if (!NBTActions.FindTag(e.DataContext, out BaseTagViewModel x))
                return Presentation.Invisible;
            return !string.IsNullOrEmpty(x.Name) ? Presentation.VisibleAndEnabled : Presentation.VisibleAndDisabled;
        }

        public override Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!NBTActions.FindTag(e.DataContext, out var tag) || string.IsNullOrEmpty(tag.Name))
                return Task.FromResult(false);
            IoC.Clipboard.ReadableText = tag.Name;
            return Task.FromResult(true);
        }
    }

    [ActionRegistration("actions.nbt.copy.primitive_value")]
    public class CopyValueAction : AnAction {
        public CopyValueAction() : base("Copy Value", "Copies the primitive tag's value to the system clipboard") {
        }

        public override Presentation GetPresentation(AnActionEventArgs e) {
            if (!NBTActions.FindTag(e.DataContext, out BaseTagViewModel x))
                return Presentation.Invisible;
            return x is TagPrimitiveViewModel ? Presentation.VisibleAndEnabled : Presentation.VisibleAndDisabled;
        }

        public override Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!NBTActions.FindTag(e.DataContext, out var tag) || !(tag is TagPrimitiveViewModel primitive))
                return Task.FromResult(false);
            IoC.Clipboard.ReadableText = primitive.Data;
            return Task.FromResult(true);
        }
    }

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