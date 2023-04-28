using System.Windows;
using System.Windows.Documents;

namespace MCNBTEditor.NBT.UI {
    public abstract class BaseNBTHeaderRunConverter {
        public Style TagNameRunStyle { get; set; }
        public Style TagDataRunStyle { get; set; }

        public Run CreateNameRun(string text) {
            Run run = this.TagNameRunStyle != null ? new Run() { Style = this.TagNameRunStyle } : new Run();
            run.Text = text;
            return run;
        }

        public Run CreateDataRun(string text) {
            Run run = this.TagDataRunStyle != null ? new Run() { Style = this.TagDataRunStyle } : new Run() {
                FontStyle = FontStyles.Italic
            };

            run.Text = text;
            return run;
        }
    }
}