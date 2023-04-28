using MCNBTEditor.Core.Views.Dialogs.FilePicking;

namespace MCNBTEditor.Core.Utils {
    public static class Filters {
        public static readonly string NBTCommonTypes =
            Filter.Of().Add("DAT File", "dat", "dat_old").Add("NBT File", "nbt").Add("Schematic", "schematic").Add("MCR", "dat_mcr").Add("BTP", "bpt").Add("RC", "rc").ToString();
        public static readonly string NBTCommonTypesWithAllFiles = new Filter(NBTCommonTypes).AddAllFiles().ToString();
    }
}