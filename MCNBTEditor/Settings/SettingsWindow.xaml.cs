using System.Windows;

namespace MCNBTEditor.Settings {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window {
        public SettingsWindow() {
            InitializeComponent();
            this.DataContext = new SettingsViewModel();
        }
    }
}
