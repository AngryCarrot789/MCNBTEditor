using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Automation;
using MCNBTEditor.Core.Explorer.Dialog;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Views.Dialogs.UserInputs;
using MCNBTEditor.Utils;

namespace MCNBTEditor.Views.NBT.Editor {
    public class TagEditorService : ITagEditorService {
        public async Task<string> EditTagNameAsync(string title, string message, InputValidator nameValidator, string defaultName = "Tag Name Here") {
            TagPrimitiveEditorViewModel vm = new TagPrimitiveEditorViewModel {
                NameValidator = nameValidator,
                CanEditName = true,
                Title = title,
                Message = message,
                Name = defaultName
            };

            if (await this.ShowEditorAsync(vm)) {
                return vm.Value;
            }

            return null;
        }

        public async Task<string> EditPrimitiveValueAsync(string title, string message, NBTType type, string defaultValue = null) {
            TagPrimitiveEditorViewModel vm = new TagPrimitiveEditorViewModel {
                CanEditValue = true,
                Title = title,
                Message = message,
                TagType = type,
                Value = defaultValue
            };

            if (await this.ShowEditorAsync(vm)) {
                return vm.Value;
            }

            return null;
        }

        public async Task<bool> EditPrimitiveTagAsync(string title, string message, TagPrimitiveViewModel tag) {
            TagCompoundViewModel parent = tag.ParentItem as TagCompoundViewModel;
            TagPrimitiveEditorViewModel editor = new TagPrimitiveEditorViewModel {
                CanEditValue = true,
                CanEditName = parent != null,
                Title = title,
                Message = message,
                TagType = tag.TagType,
                Value = tag.Data,
                NameValidator = parent?.NameValidator,
                Name = tag.Name
            };

            if (await this.ShowEditorAsync(editor)) {
                if (editor.CanEditName)
                    tag.Name = editor.Name;
                if (editor.CanEditValue)
                    tag.Data = editor.Value;
                return true;
            }

            return false;
        }

        public Task<bool> ShowEditorAsync(TagPrimitiveEditorViewModel model) {
            return DispatcherUtils.InvokeAsync(() => this.ShowEditor(model));
        }

        public bool ShowEditor(TagPrimitiveEditorViewModel model) {
            TagPrimitiveEditorWindow window = new TagPrimitiveEditorWindow {
                DataContext = model
            };

            model.Dialog = window;
            return window.ShowDialog() == true;
        }
    }
}