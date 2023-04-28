using System.Windows;
using System.Windows.Controls;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.Explorer.Regions;

namespace MCNBTEditor.NBT.UI {
    public class ExtendedTreeItemTemplateSelector : DataTemplateSelector {
        public DataTemplate TagPrimitive { get; set; }
        public DataTemplate TagArray { get; set; }
        public DataTemplate TagList { get; set; }
        public DataTemplate TagCompound { get; set; }
        public DataTemplate RegionFile { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            switch (item) {
                case TagPrimitiveViewModel _:   return this.TagPrimitive;
                case BaseTagArrayViewModel _:   return this.TagArray;
                case TagListViewModel _:        return this.TagList;
                case TagCompoundViewModel _:    return this.TagCompound;
                case RegionFileViewModel _:     return this.RegionFile;
                default:                        return base.SelectTemplate(item, container);
            }
        }
    }
}