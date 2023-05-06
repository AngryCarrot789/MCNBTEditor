using MCNBTEditor.ColourMap;
using MCNBTEditor.Core;
using MCNBTEditor.Core.Shortcuts.Managing;
using MCNBTEditor.Core.Shortcuts.ViewModels;

namespace MCNBTEditor.Settings {
    public class SettingsViewModel : BaseViewModel {
        private ColourSchemaViewModel currentColourSchema;
        public ColourSchemaViewModel CurrentColourSchema {
            get => this.currentColourSchema;
            set => this.RaisePropertyChanged(ref this.currentColourSchema, value);
        }

        private ShortcutManagerViewModel shortcutsManager;
        public ShortcutManagerViewModel ShortcutsManager {
            get => this.shortcutsManager;
            set => this.RaisePropertyChanged(ref this.shortcutsManager, value);
        }

        public SettingsViewModel() {
            this.ShortcutsManager = new ShortcutManagerViewModel(ShortcutManager.Instance);
            this.CurrentColourSchema = ColourSchemaViewModel.CreateDarkMode();
        }
    }
}
