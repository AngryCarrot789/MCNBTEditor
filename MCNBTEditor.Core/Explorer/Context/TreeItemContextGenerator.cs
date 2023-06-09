using System;
using System.Collections.Generic;
using System.Linq;
using MCNBTEditor.Core.Actions.Contexts;
using MCNBTEditor.Core.AdvancedContextService;
using MCNBTEditor.Core.Explorer.Actions;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.Explorer.Regions;
using MCNBTEditor.Core.NBT;

namespace MCNBTEditor.Core.Explorer.Context {
    public class TreeItemContextGenerator : IContextGenerator {
        public void Generate(List<IContextEntry> list, IDataContext context) {
            bool isMultiSelect = false;
            if (context.TryGetContext(out IExtendedList extendedList)) {
                isMultiSelect = extendedList.SelectedItems.Count() > 1;
            }

            if (context.TryGetContext(out BaseTagViewModel tag)) {
                this.GenerateForTag(list, tag, isMultiSelect);
            }
            else if (context.TryGetContext(out RegionFileViewModel region)) {
                this.GenerateForRegion(list, region, isMultiSelect);
            }
        }

        public static string GetNewLineDescriptor() {
            switch (Environment.NewLine) {
                case "\n":   return "a new line/line feed (\\n) char";
                case "\r":   return "a carriage return (\\r) char";
                case "\r\n": return "a CRLF (\\r\\n) sequence";
                default:     return $"the system's new line text sequence ({Environment.NewLine.Length} chars)";
            }
        }

        public void GenerateForTag(List<IContextEntry> list, BaseTagViewModel tag, bool isMultiSelect) {
            if (isMultiSelect) {
                string newLineDesc = GetNewLineDescriptor();
                list.Add(new ActionContextEntry(tag, "actions.nbt.copy.name", "Copy Names", $"Copy these tags' names as a string separated by {newLineDesc}"));
                if (tag is TagPrimitiveViewModel p2) {
                    list.Add(new ActionContextEntry(p2, "actions.nbt.copy.primitive_value", "Copy Values", $"Copy these tags' primitive values as a string separated by {newLineDesc}"));
                }

                list.Add(new ActionContextEntry(tag, ActionIds.CopyBinaryAction, "Copy (binary)"));
                list.Add(SeparatorEntry.Instance);
                list.Add(new ActionContextEntry(tag, "actions.nbt.remove_from_parent", "Remove tags", "Removes these tags from their parent tag"));
            }
            else {
                if (tag is BaseTagCollectionViewModel tagCollection) {
                    if (this.GenerateNewItems(list, tagCollection)) {
                        list.Add(SeparatorEntry.Instance);
                    }

                    list.Add(new CommandContextEntry("Sort by Name", tagCollection.SortByNameCommand));
                    list.Add(new CommandContextEntry("Sort by Type", tagCollection.SortByTypeCommand));
                    list.Add(new CommandContextEntry("Sort by Both (default)", tagCollection.SortByBothCommand));
                }

                list.Add(SeparatorEntry.Instance);
                if (tag is TagDataFileViewModel datFile) {
                    list.Add(new CommandContextEntry("Refresh", datFile.RefreshDataCommand));
                    list.Add(new ShortcutCommandContextEntry("Save", null, "Application/EditorView/NBTTag/Save", datFile.SaveFileCommand));
                    list.Add(new ShortcutCommandContextEntry("Save As...", null, "Application/EditorView/NBTTag/SaveAs", datFile.SaveFileAsCommand));
                    list.Add(new ShortcutCommandContextEntry("Copy file path", null, "Application/EditorView/NBTTag/CopyFilePath", datFile.CopyFilePathToClipboardCommand));
                    list.Add(new ShortcutCommandContextEntry("Open in Explorer", null, "Application/EditorView/NBTTag/OpenInExplorer", datFile.OpenInExplorerCommand));
                }

                list.Add(SeparatorEntry.Instance);
                list.Add(new ActionContextEntry(tag, "actions.nbt.rename", "Rename", "Renames this tag"));
                if (tag is TagPrimitiveViewModel p2) {
                    list.Add(new CommandContextEntry("Edit...", p2.EditGeneralCommand));
                    list.Add(new ActionContextEntry(tag, "actions.nbt.copy.name", "Copy Name", "Copy this tag's name to the clipboard"));
                    list.Add(new ActionContextEntry(tag, "actions.nbt.copy.primitive_value", "Copy Value", "Copies this tag's value to the clipboard"));
                }
                else {
                    list.Add(new ActionContextEntry(tag, "actions.nbt.copy.name", "Copy Name", "Copy this tag's name to the clipboard"));
                }

                list.Add(new ActionContextEntry(tag, ActionIds.CopyBinaryAction, "Copy (binary)"));
                list.Add(new ActionContextEntry(tag, ActionIds.PasteBinaryAction, "Paste (binary)"));
                list.Add(SeparatorEntry.Instance);
                if (tag is BaseTagCollectionViewModel) {
                    list.Add(new ActionContextEntry(tag, "actions.nbt.find", "Find"));
                    list.Add(SeparatorEntry.Instance);
                }

                list.Add(new ActionContextEntry(tag, "actions.nbt.remove_from_parent", "Remove"));
                if (tag is TagDataFileViewModel datFileAgain) {
                    list.Add(new CommandContextEntry("Delete FILE", datFileAgain.DeleteFileCommand));
                }
            }
        }

        public void GenerateForRegion(List<IContextEntry> list, RegionFileViewModel region, bool isMultiSelect) {
            list.Add(new CommandContextEntry("Refresh", region.RefreshCommand));
            list.Add(new CommandContextEntry("Save (coming soon)", region.SaveFileCommand));
            list.Add(new CommandContextEntry("Save as (coming soon)", region.SaveFileAsCommand));
            list.Add(new ActionContextEntry(region, "actions.item.CopyFilePath", "Copy file path", "Copies this .DAT file's file path to the system clipboard"));
            list.Add(new ActionContextEntry(region, "actions.item.OpenInExplorer", "Show in Explorer", "Opens the windows file explorer with this .DAT actual file's selected"));

            list.Add(SeparatorEntry.Instance);
            list.Add(new ActionContextEntry(region, "actions.nbt.remove_from_parent", "Remove", "Removes the region from the tree"));
            list.Add(new CommandContextEntry("Delete FILE", region.DeleteFileCommand));
        }

        public bool GenerateNewItems(List<IContextEntry> list, BaseTagCollectionViewModel tag) {
            if (tag is TagCompoundViewModel || (tag is TagListViewModel tagList && tagList.ChildrenCount < 1)) {
                List<IContextEntry> entries = new List<IContextEntry>();
                this.GetAllNewItems(entries, tag);
                list.Add(new GroupContextEntry(tag, "New...", "All possible types of tags to create", entries));
                return true;
            }
            else if (tag is TagListViewModel tagList2) {
                switch (tagList2.TargetType) {
                    case NBTType.End: break;
                    case NBTType.Byte:      list.Add(CreateAction("actions.nbt.newtag", "Add new Byte", "Create new NBTTagByte", NewTagAction.TypeKey, NBTType.Byte)); break;
                    case NBTType.Short:     list.Add(CreateAction("actions.nbt.newtag", "Add new Short", "Create new NBTTagShort", NewTagAction.TypeKey, NBTType.Short)); break;
                    case NBTType.Int:       list.Add(CreateAction("actions.nbt.newtag", "Add new Int", "Create new NBTTagInt", NewTagAction.TypeKey, NBTType.Int)); break;
                    case NBTType.Long:      list.Add(CreateAction("actions.nbt.newtag", "Add new Long", "Create new NBTTagLong", NewTagAction.TypeKey, NBTType.Long)); break;
                    case NBTType.Float:     list.Add(CreateAction("actions.nbt.newtag", "Add new Float", "Create new NBTTagFloat", NewTagAction.TypeKey, NBTType.Float)); break;
                    case NBTType.Double:    list.Add(CreateAction("actions.nbt.newtag", "Add new Double", "Create new NBTTagDouble", NewTagAction.TypeKey, NBTType.Double)); break;
                    case NBTType.String:    list.Add(CreateAction("actions.nbt.newtag", "Add new String", "Create new NBTTagString", NewTagAction.TypeKey, NBTType.String)); break;
                    case NBTType.ByteArray: list.Add(CreateAction("actions.nbt.newtag", "Add new Byte Array", "Create new NBTTagByteArray", NewTagAction.TypeKey, NBTType.ByteArray)); break;
                    case NBTType.IntArray:  list.Add(CreateAction("actions.nbt.newtag", "Add new Int Array", "Create new NBTTagIntArray", NewTagAction.TypeKey, NBTType.IntArray)); break;
                    case NBTType.LongArray: list.Add(CreateAction("actions.nbt.newtag", "Add new Long Array", "Create new NBTTagLongArray", NewTagAction.TypeKey, NBTType.LongArray)); break;
                    case NBTType.List:      list.Add(CreateAction("actions.nbt.newtag", "Add new List", "Create new NBTTagList", NewTagAction.TypeKey, NBTType.List)); break;
                    case NBTType.Compound:  list.Add(CreateAction("actions.nbt.newtag", "Add new Compound", "Create new NBTTagCompound", NewTagAction.TypeKey, NBTType.Compound)); break;
                    default: throw new ArgumentOutOfRangeException();
                }

                return true;
            }

            return false;
        }

        public void GetAllNewItems(List<IContextEntry> list, BaseTagCollectionViewModel target) {
            list.Add(CreateAction("actions.nbt.newtag", "Byte", "Create new NBTTagByte", NewTagAction.TypeKey, NBTType.Byte));
            list.Add(CreateAction("actions.nbt.newtag", "Short", "Create new NBTTagShort", NewTagAction.TypeKey, NBTType.Short));
            list.Add(CreateAction("actions.nbt.newtag", "Int", "Create new NBTTagInt", NewTagAction.TypeKey, NBTType.Int));
            list.Add(CreateAction("actions.nbt.newtag", "Long", "Create new NBTTagLong", NewTagAction.TypeKey, NBTType.Long));
            list.Add(CreateAction("actions.nbt.newtag", "Float", "Create new NBTTagFloat", NewTagAction.TypeKey, NBTType.Float));
            list.Add(CreateAction("actions.nbt.newtag", "Double", "Create new NBTTagDouble", NewTagAction.TypeKey, NBTType.Double));
            list.Add(CreateAction("actions.nbt.newtag", "String", "Create new NBTTagString", NewTagAction.TypeKey, NBTType.String));
            list.Add(CreateAction("actions.nbt.newtag", "Byte Array", "Create new NBTTagByteArray", NewTagAction.TypeKey, NBTType.ByteArray));
            list.Add(CreateAction("actions.nbt.newtag", "Int Array", "Create new NBTTagIntArray", NewTagAction.TypeKey, NBTType.IntArray));
            list.Add(CreateAction("actions.nbt.newtag", "Long Array", "Create new NBTTagLongArray", NewTagAction.TypeKey, NBTType.LongArray));
            list.Add(CreateAction("actions.nbt.newtag", "List", "Create new NBTTagList", NewTagAction.TypeKey, NBTType.List));
            list.Add(CreateAction("actions.nbt.newtag", "Compound", "Create new NBTTagCompound", NewTagAction.TypeKey, NBTType.Compound));
        }

        private static ActionContextEntry CreateAction(string action, string header, string description, string key, object value) {
            ActionContextEntry entry = new ActionContextEntry(null, action, header, description);
            entry.SetActionKey(key, value);
            return entry;
        }
    }
}