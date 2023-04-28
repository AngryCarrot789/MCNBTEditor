using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MCNBTEditor.Core.Actions;

namespace MCNBTEditor.Shortcuts.Converters {
    public class ActionIdToHeaderConverter : IValueConverter {
        public static ActionIdToHeaderConverter Instance { get; } = new ActionIdToHeaderConverter();

        public string NoSuchActionText { get; set; } = null;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string id) {
                return ActionIdToHeader(id, this.NoSuchActionText, out string header) ? header : DependencyProperty.UnsetValue;
            }

            throw new Exception("Value is not a string");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public static bool ActionIdToHeader(string id, string fallback, out string header) {
            AnAction action = ActionManager.Instance.GetAction(id);
            if (action == null) {
                return (header = fallback) != null;
            }

            header = action.Header();
            return (header ?? fallback) != null;
        }
    }
}