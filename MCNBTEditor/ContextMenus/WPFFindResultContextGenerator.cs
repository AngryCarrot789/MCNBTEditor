using System.Collections.Generic;
using System.Windows;
using MCNBTEditor.AdvancedContextService;
using MCNBTEditor.Core.Actions.Contexts;
using MCNBTEditor.Core.AdvancedContextService;
using MCNBTEditor.Core.Explorer.Actions;
using MCNBTEditor.Core.Explorer.Context;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Views.NBT.Finding;

namespace MCNBTEditor.ContextMenus {
    public class WPFFindResultContextGenerator : FindResultContextGenerator, IWPFContextGenerator {
        public static WPFFindResultContextGenerator Instance { get; } = new WPFFindResultContextGenerator();

        public void Generate(List<IContextEntry> list, DependencyObject sender, DependencyObject target, object context) {
            if (context is NBTMatchResult result) {
                this.Generate(list, result);
            }
        }

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (context.TryGetContext(out NBTMatchResult result)) {
                this.Generate(list, result);
            }
        }

        public void Generate(List<IContextEntry> list, NBTMatchResult result) {
            list.Add(new CommandContextEntry("Navigate", result.NavigateToItemCommand));
            list.Add(SeparatorEntry.Instance);
            BaseTagViewModel tag = result.NBT;
            if (tag is TagDataFileViewModel) {
                list.Add(new ActionContextEntry(tag, "actions.item.CopyFilePath", "Copy file path", "Copies this .DAT file's file path to the system clipboard"));
                list.Add(new ActionContextEntry(tag, "actions.item.OpenInExplorer", "Show in Explorer", "Opens the windows file explorer with this .DAT actual file's selected"));
                list.Add(SeparatorEntry.Instance);
            }

            list.Add(new ActionContextEntry(tag, "actions.nbt.copy.name", "Copy Name", "Copies this tag's name to the system clipboard"));
            if (tag is TagPrimitiveViewModel) {
                list.Add(new ActionContextEntry(tag, "actions.nbt.copy.primitive_value", "Copy Value", "Copies this primitive tag's value to the system clipboard"));
            }

            list.Add(new ActionContextEntry(tag, ActionIds.CopyBinaryAction, "Copy (binary)", "Serialises this tag and sets it as the system clipboard (does not directly serialise the tag, but instead a tag compound containing the tag)"));
            list.Add(SeparatorEntry.Instance);
        }
    }
}