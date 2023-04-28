using System.Windows;
using System.Windows.Controls;
using MCNBTEditor.Core.AdvancedContextService;

namespace MCNBTEditor.AdvancedContextService {
    public class AdvancedMenuItemStyleSelector : StyleSelector {
        public Style NonCheckableCommandMenuItemStyle { get; set; }
        public Style CheckableCommandMenuItemStyle { get; set; }

        public Style NonCheckableActionMenuItemStyle { get; set; }
        public Style CheckableActionMenuItemStyle { get; set; }

        public Style GroupingMenuItemStyle { get; set; }
        public Style ShortcutCommandMenuItemStyle { get; set; }

        public Style DefaultAdvancedMenuItemStyle { get; set; }
        public Style DefaultMenuItemStyle { get; set; }

        public Style SeparatorStyle { get; set; }

        public AdvancedMenuItemStyleSelector() {

        }

        public override Style SelectStyle(object item, DependencyObject container) {
            if (container is MenuItem) {
                switch (item) {
                    case ActionCheckableContextEntry  _: return this.CheckableActionMenuItemStyle ?? this.NonCheckableActionMenuItemStyle;
                    case CommandCheckableContextEntry _: return this.CheckableCommandMenuItemStyle ?? this.NonCheckableCommandMenuItemStyle;
                    case ActionContextEntry           _: return this.NonCheckableActionMenuItemStyle;
                    case CommandContextEntry          _: return this.NonCheckableCommandMenuItemStyle;
                    case ShortcutCommandContextEntry  _: return this.ShortcutCommandMenuItemStyle;
                    case GroupContextEntry            _: return this.GroupingMenuItemStyle;
                    default: return container is AdvancedMenuItem ? this.DefaultAdvancedMenuItemStyle : this.DefaultMenuItemStyle;
                }
            }
            else if (container is Separator) {
                return this.SeparatorStyle;
            }
            else {
                return base.SelectStyle(item, container);
            }
        }
    }
}