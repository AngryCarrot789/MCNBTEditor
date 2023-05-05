using System.Threading.Tasks;
using MCNBTEditor.Core.Actions;
using MCNBTEditor.Core.Explorer.NBT;

namespace MCNBTEditor.Core.Explorer.Actions {
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
}