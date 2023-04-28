using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;

namespace MCNBTEditor.NBT.UI.Inlines {
    public class NBTPrimitiveInlineHeaderConverter : BaseNBTHeaderRunConverter, IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values.Length != 2) {
                throw new Exception("Expected 2 values: <original Name> <string data>");
            }

            List<Run> runs = new List<Run>();
            string name = values[0] as string;
            if (values[1] is object value) {
                if (string.IsNullOrEmpty(name)) {
                    runs.Add(this.CreateDataRun(value.ToString()));
                }
                else {
                    runs.Add(this.CreateNameRun(name + " "));
                    runs.Add(this.CreateDataRun("(" + value + ")"));
                }
            }
            else {
                runs.Add(this.CreateNameRun(name));
            }

            return runs;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}