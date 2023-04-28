using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MCNBTEditor.Core.Actions;

namespace MCNBTEditor.Shortcuts.Converters {
    public class ActionIdToToolTipConverter : IValueConverter {
        public static ActionIdToToolTipConverter Instance { get; } = new ActionIdToToolTipConverter();

        public string NoSuchActionText { get; set; } = null;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string id) {
                return ActionIdToToolTip(id, this.NoSuchActionText, out string tooltip) ? tooltip : DependencyProperty.UnsetValue;
            }

            throw new Exception("Value is not a string");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public static bool ActionIdToToolTip(string id, string fallback, out string tooltip) {
            AnAction action = ActionManager.Instance.GetAction(id);
            if (action == null) {
                return (tooltip = fallback) != null;
            }

            tooltip = action.Description();
            return (tooltip ?? fallback) != null;
        }
    }
}