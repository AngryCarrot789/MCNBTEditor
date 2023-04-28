using System.Windows.Input;
using MCNBTEditor.Core.Shortcuts.Inputs;
using MCNBTEditor.Core.Views.Dialogs;
using MCNBTEditor.Shortcuts.Converters;
using MCNBTEditor.Views;

namespace MCNBTEditor.Shortcuts.Dialogs {
    /// <summary>
    /// Interaction logic for MouseStrokeInputWindow.xaml
    /// </summary>
    public partial class MouseStrokeInputWindow : BaseDialog {
        public MouseStroke Stroke { get; set; }

        public MouseStrokeInputWindow() {
            this.InitializeComponent();
            this.DataContext = new BaseConfirmableDialogViewModel(this);
            this.InputBox.Text = "";
        }

        private void InputBox_MouseDown(object sender, MouseButtonEventArgs e) {
            MouseStroke stroke = ShortcutUtils.GetMouseStrokeForEvent(e);
            this.Stroke = stroke;
            this.InputBox.Text = MouseStrokeStringConverter.ToStringFunction(stroke.MouseButton, stroke.Modifiers, stroke.ClickCount);
        }

        private void InputBox_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (ShortcutUtils.GetMouseStrokeForEvent(e, out MouseStroke stroke)) {
                this.Stroke = stroke;
                this.InputBox.Text = MouseStrokeStringConverter.ToStringFunction(stroke.MouseButton, stroke.Modifiers, stroke.ClickCount);
            }
        }
    }
}
