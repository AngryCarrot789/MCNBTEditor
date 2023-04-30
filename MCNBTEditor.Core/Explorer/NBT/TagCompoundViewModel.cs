using System;
using System.Linq;
using System.Threading.Tasks;
using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Views.Dialogs.UserInputs;

namespace MCNBTEditor.Core.Explorer.NBT {
    public class TagCompoundViewModel : BaseTagCollectionViewModel {
        public InputValidator NameValidator { get; }

        public TagCompoundViewModel(string name = null) : base(name, NBTType.Compound) {
            this.NameValidator = InputValidator.FromFunc(input => {
                if (string.IsNullOrEmpty(input)) {
                    return "Input cannot be an empty string";
                }

                BaseTagViewModel first = this.FindChildTagByName(input);
                if (first != null) {
                    return "A tag already exists with that name: " + first;
                }

                return null;
            });
        }

        public override async Task PasteBinaryTagDataAction(string name, NBTBase nbt) {
            if (string.IsNullOrEmpty(name)) {
                await IoC.MessageDialogs.ShowMessageAsync("Invalid NBT", "No name associated with the tag. Cannot add it to a compound");
                return;
            }

            if (this.ChildTags.Any(x => x.Name == name)) {
                await IoC.MessageDialogs.ShowMessageAsync("Already exists", "A tag already exists with the name: " + name);
                return;
            }

            this.AddChild(CreateFrom(name, nbt));
        }

        public override NBTBase ToNBT() {
            NBTTagCompound tag = new NBTTagCompound();
            foreach (BaseTagViewModel item in this.ChildTags) {
                NBTBase nbt = item.ToNBT();
                if (nbt.TagType != 0) {
                    if (string.IsNullOrEmpty(item.Name)) {
                        throw new Exception("Tag name cannot be null or empty: " + item);
                    }

                    tag.map[item.Name] = nbt;
                }
            }

            return tag;
        }

        public override BaseTagViewModel Clone() {
            TagCompoundViewModel tag = new TagCompoundViewModel(this.Name);
            foreach (BaseTagViewModel item in this.ChildTags)
                tag.Add(item.Clone());
            return tag;
        }
    }
}