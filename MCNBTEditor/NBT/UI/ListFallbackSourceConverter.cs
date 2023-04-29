using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MCNBTEditor.Core.Explorer;

namespace MCNBTEditor.NBT.UI {
    public class ListFallbackSourceConverter : IMultiValueConverter {
        public BaseTreeItemViewModel FallbackItem { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            foreach (object value in values) {
                if (value != null && value != DependencyProperty.UnsetValue) {
                    return value;
                }
            }

            return this.FallbackItem ?? DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}