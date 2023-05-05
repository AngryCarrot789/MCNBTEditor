using MCNBTEditor.Core;

namespace MCNBTEditor.ColourMap.Maps.Brushes {
    public class BrushViewModel : BaseViewModel {
        private bool hasBeenModified;
        public bool HasBeenModified {
            get => this.hasBeenModified;
            set {
                this.RaisePropertyChanged(ref this.hasBeenModified, value);
                if (value && !this.OwningItem.HasBeenModified) {
                    this.OwningItem.HasBeenModified = true;
                }
            }
        }

        public BrushItemViewModel OwningItem { get; }

        public BrushViewModel(BrushItemViewModel owningItem) {
            this.OwningItem = owningItem;
        }
    }
}