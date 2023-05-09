using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MCNBTEditor.AdvancedContextService;
using MCNBTEditor.Core.AdvancedContextService;
using MCNBTEditor.Core.Explorer;
using MCNBTEditor.Core.Explorer.Context;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.Explorer.Regions;

namespace MCNBTEditor.ContextMenus {
    public class WPFTreeItemContextGenerator : TreeItemContextGenerator, IWPFContextGenerator {
        public static WPFTreeItemContextGenerator Instance { get; } = new WPFTreeItemContextGenerator();

        public void Generate(List<IContextEntry> list, DependencyObject source, DependencyObject target, object context) {
            bool isMultiSelect = false;
            if (source is IExtendedList extendedList) { // ListBox
                isMultiSelect = extendedList.SelectedItems.Count() > 1;
            }

            if (context is BaseTagViewModel tag) {
                this.GenerateForTag(list, tag, isMultiSelect);
            }
            else if (context is RegionFileViewModel region) {
                this.GenerateForRegion(list, region, isMultiSelect);
            }
        }
    }
}