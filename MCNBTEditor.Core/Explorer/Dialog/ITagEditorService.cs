using System.Threading.Tasks;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Views.Dialogs.UserInputs;

namespace MCNBTEditor.Core.Explorer.Dialog {
    public interface ITagEditorService {
        Task<string> EditTagNameAsync(string title, string message, InputValidator nameValidator, string defaultName = "Tag Name Here");
        Task<string> EditPrimitiveValueAsync(string title, string message, NBTType type, string defaultValue = null);
        Task<bool> EditPrimitiveTagAsync(string title, string message, TagPrimitiveViewModel tag);
        Task<bool> ShowEditorAsync(TagPrimitiveEditorViewModel model);
    }
}