using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Views.NBT.Finding.Inlines;
using TextRange = MCNBTEditor.Core.Utils.TextRange;

namespace MCNBTEditor.Views.NBT.Finding {
    public class InlinesTagNameValueConverter : BaseInlineHighlightConverter, IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null || values.Length != 4) {
                throw new Exception("Expected 4 values, got " + (values != null ? values.Length.ToString() : "null"));
            }

            BaseTagViewModel nbt = (BaseTagViewModel) values[0];
            string primitiveOrArrayFoundValue = (string) values[1];
            List<TextRange> nameMatches = (List<TextRange>) values[2];
            List<TextRange> valueMatches = (List<TextRange>) values[3];

            List<Run> output = new List<Run>();
            if (!string.IsNullOrEmpty(nbt.Name)) {
                output.AddRange(this.CreateString(nbt.Name, nameMatches));
            }

            if (!string.IsNullOrEmpty(primitiveOrArrayFoundValue)) {
                bool hasName = output.Count > 0;
                if (hasName) {
                    output.Add(this.CreateNormalRun(" ("));
                }

                output.AddRange(this.CreateString(primitiveOrArrayFoundValue, valueMatches));
                if (hasName) {
                    output.Add(this.CreateNormalRun(")"));
                }
            }
            else if (nbt is TagPrimitiveViewModel primitive) {
                output.Add(this.CreateNormalRun(" (" + primitive.Data + ")"));
            }
            else if (nbt is TagIntArrayViewModel intArray) {
                output.Add(this.CreateNormalRun(" (" + string.Join(",", intArray.Data) + ")"));
            }
            else if (nbt is TagByteArrayViewModel byteArray) {
                output.Add(this.CreateNormalRun(" (" + string.Join(",", byteArray.Data) + ")"));
            }

            return output;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}