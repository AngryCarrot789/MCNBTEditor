using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MCNBTEditor.Core.Explorer;
using MCNBTEditor.Core.Views.Dialogs;

namespace MCNBTEditor.Views.NBT.Selector {
    public class ItemSelectorViewModel : BaseConfirmableDialogViewModel {
        private string title;
        public string Title {
            get => this.title;
            set => this.RaisePropertyChanged(ref this.title, value);
        }

        private string message;
        public string Message {
            get => this.message;
            set => this.RaisePropertyChanged(ref this.message, value);
        }

        private BaseTreeItemViewModel selectedItem;
        public BaseTreeItemViewModel SelectedItem {
            get => this.selectedItem;
            set {
                this.RaisePropertyChanged(ref this.selectedItem, value);
                this.ConfirmCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<BaseTreeItemViewModel> Items { get; }

        public ItemSelectorViewModel(IEnumerable<BaseTreeItemViewModel> items) {
            this.Items = new ObservableCollection<BaseTreeItemViewModel>(items ?? Enumerable.Empty<BaseTreeItemViewModel>());
        }

        protected override bool CanConfirm() {
            return base.CanConfirm() && this.SelectedItem != null;
        }
    }
}
