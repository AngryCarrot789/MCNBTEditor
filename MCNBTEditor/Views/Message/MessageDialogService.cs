using System;
using System.Threading.Tasks;
using System.Windows;
using MCNBTEditor.Core.Views.Dialogs.Message;
using MCNBTEditor.Utils;
using MCNBTEditor.Views.FilePicking;

namespace MCNBTEditor.Views.Message {
    public class MessageDialogService : IMessageDialogService {
        public Task ShowMessageAsync(string caption, string message) {
            return DispatcherUtils.Invoke(() => {
                MessageWindow.DODGY_PRIMARY_SELECTION = "ok";
                return Dialogs.OkDialog.ShowAsync(caption, message);
            });
        }

        public Task ShowMessageExAsync(string title, string header, string message) {
            return DispatcherUtils.Invoke(() => {
                MessageWindow.DODGY_PRIMARY_SELECTION = "ok";
                return Dialogs.OkDialog.ShowAsync(title, header, message);
            });
        }

        public Task ShowMessageAsync(string message) {
            return this.ShowMessageAsync("Information", message);
        }

        public async Task<MsgDialogResult> ShowDialogAsync(string caption, string message, MsgDialogType type, MsgDialogResult defaultResult = MsgDialogResult.None) {
            MessageDialog dialog;
            switch (type) {
                case MsgDialogType.OK:          dialog = Dialogs.OkDialog; break;
                case MsgDialogType.OKCancel:    dialog = Dialogs.OkCancelDialog; break;
                case MsgDialogType.YesNo:       dialog = Dialogs.YesNoDialog; break;
                case MsgDialogType.YesNoCancel: dialog = Dialogs.YesNoCancelDialog; break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            string id;
            switch (defaultResult) {
                case MsgDialogResult.None:   id = null; break;
                case MsgDialogResult.OK:     id = "ok"; break;
                case MsgDialogResult.Yes:    id = "yes"; break;
                case MsgDialogResult.No:     id = "no"; break;
                case MsgDialogResult.Cancel: id = "cancel"; break;
                default: throw new ArgumentOutOfRangeException(nameof(defaultResult), defaultResult, null);
            }

            MessageWindow.DODGY_PRIMARY_SELECTION = id;
            string clickedId = await dialog.ShowAsync(caption, message);
            switch (clickedId) {
                case "cancel": return MsgDialogResult.Cancel;
                case "ok": return MsgDialogResult.OK;
                case "yes": return MsgDialogResult.Yes;
                case "no": return MsgDialogResult.No;
                default: return MsgDialogResult.None;
            }
        }

        public async Task<bool> ShowYesNoDialogAsync(string caption, string message, bool defaultResult = true) {
            MessageDialog dialog = Dialogs.YesNoDialog;
            string id = defaultResult ? "yes" : "no";
            MessageWindow.DODGY_PRIMARY_SELECTION = id;
            string clickedId = await dialog.ShowAsync(caption, message);
            return clickedId == "yes";
        }

        public bool? ShowDialogMainThread(MessageDialog dialog) {
            MessageWindow window = new MessageWindow {
                DataContext = dialog
            };

            if (MessageWindow.DODGY_PRIMARY_SELECTION == null) {
                MessageWindow.DODGY_PRIMARY_SELECTION = dialog.PreFocusedActionId;
            }

            dialog.Dialog = window;
            bool? result = window.ShowDialog();
            dialog.Dialog = null;
            return result;
        }

        public Task<bool?> ShowDialogAsync(MessageDialog dialog) {
            return DispatcherUtils.InvokeAsync(() => this.ShowDialogMainThread(dialog));
        }
    }
}