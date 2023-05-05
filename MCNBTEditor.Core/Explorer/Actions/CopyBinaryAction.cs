using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCNBTEditor.Core.Actions;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.Views.Dialogs.Message;

namespace MCNBTEditor.Core.Explorer.Actions {
    [ActionRegistration("actions.nbt.copy.primitive_value")]
    public class CopyValueAction : ExtendedListActionBase {
        public CopyValueAction() : base("Copy Value", "Copies the primitive tag's value to the system clipboard") {

        }

        public override async Task<bool> ExecuteSelectionAsync(AnActionEventArgs e, IEnumerable<BaseTreeItemViewModel> selection) {
            if (IoC.Clipboard == null) {
                await Dialogs.ClipboardUnavailableDialog.ShowAsync("Clipboard unavailable", "Clipboard is unavailable. Cannot copy tag value");
                return true;
            }

            List<TagPrimitiveViewModel> primitives = selection.OfType<TagPrimitiveViewModel>().ToList();
            if (primitives.Count < 1)
                return false; // should this return true? logically it should...

            IoC.Clipboard.ReadableText = string.Join("\n", primitives.Select(x => x.Data));
            return true;
        }
    }
}