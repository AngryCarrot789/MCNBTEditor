using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MCNBTEditor.Core.Actions;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Views.Dialogs.Message;
using REghZy.Streams;

namespace MCNBTEditor.Core.Explorer.Actions {
    [ActionRegistration("actions.nbt.copy.binary.sysclipboard")]
    public class CopyBinaryAction : ExtendedListActionBase {
        public static readonly MessageDialog TypeDialog;

        static CopyBinaryAction() {
            TypeDialog = new MessageDialog {ShowAlwaysUseNextResultOption = true};
            TypeDialog.AddButton("BE+Compress", "bc", true);
            TypeDialog.AddButton("BE+Uncompressed", "bu", true);
            TypeDialog.AddButton("LE+Compress", "lc", true);
            TypeDialog.AddButton("LE+Uncompressed", "lu", true);
            TypeDialog.AddButton("Cancel", "cancel", false);
        }

        public CopyBinaryAction() : base("Copy Value", "Copies the primitive tag's value to the system clipboard") {

        }

        public override async Task<bool> ExecuteSelectionAsync(AnActionEventArgs e, IEnumerable<BaseTreeItemViewModel> selection) {
            if (IoC.Clipboard == null) {
                await Dialogs.ClipboardUnavailableDialog.ShowAsync("Clipboard unavailable", "Clipboard is unavailable. Cannot copy tag value");
                return true;
            }

            List<BaseTagViewModel> tags = selection.OfType<BaseTagViewModel>().ToList();
            if (tags.Count < 1)
                return true;

            bool compressed = false;
            bool bigEndian = false;
            switch (await TypeDialog.ShowAsync()) {
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

            try {
                NBTTagCompound compound = new NBTTagCompound();
                compound.Put("VERY_DODGY_IS_ACTION_BASED", NBTTagByte.TrueValue);
                for (int i = 0; i < tags.Count; i++) {
                    compound.Put(i.ToString(), tags[i].ToNBT());
                }

                using (MemoryStream stream = new MemoryStream(4096)) {
                    CompressedStreamTools.Write(compound, stream, compressed, bigEndian);
                    IoC.Clipboard.SetBinaryTag("NBT_DODGY_COPIED_COMPOUND", stream.ToArray());
                }
            }
            catch (Exception ex) {
                await IoC.MessageDialogs.ShowMessageExAsync("Error saving tags", "Exception while serialising tags", ex.ToString());
            }

            return true;
        }
    }
}