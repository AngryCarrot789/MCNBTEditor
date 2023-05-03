using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;

namespace MCNBTEditor.NBT.UI.Inlines {
    public class NBTArrayInlineHeaderConverter : BaseNBTHeaderRunConverter, IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values.Length != 2) {
                throw new Exception("Expected 2 values: [original Name] [array instance]");
            }

            List<Run> runs = new List<Run>();
            string name = values[0] as string;
            if (!string.IsNullOrEmpty(name)) {
                runs.Add(this.CreateNameRun($"{name} "));
            }

            if (values[1] is int[] intArray) {
                runs.Add(this.CreateDataRun($"({intArray.Length} integer elements)"));
            }
            else if (values[1] is byte[] byteArray) {
                runs.Add(this.CreateDataRun($"({byteArray.Length} byte elements)"));
            }
            else if (values[1] is long[] longArray) {
                runs.Add(this.CreateDataRun($"({longArray.Length} long elements)"));
            }
            else {
                runs.Add(this.CreateNameRun($"<invalid array: {values[1]?.GetType()}>"));
            }

            return runs;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}