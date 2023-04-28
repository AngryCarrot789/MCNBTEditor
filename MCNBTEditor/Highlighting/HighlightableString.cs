using System.Collections.Generic;
using MCNBTEditor.Core;
using MCNBTEditor.Core.Utils;

namespace MCNBTEditor.Highlighting {
    public class HighlightableString : BaseViewModel {
        private string text;
        public string Text {
            get => this.text;
            set => this.RaisePropertyChanged(ref this.text, value);
        }

        private IEnumerable<TextRange> highlighting;
        public IEnumerable<TextRange> Highlighting {
            get => this.highlighting;
            set => this.RaisePropertyChanged(ref this.highlighting, value);
        }

        public HighlightableString() : this(null, null) {
        }

        public HighlightableString(string text) : this(text, null) {
        }

        public HighlightableString(string text, IEnumerable<TextRange> highlighting) {
            this.highlighting = highlighting;
            this.text = text;
        }
    }
}