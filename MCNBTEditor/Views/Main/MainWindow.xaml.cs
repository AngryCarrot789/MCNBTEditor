using System.Windows;

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
                    foreach (string file in files) {
                        await mvm.LoadFileAction(file);
                    }
                }
            }
        }
    }
}
