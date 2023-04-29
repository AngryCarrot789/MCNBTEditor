using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Views.Dialogs.Message;

namespace MCNBTEditor.Core.Explorer.NBT {
    public abstract class BaseTagCollectionViewModel : BaseTagViewModel, IHaveChildren {
        public override bool CanHoldChildren => true;

        /// <summary>
        /// Returns an enumerable which selects only <see cref="BaseTagViewModel"/> from this item's children. Realistically, all of them should be NBT tags
        /// </summary>
        public IEnumerable<BaseTagViewModel> ChildTags => this.Children.OfType<BaseTagViewModel>();

        /// <summary>
        /// A helper property for finding the first child that is a <see cref="BaseTagViewModel"/>
        /// </summary>
        public BaseTagViewModel FirstTag => this.GetFirstChild<BaseTagViewModel>();

        public RelayCommand SortByTypeCommand { get; }
        public RelayCommand SortByNameCommand { get; }
        public RelayCommand SortByBothCommand { get; }

        public AsyncRelayCommand PasteBinaryTagCommand { get; }

        protected BaseTagCollectionViewModel(string name, NBTType type) : base(name, type) {
            this.SortByTypeCommand = new RelayCommand(() => {
                List<BaseTagViewModel> list = this.ChildTags.OrderByDescending((a) => a.NBTType).ToList();
                this.ApplySort(list);
            });

            this.SortByNameCommand = new RelayCommand(() => {
                List<BaseTagViewModel> list = this.ChildTags.OrderBy((a) => a.Name ?? "").ToList();
                this.ApplySort(list);
            });

            this.SortByBothCommand = new RelayCommand(() => {
                List<BaseTagViewModel> list = new List<BaseTagViewModel>(this.ChildTags);
                list.Sort((a, b) => {
                    int cmp = a.NBTType.Compare4(b.NBTType);
                    return cmp != 0 ? cmp : string.Compare(a.Name ?? "", b.Name ?? "", StringComparison.CurrentCulture);
                });

                this.ApplySort(list);
            });

            this.PasteBinaryTagCommand = new AsyncRelayCommand(this.PasteBinaryTagAction);
        }

        public async Task PasteBinaryTagAction() {
            if (IoC.Clipboard == null) {
                await MessageDialogs.ClipboardUnavailableDialog.ShowAsync("Clipboard unavailable", "Clipboard is unavailable. Cannot paste");
                return;
            }

            byte[] bytes = IoC.Clipboard.GetBinaryTag("TAG_NBT");
            if (bytes == null || bytes.Length < 1) {
                await MessageDialogs.InvalidClipboardDataDialog.ShowAsync("Invalid clipboard", "Clipboard did not contain a copied tag");
                return;
            }

            using (MemoryStream stream = new MemoryStream(bytes)) {
                bool result;
                string tagName;
                NBTBase nbt;
                try {
                    result = NBTBase.ReadTag(CompressedStreamTools.CreateInput(stream), 0, out tagName, out nbt);
                }
                catch (Exception e) {
                    await MessageDialogs.InvalidClipboardDataDialog.ShowAsync("Invalid clipboard", "Failed to read NBT from clipboard: " + e.Message);
                    return;
                }

                if (!result) {
                    await MessageDialogs.InvalidClipboardDataDialog.ShowAsync("Invalid clipboard", "Clipboard did not contain a valid tag? That's weird...");
                    return;
                }

                await this.PasteBinaryTagDataAction(tagName, nbt);
            }
        }

        public abstract Task PasteBinaryTagDataAction(string name, NBTBase nbt);

        public void AddChild(BaseTagViewModel nbt) {
            base.Add(nbt);
        }

        public void AddItems(IEnumerable<BaseTagViewModel> nbt) {
            base.AddRange(nbt);
        }

        public void AddItem(NBTTagCompound nbt, bool attemptAutoSort = true) {
            if (nbt.map.Count < 1) {
                return;
            }

            if (attemptAutoSort && IoC.SortTagCompoundByDefault) {
                List<KeyValuePair<string, NBTBase>> list = new List<KeyValuePair<string, NBTBase>>(nbt.map);
                list.Sort((a, b) => {
                    int cmp = a.Value.TagType.Compare4(b.Value.TagType);
                    return cmp != 0 ? cmp : string.Compare(a.Key ?? "", b.Key ?? "", StringComparison.CurrentCulture);
                });

                base.AddRange(list.Select(x => CreateFrom(x.Key, x.Value)));
            }
            else {
                base.AddRange(nbt.map.Select(x => CreateFrom(x.Key, x.Value)));
            }
        }

        public void AddItem(NBTTagList nbt) {
            base.AddRange(nbt.tags.Select(x => CreateFrom(null, x)));
        }

        protected void ApplySort(List<BaseTagViewModel> sorted) {
            base.ClearAndAddRange(sorted);
            // for (int index = 0; index < sorted.Count; index++) {
            //     this.children.Move(this.children.IndexOf(sorted[index]), index);
            // }
        }

        protected override void EnsureChild(BaseTreeItemViewModel child, bool valid) {
            if (child is BaseTagViewModel tag) {
                if (valid) {
                    base.EnsureChild(child, true);
                    tag.OnAddedToParent();
                }
                else {
                    tag.OnRemovedFromParent();
                    base.EnsureChild(child, true);
                }
            }
            else {
                base.EnsureChild(child, valid);
            }
        }

        public BaseTagViewModel FindChildTagByName(string name) {
            return this.ChildTags.FirstOrDefault(x => x.Name == name);
        }

        public void AddItem(BaseTreeItemViewModel item) {
            base.Add(item);
        }

        public bool RemoveItem(BaseTreeItemViewModel item) {
            return base.Remove(item);
        }

        public int IndexOfItem(BaseTreeItemViewModel item) {
            return base.IndexOf(item);
        }

        public void ClearItems() {
            base.Clear();
        }
    }
}