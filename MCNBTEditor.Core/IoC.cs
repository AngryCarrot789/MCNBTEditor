using System;
using MCNBTEditor.Core.Actions;
using MCNBTEditor.Core.Explorer;
using MCNBTEditor.Core.Explorer.Dialog;
using MCNBTEditor.Core.Services;
using MCNBTEditor.Core.Shortcuts.Dialogs;
using MCNBTEditor.Core.Shortcuts.Managing;
using MCNBTEditor.Core.Views.Dialogs.FilePicking;
using MCNBTEditor.Core.Views.Dialogs.Message;
using MCNBTEditor.Core.Views.Dialogs.UserInputs;

namespace MCNBTEditor.Core {
    public static class IoC {
        public static SimpleIoC Instance { get; } = new SimpleIoC();

        public static ActionManager ActionManager { get; } = new ActionManager();
        public static Action<string> OnShortcutModified { get; set; }
        public static Action<string> BroadcastShortcutActivity { get; set; }

        public static IDispatcher Dispatcher { get; set; }
        public static IClipboardService Clipboard { get; set; }
        public static IMessageDialogService MessageDialogs { get; set; }
        public static IFilePickDialogService FilePicker { get; set; }
        public static IUserInputDialogService UserInput { get; set; }
        public static IItemSelectorService ItemSelectorService { get; set; }
        public static ITagEditorService TagEditorService { get; set; }
        public static IExplorerService ExplorerService { get; set; }
        public static IKeyboardDialogService KeyboardDialogs { get; set; }
        public static IMouseDialogService MouseDialogs { get; set; }
        public static ShortcutManager ShortcutManager { get; set; }
        public static IShortcutManagerDialogService ShortcutManagerDialog { get; set; }

        public static bool SortTagCompoundByDefault { get; set; } = true;
        public static IExtendedTree TreeView { get; set; }
    }
}