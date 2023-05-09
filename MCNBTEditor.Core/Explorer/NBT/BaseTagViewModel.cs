using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MCNBTEditor.Core.AdvancedContextService;
using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Shortcuts;
using MCNBTEditor.Core.Utils;

namespace MCNBTEditor.Core.Explorer.NBT {
    public abstract class BaseTagViewModel : BaseTreeItemViewModel, IHaveTreePath, IRemoveable {
        public string TreePathPartName {
            get {
                if (this.ParentItem is TagListViewModel tagList) {
                    return new StringBuilder().Append('[').Append(tagList.Children.IndexOf(this)).Append(']').ToString();
                }
                else if (string.IsNullOrEmpty(this.Name)) {
                    return "<unnamed>";
                }
                else {
                    return this.Name;
                }
            }
        }

        public BaseTagCollectionViewModel ParentTag => this.ParentItem as BaseTagCollectionViewModel;

        public NBTType TagType { get; }

        protected string name;
        public string Name {
            get => this.name;
            set => this.RaisePropertyChanged(ref this.name, value);
        }

        public AsyncRelayCommand RemoveFromParentCommand { get; }
        public AsyncRelayCommand CopyNameCommand { get; }
        public AsyncRelayCommand CopyBinaryToClipboardCommand { get; }
        public AsyncRelayCommand RenameCommand { get; }

        public override bool CanHoldChildren => false;

        protected BaseTagViewModel(string name, NBTType type) {
            this.Name = name;
            this.TagType = type;
            this.RemoveFromParentCommand = new AsyncRelayCommand(async () => await this.RemoveFromParentAction(), this.CanRemoveFromParent);
            this.CopyNameCommand = new AsyncRelayCommand(async () => {
                await ClipboardUtils.SetClipboardOrShowErrorDialog(this.Name ?? "");
            }, () => !string.IsNullOrEmpty(this.Name));
            this.CopyBinaryToClipboardCommand = new AsyncRelayCommand(async () => {
                if (IoC.Clipboard == null) {
                    await Task.Run(() => IoC.MessageDialogs.ShowMessageAsync("No clipboard", "Clipboard is unavailable. Cannot copy the NBT to the clipboard"));
                }
                else {
                    using (MemoryStream stream = new MemoryStream()) {
                        try {
                            NBTBase nbt = this.ToNBT();
                            NBTBase.WriteTag(CompressedStreamTools.CreateOutput(stream), this.Name, nbt);
                            IoC.Clipboard.SetBinaryTag("TAG_NBT", stream.ToArray());
                        }
                        catch (Exception e) {
                            await IoC.MessageDialogs.ShowMessageAsync("Failed to write NBT", "Failed to write NBT to clipboard: " + e.Message);
                        }
                    }
                }
            });

            this.RenameCommand = new AsyncRelayCommand(this.RenameAction, () => this.ParentTag is TagCompoundViewModel);
        }

        protected BaseTagViewModel(NBTType type) : this(null, type) {

        }

        protected BaseTagViewModel() : this(null, NBTType.End) {

        }

        public static TagCompoundViewModel CreateFrom(string name, NBTTagCompound nbt) {
            TagCompoundViewModel tag = new TagCompoundViewModel(name);
            tag.AddItem(nbt);
            return tag;
        }

        public static TagListViewModel CreateFrom(string name, NBTTagList nbt) {
            TagListViewModel tag = new TagListViewModel(name);
            tag.AddItem(nbt);
            return tag;
        }

        public static BaseTagViewModel CreateFrom(string name, NBTBase nbt) {
            switch (nbt) {
                case NBTTagCompound  comp: return CreateFrom(name, comp);
                case NBTTagList      list: return CreateFrom(name, list);
                case NBTTagLongArray la:   return new TagLongArrayViewModel(name) {Data = la.data};
                case NBTTagIntArray  ia:   return new TagIntArrayViewModel(name) {Data = ia.data};
                case NBTTagByteArray ba:   return new TagByteArrayViewModel(name) {Data = ba.data};
                case NBTTagByte      b:    return new TagPrimitiveViewModel(name, NBTType.Byte) {Data = b.data.ToString()};
                case NBTTagShort     s:    return new TagPrimitiveViewModel(name, NBTType.Short) {Data = s.data.ToString()};
                case NBTTagInt       i:    return new TagPrimitiveViewModel(name, NBTType.Int) {Data = i.data.ToString()};
                case NBTTagLong      l:    return new TagPrimitiveViewModel(name, NBTType.Long) {Data = l.data.ToString()};
                case NBTTagFloat     f:    return new TagPrimitiveViewModel(name, NBTType.Float) {Data = f.data.ToString()};
                case NBTTagDouble    d:    return new TagPrimitiveViewModel(name, NBTType.Double) {Data = d.data.ToString()};
                case NBTTagString    str:  return new TagPrimitiveViewModel(name, NBTType.String) {Data = str.data};
                case NBTTagEnd       _:    return new TagPrimitiveViewModel(name, NBTType.End);
                case null:                 return null;
                default:                   throw new ArgumentException($"Unexpected NBT instance: {nbt}", nameof(nbt));
            }
        }

        public static BaseTagViewModel CreateFrom(string name, NBTType type) {
            switch (type) {
                case NBTType.Compound:  return new TagCompoundViewModel(name);
                case NBTType.List:      return new TagListViewModel(name);
                case NBTType.LongArray: return new TagLongArrayViewModel(name);
                case NBTType.IntArray:  return new TagIntArrayViewModel(name);
                case NBTType.ByteArray: return new TagByteArrayViewModel(name);
                case NBTType.Byte:      return new TagPrimitiveViewModel(name, NBTType.Byte);
                case NBTType.Short:     return new TagPrimitiveViewModel(name, NBTType.Short);
                case NBTType.Int:       return new TagPrimitiveViewModel(name, NBTType.Int);
                case NBTType.Long:      return new TagPrimitiveViewModel(name, NBTType.Long);
                case NBTType.Float:     return new TagPrimitiveViewModel(name, NBTType.Float);
                case NBTType.Double:    return new TagPrimitiveViewModel(name, NBTType.Double);
                case NBTType.String:    return new TagPrimitiveViewModel(name, NBTType.String);
                case NBTType.End:       return new TagPrimitiveViewModel(name, NBTType.End);
                default:                throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public List<BaseTagViewModel> GetParentTagChain() {
            BaseTagViewModel item = this;
            List<BaseTagViewModel> list = new List<BaseTagViewModel>();
            do {
                list.Add(item);
            } while ((item = item.ParentTag) != null);
            list.Reverse();
            return list;
        }

        public virtual async Task RenameAction() {
            if (!(this.ParentTag is TagCompoundViewModel compound)) {
                return;
            }

            string newName = await IoC.TagEditorService.EditNameAsync($"Rename NBTTag{this.TagType}", "Input a new name for this element", compound.CreateNameValidatorForEdit(this), this.Name ?? "");
            if (newName != null) {
                this.Name = newName;
            }
        }

        public abstract NBTBase ToNBT();

        public virtual BaseTagViewModel Clone() {
            // Slow clone
            return CreateFrom(this.Name, this.ToNBT());
        }

        public override string ToString() {
            return $"{this.GetType()} ({this.TagType})";
        }

        public virtual void OnRemovedFromParent() {

        }

        public virtual void OnAddedToParent() {

        }

        public virtual bool CanRemoveFromParent() {
            return this.ParentItem is IHaveChildren;
        }

        public virtual Task RemoveFromParentAction() {
            if (this.ParentItem is IHaveChildren children) {
                children.RemoveItem(this);
            }

            return Task.CompletedTask;
        }

        public virtual bool RemoveFromParent() {
            return this.ParentItem is IHaveChildren children && children.RemoveItem(this);
        }

        // Easiest way to implement a modified notification
        public virtual void OnModified(BaseTagViewModel tag) {
            this.ParentTag?.OnModified(tag);
        }
    }
}