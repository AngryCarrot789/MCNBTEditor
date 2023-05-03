using System;
using MCNBTEditor.Core.Explorer;

namespace MCNBTEditor.Views.NBT.Finding {
    /// <summary>
    /// Interaction logic for FindNBTWindow.xaml
    /// </summary>
    public partial class FindNBTWindow : BaseWindow {
        public FindNBTWindow(BaseTreeItemViewModel rootItem) {
            this.InitializeComponent();
            this.DataContext = new FindViewModel(rootItem) {
                Window = this
            };

            this.Loaded += (sender, args) => {
                if (this.NameBox.IsFocused || (this.NameBox.Focusable && this.NameBox.Focus())) {
                    this.NameBox.SelectAll();
                }
            };
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            if (this.DataContext is FindViewModel findViewModel) {
                findViewModel.Dispose();
            }
        }
    }
}
