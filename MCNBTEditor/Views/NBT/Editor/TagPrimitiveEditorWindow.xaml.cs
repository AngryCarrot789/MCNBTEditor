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

namespace MCNBTEditor.Views.NBT.Editor {
    /// <summary>
    /// Interaction logic for TagPrimitiveEditorWindow.xaml
    /// </summary>
    public partial class TagPrimitiveEditorWindow : BaseDialog {
        public TagPrimitiveEditorWindow() {
            InitializeComponent();
            this.ContentRendered += this.TagPrimitiveEditorWindow_ContentRendered;
        }

        private void TagPrimitiveEditorWindow_ContentRendered(object sender, EventArgs e) {
            this.SizeToContent = SizeToContent.Manual;
            this.Height = Math.Ceiling(this.Height + 2);
        }
    }
}
