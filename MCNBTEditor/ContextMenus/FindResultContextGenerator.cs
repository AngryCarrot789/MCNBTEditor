using System.Collections.Generic;
using System.Windows;
using MCNBTEditor.AdvancedContextService;
using MCNBTEditor.Core.AdvancedContextService;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Views.NBT.Finding;

namespace MCNBTEditor.ContextMenus {
    public class FindResultContextGenerator : IWPFContextGenerator {
        public static FindResultContextGenerator Instance { get; } = new FindResultContextGenerator();

        public void Generate(List<IContextEntry> list, DependencyObject sender, DependencyObject target, object context) {
            if (context is NBTMatchResult result) {
                list.Add(new CommandContextEntry("Navigate", result.NavigateToItemCommand));
                list.Add(SeparatorEntry.Instance);
                BaseTagViewModel tag = result.NBT;
                if (tag is TagDataFileViewModel datFile) {
                    list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/CopyFilePath", datFile.CopyFilePathToClipboardCommand));
                    list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/OpenInExplorer", datFile.ShowInExplorerCommand));
                    list.Add(SeparatorEntry.Instance);
                }

                list.Add(new ActionContextEntry(tag, "actions.nbt.copy.name"));
                if (tag is TagPrimitiveViewModel) {
                    list.Add(new ActionContextEntry(tag, "actions.nbt.copy.primitive_value"));
                }

                list.Add(new ActionContextEntry(tag, "actions.nbt.copy.binary.sysclipboard"));
                list.Add(SeparatorEntry.Instance);
            }
        }
    }
}