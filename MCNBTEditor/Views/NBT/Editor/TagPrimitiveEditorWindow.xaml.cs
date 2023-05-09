using System;
using System.Windows;
using System.Windows.Controls;
using MCNBTEditor.Views.UserInputs;

namespace MCNBTEditor.Views.NBT.Editor {
    /// <summary>
    /// Interaction logic for TagPrimitiveEditorWindow.xaml
    /// </summary>
    public partial class TagPrimitiveEditorWindow : BaseDialog {
        public SimpleInputValidationRule NameValidationRule => (SimpleInputValidationRule) this.Resources["NameValidationRule"];
        public SimpleInputValidationRule ValueValidationRule => (SimpleInputValidationRule) this.Resources["ValueValidationRule"];

        public TagPrimitiveEditorWindow() {
            this.InitializeComponent();
            this.ContentRendered += this.TagPrimitiveEditorWindow_ContentRendered;
            this.Loaded += this.TagPrimitiveEditorWindow_Loaded;
        }

        private void TagPrimitiveEditorWindow_Loaded(object sender, RoutedEventArgs e) {
            this.NameTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            this.ValueTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            if (this.NameTextBox.Visibility == Visibility.Visible) {
                this.NameTextBox.Focus();
                this.NameTextBox.SelectAll();
            }
            else if (this.ValueTextBox.Visibility == Visibility.Visible) {
                this.ValueTextBox.Focus();
                this.ValueTextBox.SelectAll();
            }
        }

        private void TagPrimitiveEditorWindow_ContentRendered(object sender, EventArgs e) {
            // this.SizeToContent = SizeToContent.Manual;
            // this.Height = Math.Ceiling(this.Height + 2);
            this.InvalidateVisual();
            this.UpdateLayout();
        }
    }
}
