using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MCNBTEditor.AdvancedContextService;
using MCNBTEditor.Core.AdvancedContextService;
using MCNBTEditor.Core.Explorer;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.Explorer.Regions;

namespace MCNBTEditor.NBT.ContextMenus {
    public class TreeItemContextGenerator : IWPFContextGenerator {
        public static TreeItemContextGenerator Instance { get; } = new TreeItemContextGenerator();

        public void Generate(List<IContextEntry> list, DependencyObject source, DependencyObject target, object context) {
            bool isMultiSelect = false;
            if (source is IExtendedList extendedList) { // ListBox
                isMultiSelect = extendedList.SelectedItems.Count() > 1;
            }

            if (context is BaseTagViewModel tag) {
                this.Generate(list, tag, isMultiSelect);
            }
            else if (context is RegionFileViewModel region) {
                this.Generate(list, region, isMultiSelect);
            }
        }

        public void Generate(List<IContextEntry> list, BaseTagViewModel tag, bool isMultiSelect) {
            if (isMultiSelect) {
                list.Add(new ActionContextEntry(tag, "actions.nbt.copy.name", "Copy Names"));
                if (tag is TagPrimitiveViewModel p2) {
                    list.Add(new ActionContextEntry(p2, "actions.nbt.copy.primitive_value", "Copy Values"));
                }

                list.Add(new ActionContextEntry(tag, "actions.nbt.copy.binary.sysclipboard"));
                list.Add(SeparatorEntry.Instance);
                if (tag is BaseTagCollectionViewModel) {
                    list.Add(new ActionContextEntry(tag, "actions.nbt.find"));
                    list.Add(SeparatorEntry.Instance);
                }

                list.Add(new ActionContextEntry(tag, "actions.nbt.remove_from_parent"));
                if (tag is TagDataFileViewModel dat) {
                    list.Add(new CommandContextEntry("Delete FILE", dat.DeleteFileCommand, "Delete FILES"));
                }
            }
            else {
                if (tag is BaseTagCollectionViewModel tagCollection) {
                    list.Add(new CommandContextEntry("Sort by Name", tagCollection.SortByNameCommand));
                    list.Add(new CommandContextEntry("Sort by Type", tagCollection.SortByTypeCommand));
                    list.Add(new CommandContextEntry("Sort by Both (default)", tagCollection.SortByBothCommand));
                }

                list.Add(SeparatorEntry.Instance);
                if (tag is TagDataFileViewModel datFile) {
                    list.Add(new CommandContextEntry("Refresh", datFile.RefreshDataCommand));
                    list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/Save", datFile.SaveFileCommand));
                    list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/SaveAs", datFile.SaveFileAsCommand));
                    list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/CopyFilePath", datFile.CopyFilePathToClipboardCommand));
                    list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/OpenInExplorer", datFile.ShowInExplorerCommand));
                }

                list.Add(SeparatorEntry.Instance);
                list.Add(new ActionContextEntry(tag, "actions.nbt.rename.tag"));
                if (tag is TagPrimitiveViewModel p2) {
                    list.Add(new CommandContextEntry("Edit...", p2.EditGeneralCommand));
                    list.Add(new ActionContextEntry(tag, "actions.nbt.copy.name"));
                    list.Add(new ActionContextEntry(tag, "actions.nbt.copy.primitive_value"));
                }
                else {
                    list.Add(new ActionContextEntry(tag, "actions.nbt.copy.name"));
                }

                list.Add(new ActionContextEntry(tag, "actions.nbt.copy.binary.sysclipboard"));
                list.Add(new ActionContextEntry(tag, "actions.nbt.paste.binary.sysclipboard"));
                list.Add(SeparatorEntry.Instance);
                if (tag is BaseTagCollectionViewModel) {
                    list.Add(new ActionContextEntry(tag, "actions.nbt.find"));
                    list.Add(SeparatorEntry.Instance);
                }

                list.Add(new ActionContextEntry(tag, "actions.nbt.remove_from_parent"));
                if (tag is TagDataFileViewModel datFileAgain) {
                    list.Add(new CommandContextEntry("Delete FILE", datFileAgain.DeleteFileCommand));
                }
            }
        }

        public void Generate(List<IContextEntry> list, RegionFileViewModel region, bool isMultiSelect) {
            list.Add(new CommandContextEntry("Refresh", region.RefreshCommand));
            list.Add(new CommandContextEntry("Save (coming soon)", region.SaveFileCommand));
            list.Add(new CommandContextEntry("Save as (coming soon)", region.SaveFileAsCommand));
            list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/CopyFilePath", region.CopyFilePathToClipboardCommand));
            list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/OpenInExplorer", region.ShowInExplorerCommand));

            list.Add(SeparatorEntry.Instance);
            list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/RemoveFromParent", region.RemoveFromParentCommand));
            list.Add(new CommandContextEntry("Delete FILE", region.DeleteFileCommand));
        }
    }
}