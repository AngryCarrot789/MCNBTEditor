using System;
using System.Threading.Tasks;
using System.Windows;
using MCNBTEditor.Core.Views.Dialogs.Message;
using MCNBTEditor.Utils;
using MCNBTEditor.Views.FilePicking;

namespace MCNBTEditor.Views.Message {
    public class MessageDialogService : IMessageDialogService {
        public async Task ShowMessageAsync(string caption, string message) {
            await DispatcherUtils.InvokeAsync(() => MessageDialogs.OkDialog.ShowAsync(caption, message));
        }

        public Task ShowMessageAsync(string message) {
            return this.ShowMessageAsync("Information", message);
        }

        public async Task<MsgDialogResult> ShowDialogAsync(string caption, string message, MsgDialogType type, MsgDialogResult defaultResult = MsgDialogResult.None) {
            MessageDialog dialog;
            switch (type) {
                case MsgDialogType.OK:          dialog = MessageDialogs.OkDialog; break;
                case MsgDialogType.OKCancel:    dialog = MessageDialogs.OkCancelDialog; break;
                case MsgDialogType.YesNo:       dialog = MessageDialogs.YesNoDialog; break;
                case MsgDialogType.YesNoCancel: dialog = MessageDialogs.YesNoCancelDialog; break;
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
            MessageDialog dialog = MessageDialogs.YesNoDialog;
            string id = defaultResult ? "yes" : "no";
            MessageWindow.DODGY_PRIMARY_SELECTION = id;
            string clickedId = await dialog.ShowAsync(caption, message);
            return clickedId == "yes";
        }

        public async Task<bool?> ShowDialogAsync(MessageDialog dialog) {
            MessageWindow window = new MessageWindow {
                DataContext = dialog
            };

            dialog.Dialog = window;
            bool? result = await window.ShowDialogAsync();
            dialog.Dialog = null;
            return result;
        }
    }
}