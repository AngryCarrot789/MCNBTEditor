using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MCNBTEditor.Core.Actions;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Utils;
using MCNBTEditor.Core.Views.Dialogs.Message;

namespace MCNBTEditor.Core.Explorer.Actions {
    [ActionRegistration("actions.nbt.paste.binary.sysclipboard")]
    public class PasteBinaryAction : AnAction {
        public PasteBinaryAction() : base("Copy Value", "Copies the primitive tag's value to the system clipboard") {

        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (IoC.Clipboard == null) {
                await Dialogs.ClipboardUnavailableDialog.ShowAsync("Clipboard unavailable", "Clipboard is unavailable. Cannot copy tag value");
                return true;
            }


            if (!e.DataContext.TryGetContext(out BaseTreeItemViewModel selectedItem))
                return false;

            if (selectedItem.IsRoot)
                return false;

            int index;
            if (selectedItem.ParentItem is BaseTagCollectionViewModel targetTag) {
                if ((index = targetTag.IndexOfItem(selectedItem)) == -1) {
                    index = targetTag.ChildrenCount;
                }
            }
            else {
                await IoC.MessageDialogs.ShowMessageAsync("Nowhere to paste", $"Could not find a location to paste NBT??? Selected = {selectedItem}");
                return false;
            }

            bool compressed = false;
            bool bigEndian = false;
            switch (await CopyBinaryAction.TypeDialog.ShowAsync()) {
                case "bc": {
                    bigEndian = true;
                    compressed = true;
                    break;
                }
                case "bu": {
                    bigEndian = true;
                    break;
                }
                case "lc": {
                    compressed = true;
                    break;
                }
                case "lu": {
                    break;
                }
                default: return true;
            }

            List<(string, NBTBase)> tagList;
            try {
                byte[] array = IoC.Clipboard.GetBinaryTag("NBT_DODGY_COPIED_COMPOUND");
                if (array == null) {
                    await Dialogs.InvalidClipboardDataDialog.ShowAsync("Invalid clipboard", "Clipboard did not contain a copied tag");
                    return true;
                }

                using (MemoryStream stream = new MemoryStream(array)) {
                    NBTTagCompound tag = CompressedStreamTools.Read(stream, out _, compressed, bigEndian);
                    NBTTagInt lenTag = tag.map.TryGetValue("Length", out var lenTagBase) ? lenTagBase as NBTTagInt : null;
                    if (lenTag == null || lenTag.data < 0) {
                        await Dialogs.InvalidClipboardDataDialog.ShowAsync("Invalid clipboard", "Clipboard contained a corrupt copied tag (TAGLEN==NULL||LEN<0)");
                        return true;
                    }

                    tagList = new List<(string, NBTBase)>();
                    int length = lenTag.data;
                    for (int i = 0; i < length; i++) {
                        if (tag.map.TryGetValue(i.ToString(), out NBTBase tagA) && tagA is NBTTagCompound compound) {
                            if (!compound.map.TryGetValue("Name", out NBTBase nameBase) || !(nameBase is NBTTagString str)) {
                                continue;
                            }

                            if (!compound.map.TryGetValue("Value", out NBTBase valueBase)) {
                                continue;
                            }

                            tagList.Add((str.data, valueBase));
                        }
                    }
                }
            }
            catch (Exception ex) {
                await IoC.MessageDialogs.ShowMessageExAsync("Error saving tags", "Exception while deserialising tags", ex.ToString());
                return true;
            }

            targetTag.InsertItems(Maths.Clamp(index + 1, 0, targetTag.ChildrenCount), tagList);
            return true;
        }
    }
}