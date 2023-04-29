using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MCNBTEditor.Core.AdvancedContextService;
using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Utils;
using MCNBTEditor.Core.Views.Dialogs.UserInputs;

namespace MCNBTEditor.Core.Explorer.NBT {
    public abstract class BaseTagViewModel : BaseTreeItemViewModel, IHaveTreePath, IContextProvider {
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

        public NBTType NBTType { get; }

        protected string name;
        public string Name {
            get => this.name;
            set => this.RaisePropertyChanged(ref this.name, value);
        }

        public RelayCommand RemoveFromParentCommand { get; }
        public AsyncRelayCommand CopyNameCommand { get; }
        public AsyncRelayCommand CopyBinaryToClipboardCommand { get; }
        public AsyncRelayCommand RenameCommand { get; }

        public override bool CanHoldChildren => false;

        protected BaseTagViewModel(string name, NBTType type) {
            this.Name = name;
            this.NBTType = type;
            this.RemoveFromParentCommand = new RelayCommand(() => this.RemoveFromParentItem(false), () => this.RemoveFromParentItem(true));
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

        public List<BaseTagViewModel> GetParentTagChain() {
            BaseTagViewModel item = this;
            List<BaseTagViewModel> list = new List<BaseTagViewModel>();
            do {
                list.Add(item);
            } while ((item = item.ParentTag) != null);
            list.Reverse();
            return list;
        }

        protected virtual async Task RenameAction() {
            if (!(this.ParentTag is TagCompoundViewModel compound)) {
                return;
            }

            string newName = await IoC.UserInput.ShowSingleInputDialog("Rename tag", "Input a new name for this element", this.Name ?? "", InputValidator.FromFunc(input => {
                BaseTagViewModel first = compound.FindChildTagByName(input);
                if (first != null) {
                    return "A tag already exists with that name: " + first;
                }

                return null;
            }));

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
            return $"{this.GetType()} ({this.NBTType.ToString()})";
        }

        public virtual void OnRemovedFromParent() {

        }

        public virtual void OnAddedToParent() {

        }

        public bool RemoveFromParentItem(bool isFakeRemove) {
            // minecraft mod style of "doRemove parameter" functions ;)
            return this.ParentItem is IHaveChildren children && (isFakeRemove || children.RemoveItem(this));
        }

        public virtual void GetContext(List<IContextEntry> list) {
            // I know using "this is" is a smelly way of doing things but it's so
            // much easier in this case than using virtual functions
            if (this is BaseTagCollectionViewModel tagCollection) {
                list.Add(new CommandContextEntry("Sort by Name", tagCollection.SortByNameCommand));
                list.Add(new CommandContextEntry("Sort by Type", tagCollection.SortByTypeCommand));
                list.Add(new CommandContextEntry("Sort by Both (default)", tagCollection.SortByBothCommand));
                list.Add(SeparatorEntry.Instance);
            }

            if (this is TagDataFileViewModel datFile) {
                list.Add(new CommandContextEntry("Refresh", datFile.RefreshDataCommand));
                list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/Save", datFile.SaveFileCommand));
                list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/SaveAs", datFile.SaveFileAsCommand));
                list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/CopyFilePath", datFile.CopyFilePathToClipboardCommand));
                list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/OpenInExplorer", datFile.ShowInExplorerCommand));
                list.Add(SeparatorEntry.Instance);
            }

            list.Add(new ShortcutCommandContextEntry(new List<string>{"Application/EditorView/NBTTag/RenameShortcut1", "Application/EditorView/NBTTag/RenameShortcut2"}, this.RenameCommand));
            list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/CopyNameShortcut", this.CopyNameCommand));
            if (this is TagPrimitiveViewModel primitive) {
                list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/CopyValueShortcut", primitive.CopyValueCommand));
            }

            list.Add(new CommandContextEntry("Copy (Binary)", this.CopyBinaryToClipboardCommand));
            // list.Add(SeparatorEntry.Instance);
            if (this is TagDataFileViewModel datFileAgain) {
                list.Add(SeparatorEntry.Instance);
                list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/RemoveFromParent", this.RemoveFromParentCommand));
                list.Add(new CommandContextEntry("Delete FILE", datFileAgain.DeleteFileCommand));
            }
            else {
                list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/RemoveFromParent", this.RemoveFromParentCommand));
            }
        }
    }
}