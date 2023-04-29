using System.IO;
using System.Windows;
using MCNBTEditor.Core;
using MCNBTEditor.Core.Actions;
using MCNBTEditor.Core.Shortcuts.Managing;
using MCNBTEditor.Core.Shortcuts.ViewModels;
using MCNBTEditor.Services;
using MCNBTEditor.Shortcuts;
using MCNBTEditor.Shortcuts.Converters;
using MCNBTEditor.Shortcuts.Dialogs;
using MCNBTEditor.Shortcuts.Views;
using MCNBTEditor.Utils;
using MCNBTEditor.Views.FilePicking;
using MCNBTEditor.Views.Main;
using MCNBTEditor.Views.Message;
using MCNBTEditor.Views.UserInputs;

namespace MCNBTEditor {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private async void Application_Startup(object sender, StartupEventArgs e) {
            ShortcutManager.Instance =  new WPFShortcutManager();
            ActionManager.Instance = new ActionManager();
            ActionManager.SearchAndRegisterActions(ActionManager.Instance);
            InputStrokeViewModel.KeyToReadableString = KeyStrokeStringConverter.ToStringFunction;
            InputStrokeViewModel.MouseToReadableString = MouseStrokeStringConverter.ToStringFunction;
            IoC.MessageDialogs = new MessageDialogService();
            IoC.Dispatcher = new DispatcherDelegate(this.Dispatcher);
            IoC.Clipboard = new ClipboardService();
            IoC.FilePicker = new FilePickDialogService();
            IoC.UserInput = new UserInputDialogService();
            IoC.ExplorerService = new WinExplorerService();
            IoC.KeyboardDialogs = new KeyboardDialogService();
            IoC.MouseDialogs = new MouseDialogService();
            IoC.ShortcutManagerDialog = new ShortcutManagerDialogService();
            IoC.OnShortcutModified = (x) => {
                if (!string.IsNullOrWhiteSpace(x)) {
                    ShortcutManager.Instance.InvalidateShortcutCache();
                    GlobalUpdateShortcutGestureConverter.BroadcastChange();
                    // UpdatePath(this.Resources, x);
                }
            };

            IoC.BroadcastShortcutActivity = (x) => {

            };

            string filePath = @"Keymap.xml";
            if (File.Exists(filePath)) {
                using (FileStream stream = File.OpenRead(filePath)) {
                    ShortcutGroup group = WPFKeyMapSerialiser.Instance.Deserialise(stream);
                    WPFShortcutManager.WPFInstance.SetRoot(group);
                }
            }
            else {
                MessageBox.Show("Keymap file does not exist: " + filePath);
            }

            // string fullPath = Path.GetFullPath(filePath);
            // WPFKeyMapSerialiser.Instance.Serialise(WPFShortcutManager.WPFInstance.Root).Save(FileUtils.ChangeActualFileName(fullPath, "KeyMap2"));

            MainWindow window = new MainWindow();
            IoC.TreeView = window.MainTreeView;
            this.MainWindow = window;
            window.Show();

            string debugPath = @"C:\Users\kettl\Desktop\TheRareCarrot.dat";
            if (window.DataContext is MainViewModel mvm && File.Exists(debugPath)) {
                // mvm.LoadFile(@"C:\Users\kettl\Desktop\TheRareCarrot.dat");
                await mvm.LoadFilesAction(new string[1] {
                    debugPath
                }, true);
            }
        }
    }
}
