using System.Threading.Tasks;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Views.Dialogs.UserInputs;

namespace MCNBTEditor.Core.Explorer.Dialog {
    public interface ITagEditorService {
        Task<string> EditNameAsync(string title, string message, InputValidator nameValidator, string defaultName = "Tag Name Here");
        Task<string> EditValueAsync(string title, string message, NBTType type, string defaultValue = null);
        Task<bool> EditPrimitiveAsync(string title, string message, TagPrimitiveViewModel tag);
        Task<TagPrimitiveViewModel> EditPrimitiveExAsync(string title, string message, NBTType type, TagCompoundViewModel targetOwningTag);
        Task<bool> ShowEditorAsync(TagPrimitiveEditorViewModel model);
    }
}