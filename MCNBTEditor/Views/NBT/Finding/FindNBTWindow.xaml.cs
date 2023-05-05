using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using MCNBTEditor.Core.Explorer;

namespace MCNBTEditor.Views.NBT.Finding {
    /// <summary>
    /// Interaction logic for FindNBTWindow.xaml
    /// </summary>
    public partial class FindNBTWindow : BaseWindow {
        public RegexValidationRule NameRegexValidator { get => (RegexValidationRule) this.Resources["NameRegexValidator"]; }
        public RegexValidationRule ValueRegexValidator { get => (RegexValidationRule) this.Resources["ValueRegexValidator"]; }

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

        private void OnNameRegexCheckChanged(object sender, System.Windows.RoutedEventArgs e) {
            this.NameRegexValidator.IsEnabled = ((ToggleButton) sender).IsChecked == true;
            this.NameBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }

        private void OnValueRegexCheckChanged(object sender, System.Windows.RoutedEventArgs e) {
            this.ValueRegexValidator.IsEnabled = ((ToggleButton) sender).IsChecked == true;
            this.ValueBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }
    }
}
