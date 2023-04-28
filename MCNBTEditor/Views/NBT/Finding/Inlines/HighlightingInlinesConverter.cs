using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;
using TextRange = MCNBTEditor.Core.Utils.TextRange;

namespace MCNBTEditor.Views.NBT.Finding.Inlines {
    public class NameValueInlinesConverter : BaseInlineHighlightConverter, IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null || values.Length != 2) {
                throw new Exception("Expected 2 values, got " + (values == null ? "null" : values.Length.ToString()));
            }

            string text = (string) values[0];
            IEnumerable<TextRange> ranges = (IEnumerable<TextRange>) values[1];
            return string.IsNullOrEmpty(text) ? new List<Run>() : this.CreateString(text, ranges);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}