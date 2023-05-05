using MCNBTEditor.Core;

namespace MCNBTEditor.ColourMap.Maps.Brushes {
    public abstract class BrushItemViewModel : BaseMapItemViewModel {
        private bool hasBeenModified;
        public bool HasBeenModified {
            get => this.hasBeenModified;
            set => this.RaisePropertyChanged(ref this.hasBeenModified, value);
        }

        protected BrushItemViewModel(ColourSchemaViewModel schema, ColourMapViewModel parent, string id, string name) : base(schema, parent, id, name) {

        }
    }
}