using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;

namespace MCNBTEditor.NBT.UI.Inlines {
    public class NBTCollectionInlineHeaderConverter : BaseNBTHeaderRunConverter, IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null || (values.Length != 2 && values.Length != 3)) {
                throw new Exception("Expected 3 values: <original Name> <number of items, or collection instance> [dummy count]");
            }

            List<Run> runs = new List<Run>();
            string name = values[0] as string;
            if (values[1] is int count) {
                if (string.IsNullOrEmpty(name)) {
                    runs.Add(this.CreateDataRun(count + " entries"));
                }
                else {
                    runs.Add(this.CreateNameRun(name + " "));
                    runs.Add(this.CreateDataRun($"({count} entries)"));
                }
            }
            else {
                runs.Add(string.IsNullOrEmpty(name) ? this.CreateNameRun("<weird tag>") : this.CreateNameRun(name));
            }

            return runs;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}