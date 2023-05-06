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
                    list.Add(new ShortcutCommandContextEntry("Copy file path", "Copies this .DAT file's file path to the system clipboard", "Application/EditorView/NBTTag/CopyFilePath", datFile.CopyFilePathToClipboardCommand));
                    list.Add(new ShortcutCommandContextEntry("Open in Explorer", "Opens the windows file explorer with this .DAT actual file's select", "Application/EditorView/NBTTag/OpenInExplorer", datFile.ShowInExplorerCommand));
                    list.Add(SeparatorEntry.Instance);
                }

                list.Add(new ActionContextEntry(tag, "actions.nbt.copy.name", "Copy Name", "Copies this tag's name to the system clipboard"));
                if (tag is TagPrimitiveViewModel) {
                    list.Add(new ActionContextEntry(tag, "actions.nbt.copy.primitive_value", "Copy Value", "Copies this primitive tag's value to the system clipboard"));
                }

                list.Add(new ActionContextEntry(tag, "actions.nbt.copy.binary.sysclipboard", "Copy (binary)", "Serialises this tag and sets it as the system clipboard (does not directly serialise the tag, but instead a tag compound containing the tag)"));
                list.Add(SeparatorEntry.Instance);
            }
        }
    }
}