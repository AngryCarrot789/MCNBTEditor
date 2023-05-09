using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MCNBTEditor.Core.Actions;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Views.Dialogs.Message;
using MCNBTEditor.Core.Views.Dialogs.Modal;

namespace MCNBTEditor.Core.Explorer.Actions {
    public class CopyBinaryAction : MultiSelectionAction {
        public static readonly MessageDialog TypeDialog;

        static CopyBinaryAction() {
            TypeDialog = new MessageDialog("bc") {ShowAlwaysUseNextResultOption = true};
            TypeDialog.Titlebar = "Serialisation/Deserialisation";
            TypeDialog.Message = "Select how you would like to serialise/deserialise the NBT data";
            DialogButton btn1 = TypeDialog.AddButton("BE+Compress", "bc", true);
            btn1.ToolTip = "Big-endian binary format and compressed using GZIP\n" +
                           "This is the default option and recommended option, as minecraft saves in big-endian by default, and compressed data is easily detectable due to the GZIP header";
            TypeDialog.AddButton("BE+Uncompressed", "bu", true).ToolTip = "Big-endian binary format without compression (uncompressed binary)";
            TypeDialog.AddButton("LE+Compress", "lc", true).ToolTip = "Little-endian binary format and compressed using GZIP";
            TypeDialog.AddButton("LE+Uncompressed", "lu", true).ToolTip = "Little-endian binary format without compression (uncompressed binary)";
            TypeDialog.AddButton("Cancel", "cancel", false).ToolTip = "Cancel the copy action";
        }

        public CopyBinaryAction() : base("Copy (Binary)", "Serialises the tag into binary and sets the system clipboard to that data") {

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
                compound.Put("Length", new NBTTagInt(tags.Count));
                for (int i = 0; i < tags.Count; i++) {
                    NBTTagCompound innerTag = new NBTTagCompound();
                    innerTag.Put("Name", new NBTTagString(tags[i].Name));
                    innerTag.Put("Value", tags[i].ToNBT());
                    compound.Put(i.ToString(), innerTag);
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