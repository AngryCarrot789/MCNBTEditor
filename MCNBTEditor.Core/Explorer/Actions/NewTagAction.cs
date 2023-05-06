using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MCNBTEditor.Core.Actions;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.NBT;

namespace MCNBTEditor.Core.Explorer.Actions {
    [ActionRegistration("actions.nbt.newtag")]
    public class NewTagAction : AnAction {
        public const string TypeKey = "NEW_TAG_TYPE";

        public NewTagAction() : base("New tag", "Create a new tag") {

        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGet(TypeKey, out NBTType type)) {
                Debug.WriteLine("[NewTagAction] Context is missing the tag type to create");
                return false;
            }

            if (!e.DataContext.TryGetContext(out BaseTagCollectionViewModel tag)) {
                Debug.WriteLine("[NewTagAction] Context is missing a tag collection view model");
                return false;
            }

            if (!(tag is TagListViewModel) && !(tag is TagCompoundViewModel)) {
                await IoC.MessageDialogs.ShowMessageAsync("Invalid tag type", $"Child tags cannot be added to {tag.TagType}");
                return true;
            }

            BaseTagViewModel newTag;
            if (tag is TagCompoundViewModel compound) {
                if (type.IsPrimitive()) {
                    newTag = await IoC.TagEditorService.EditPrimitiveExAsync($"Create new NBTTag{type}", null, type, compound);
                    if (newTag == null) {
                        return true;
                    }
                }
                else {
                    string result = await IoC.TagEditorService.EditNameAsync($"Create new NBTTag{type}", null, compound.NameValidator);
                    if (result == null) {
                        return true;
                    }

                    newTag = BaseTagViewModel.CreateFrom(result, type);
                }
            }
            else {
                if (type.IsPrimitive()) {
                    string result = await IoC.TagEditorService.EditValueAsync($"Create new NBTTag{type}", null, type);
                    if (result == null) {
                        return true;
                    }

                    newTag = BaseTagViewModel.CreateFrom(null, type);
                    if (!(newTag is TagPrimitiveViewModel primitive)) {
                        throw new Exception($"NBTType '{type}' did not create a primitive tag view model?????");
                    }

                    primitive.Data = result;
                }
                else {
                    newTag = BaseTagViewModel.CreateFrom(null, type);
                }
            }

            if (newTag.Name != null) {
                // Input validators should prevent duplicates... but just in case...
                BaseTagViewModel existing = tag.FindChildTagByName(newTag.Name);
                if (existing != null) {
                    await IoC.MessageDialogs.ShowMessageAsync("Tag already exists", $"A tag with the name '{newTag.Name}' already exists as {existing.TagType}");
                    return true;
                }
            }

            tag.AddChild(newTag);
            return true;
        }
    }
}