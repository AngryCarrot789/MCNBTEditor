using System.Collections.Generic;

namespace MCNBTEditor.Core.AdvancedContextService {
    public class GroupContextEntry : BaseContextEntry {
        private string header;
        public string Header {
            get => this.header;
            set => this.RaisePropertyChanged(ref this.header, value);
        }

        private string toolTip;
        public string ToolTip {
            get => this.toolTip;
            set => this.RaisePropertyChanged(ref this.toolTip, value);
        }

        public GroupContextEntry(object dataContext, string header, string toolTip, IEnumerable<IContextEntry> children = null) : base(dataContext, children) {
            this.header = header;
            this.toolTip = toolTip;
        }

        public GroupContextEntry(string header, string toolTip, IEnumerable<IContextEntry> children = null) : this(null, header, toolTip, children) {

        }

        public GroupContextEntry(object dataContext, string header, IEnumerable<IContextEntry> children = null) : this(dataContext, header, null, children) {

        }

        public GroupContextEntry(string header, IEnumerable<IContextEntry> children = null) : this(null, header, null, children) {

        }
    }
}