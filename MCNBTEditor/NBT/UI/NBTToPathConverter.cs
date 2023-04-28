using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MCNBTEditor.Core.Explorer.NBT;

namespace MCNBTEditor.NBT.UI {
    public class NBTToPathConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value is BaseTagViewModel nbt ? nbt.TreeFullPath : DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}