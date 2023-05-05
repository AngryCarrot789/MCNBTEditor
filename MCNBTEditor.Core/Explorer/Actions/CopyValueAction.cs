using System.Threading.Tasks;
using MCNBTEditor.Core.Actions;
using MCNBTEditor.Core.Explorer.NBT;

namespace MCNBTEditor.Core.Explorer.Actions {
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
}