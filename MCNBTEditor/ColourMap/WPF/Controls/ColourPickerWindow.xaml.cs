using System.Windows;
using System.Windows.Media;
using MCNBTEditor.Core.Utils;

namespace MCNBTEditor.ColourMap.WPF.Controls {
    /// <summary>
    /// Interaction logic for ColourPickerWindow.xaml
    /// </summary>
    public partial class ColourPickerWindow : Window {
        public static readonly DependencyProperty ColourProperty = DependencyProperty.Register("Colour", typeof(Color), typeof(ColourPickerWindow), new PropertyMetadata(Colors.Transparent, OnColourChanged));

        public delegate void ColourChangedEventHandler(Color newColour);

        public event ColourChangedEventHandler ColourChanged;

        public Color Colour {
            get => (Color) this.GetValue(ColourProperty);
            set => this.SetValue(ColourProperty, value);
        }

        public ColourPickerWindow() {
            this.InitializeComponent();
            this.Colour = Colors.Gray;
        }

        private static void OnColourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is ColourPickerWindow win && e.NewValue is Color colour) {
                win.ColourChanged?.Invoke(colour);
                win.PreviewBorder.Background = new SolidColorBrush(colour);
            }
        }

        private void OnRedChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Color color = this.Colour;
            color.R = (byte) Maths.Clamp(e.NewValue, 0, 255);
            this.Colour = color;
        }

        private void OnGreenChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Color color = this.Colour;
            color.G = (byte) Maths.Clamp(e.NewValue, 0, 255);
            this.Colour = color;
        }

        private void OnBlueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Color color = this.Colour;
            color.B = (byte) Maths.Clamp(e.NewValue, 0, 255);
            this.Colour = color;
        }

        private void OnAlphaChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Color color = this.Colour;
            color.A = (byte) Maths.Clamp(e.NewValue, 0, 255);
            this.Colour = color;
        }
    }
}
