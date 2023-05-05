using System.Collections.ObjectModel;
using MCNBTEditor.Core.Utils;

namespace MCNBTEditor.ColourMap.Maps {
    public class ColourMapViewModel : BaseMapItemViewModel {
        internal readonly ObservableCollectionEx<BaseMapItemViewModel> items;
        public ReadOnlyObservableCollection<BaseMapItemViewModel> Items { get; }

        public override bool IsReadOnly {
            get => base.IsReadOnly;
            set {
                base.IsReadOnly = value;
                this.RefreshIsReadOnly();
            }
        }

        public ColourMapViewModel(ColourSchemaViewModel schema, ColourMapViewModel parent, string displayName) : base(schema, parent, displayName) {
            this.items = new ObservableCollectionEx<BaseMapItemViewModel>();
            this.Items = new ReadOnlyObservableCollection<BaseMapItemViewModel>(this.items);
        }

        public override void RefreshIsReadOnly() {
            base.RefreshIsReadOnly();
            foreach (BaseMapItemViewModel item in this.items) {
                item.RefreshIsReadOnly();
            }
        }

        public void AddItem(BaseMapItemViewModel item) {
            this.items.Add(item);
        }
    }
}