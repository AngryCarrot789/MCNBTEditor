using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCNBTEditor.Core.Actions;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.Views.Dialogs.Message;

namespace MCNBTEditor.Core.Explorer.Actions {
    public class CopyNameAction : MultiSelectionAction {
        public CopyNameAction() : base("Copy Name", "Copies the tag's name (if it has one) to the system clipboard") {

        }

        public override async Task<bool> ExecuteSelectionAsync(AnActionEventArgs e, IEnumerable<BaseTreeItemViewModel> selection) {
            if (IoC.Clipboard == null) {
                await Dialogs.ClipboardUnavailableDialog.ShowAsync("Clipboard unavailable", "Clipboard is unavailable. Cannot copy tag value");
                return true;
            }

            List<BaseTagViewModel> primitives = selection.OfType<BaseTagViewModel>().ToList();
            if (primitives.Count < 1)
                return true;

            IoC.Clipboard.ReadableText = string.Join("\n", primitives.Select(x => x.Name));
            return true;
        }
    }
}