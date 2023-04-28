using System.Windows;
using System.Windows.Controls;
using MCNBTEditor.Core.Explorer.NBT;

namespace MCNBTEditor.Views.NBT.Finding {
    public class FindResultTemplateSelector : DataTemplateSelector {
        public DataTemplate PrimitiveNBTTemplate { get; set; }
        public DataTemplate ArrayNBTTemplate { get; set; }
        public DataTemplate ListNBTTemplate { get; set; }
        public DataTemplate CompoundNBTTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (item is NBTMatchResult result) {
                switch (result.NBT) {
                    case TagPrimitiveViewModel _: return this.PrimitiveNBTTemplate;
                    case BaseTagArrayViewModel _: return this.ArrayNBTTemplate;
                    case TagListViewModel _: return this.ListNBTTemplate;
                    case TagCompoundViewModel _: return this.CompoundNBTTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}