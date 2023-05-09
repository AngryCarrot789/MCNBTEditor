namespace MCNBTEditor.Core.Explorer.Actions {
    public static class ActionIds {
        public const string CopyBinaryAction = "actions.nbt.copy.binary";
        public const string PasteBinaryAction = "actions.nbt.paste.binary";

        public static IconType ResolveIcon(string actionId) {
            switch (actionId) {
                case "actions.item.Refresh":             return IconType.ACTION_ITEM_Refresh;
                case "actions.nbt.rename":               return IconType.ACTION_TAG_Rename;
                case "actions.nbt.remove_from_parent":   return IconType.ACTION_TAG_Delete;
                case "actions.nbt.copy.name":            return IconType.ACTION_TAG_CopyName;
                case "actions.nbt.copy.primitive_value": return IconType.ACTION_TAG_CopyValue;
                case CopyBinaryAction:                   return IconType.ACTION_TAG_CopyBinary;
                case PasteBinaryAction:                  return IconType.ACTION_TAG_PasteBinary;
            }

            return IconType.None;
        }
    }
}