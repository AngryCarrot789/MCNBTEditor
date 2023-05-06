using System;
using System.Threading.Tasks;
using MCNBTEditor.Core.Explorer.Dialog;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Views.Dialogs.UserInputs;
using MCNBTEditor.Utils;

namespace MCNBTEditor.Views.NBT.Editor {
    public class TagEditorService : ITagEditorService {
        public async Task<string> EditNameAsync(string title, string message, InputValidator nameValidator, string defaultName = "Tag Name Here") {
            TagPrimitiveEditorViewModel vm = new TagPrimitiveEditorViewModel {
                NameValidator = nameValidator,
                CanEditName = true,
                Title = title,
                Message = message,
                Name = defaultName
            };

            if (await this.ShowEditorAsync(vm)) {
                return vm.Name;
            }

            return null;
        }

        public async Task<string> EditValueAsync(string title, string message, NBTType type, string defaultValue = null) {
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

        public async Task<bool> EditPrimitiveAsync(string title, string message, TagPrimitiveViewModel tag) {
            TagCompoundViewModel parent = tag.ParentItem as TagCompoundViewModel;
            TagPrimitiveEditorViewModel editor = new TagPrimitiveEditorViewModel {
                CanEditValue = true,
                CanEditName = parent != null,
                Title = title,
                Message = message,
                TagType = tag.TagType,
                Value = tag.Data,
                NameValidator = parent?.CreateNameValidatorForEdit(tag),
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

        public async Task<TagPrimitiveViewModel> EditPrimitiveExAsync(string title, string message, NBTType type, TagCompoundViewModel targetOwningTag) {
            if (!type.IsPrimitive())
                throw new ArgumentException($"Type is not primitive: {type}", nameof(type));

            TagPrimitiveEditorViewModel editor = new TagPrimitiveEditorViewModel {
                CanEditValue = true,
                CanEditName = true,
                Title = title,
                Message = message,
                TagType = type,
                Value = "",
                NameValidator = targetOwningTag.NameValidator,
                Name = $"My {type}"
            };

            if (await this.ShowEditorAsync(editor)) {
                return new TagPrimitiveViewModel(editor.Name, type) {Data = editor.Value};
            }

            return null;
        }

        public Task<bool> ShowEditorAsync(TagPrimitiveEditorViewModel model) {
            return DispatcherUtils.InvokeAsync(() => this.ShowEditor(model));
        }

        public bool ShowEditor(TagPrimitiveEditorViewModel vm) {
            TagPrimitiveEditorWindow window = new TagPrimitiveEditorWindow {
                DataContext = vm
            };

            vm.Dialog = window;
            if (vm.NameValidator != null && window.NameValidationRule != null) {
                window.NameValidationRule.Validator = vm.NameValidator;
            }

            if (vm.ValueValidator != null && window.ValueValidationRule != null) {
                window.ValueValidationRule.Validator = vm.ValueValidator;
            }

            return window.ShowDialog() == true;
        }
    }
}