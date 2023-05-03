using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using MCNBTEditor.Views.UserInputs;

namespace MCNBTEditor.Views.NBT.Editor {
    /// <summary>
    /// Interaction logic for TagPrimitiveEditorWindow.xaml
    /// </summary>
    public partial class TagPrimitiveEditorWindow : BaseDialog {
        public SimpleInputValidationRule NameValidationRule => (SimpleInputValidationRule) this.Resources["NameValidationRule"];
        public SimpleInputValidationRule ValueValidationRule => (SimpleInputValidationRule) this.Resources["ValueValidationRule"];

        public TagPrimitiveEditorWindow() {
            InitializeComponent();
            this.ContentRendered += this.TagPrimitiveEditorWindow_ContentRendered;
            this.Loaded += this.TagPrimitiveEditorWindow_Loaded;
        }

        private void TagPrimitiveEditorWindow_Loaded(object sender, RoutedEventArgs e) {
            this.NameTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            this.ValueTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }

        private void TagPrimitiveEditorWindow_ContentRendered(object sender, EventArgs e) {
            // this.SizeToContent = SizeToContent.Manual;
            // this.Height = Math.Ceiling(this.Height + 2);
        }
    }
}
