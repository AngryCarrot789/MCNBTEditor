using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MCNBTEditor.Themes;

namespace MCNBTEditor.Views.Main {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseWindow {
        public MainWindow() {
            this.InitializeComponent();
            this.DataContext = new MainViewModel(this.MainTreeView, this.MainListBox);
        }

        private async void MainTreeView_Drop(object sender, DragEventArgs e) {
            if (this.DataContext is MainViewModel mvm) {
                if (e.Data.GetData(DataFormats.FileDrop) is string[] files) {
                    await mvm.LoadFilesAction(files, true);
                }
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            ResourceDictionary dictionary = ((App) Application.Current).ThemeDictionary;
            SolidColorBrush brush = dictionary["_REghZy.TestBrush"] as SolidColorBrush;
            if (brush == null || brush.IsFrozen) {
                brush = new SolidColorBrush();
            }

            Color c = Colors.Red;
            c.B = (byte) e.NewValue;
            brush.Color = c;
            dictionary["_REghZy.TestBrush"] = brush;
        }

        // lazy
        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            switch (((MenuItem) sender).Tag) {
                case "0": ThemesController.SetTheme(ThemeType.SoftDark); return;
                case "1": ThemesController.SetTheme(ThemeType.DeepDark); return;
                case "2": ThemesController.SetTheme(ThemeType.SoftDarkAndBlue); return;
                case "3": ThemesController.SetTheme(ThemeType.RedBlackTheme); return;
            }
        }
    }
}
