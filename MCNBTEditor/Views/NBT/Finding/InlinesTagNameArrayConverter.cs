using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Views.NBT.Finding.Inlines;
using TextRange = MCNBTEditor.Core.Utils.TextRange;

namespace MCNBTEditor.Views.NBT.Finding {
    public class InlinesTagNameArrayConverter : BaseInlineHighlightConverter, IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null || values.Length != 3) {
                throw new Exception("Expected 4 values, got " + (values != null ? values.Length.ToString() : "null"));
            }

            BaseTagViewModel nbt = (BaseTagViewModel) values[0];
            List<TextRange> nameMatches = (List<TextRange>) values[1];

            List<Run> output = new List<Run>();
            if (!string.IsNullOrEmpty(nbt.Name)) {
                output.AddRange(this.CreateString(nbt.Name, nameMatches));
            }

            return output;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}