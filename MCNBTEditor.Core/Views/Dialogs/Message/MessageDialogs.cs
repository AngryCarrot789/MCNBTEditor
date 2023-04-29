namespace MCNBTEditor.Core.Views.Dialogs.Message {
    public class MessageDialogs {
        public static readonly MessageDialog YesNoCancelDialog;
        public static readonly MessageDialog OkDialog;
        public static readonly MessageDialog OkCancelDialog;
        public static readonly MessageDialog ItemAlreadyExistsDialog;
        public static readonly MessageDialog OpenFileFailureDialog;
        public static readonly MessageDialog UnknownFileFormatDialog;


        static MessageDialogs() {
            YesNoCancelDialog = new MessageDialog();
            YesNoCancelDialog.AddButton("Yes", "yes", true);
            YesNoCancelDialog.AddButton("No", "no", true);
            YesNoCancelDialog.AddButton("Cancel", "cancel", false);
            YesNoCancelDialog.MarkReadOnly();

            OkDialog = new MessageDialog();
            OkDialog.AddButton("OK", "ok", false);
            OkDialog.MarkReadOnly();

            OkCancelDialog = new MessageDialog();
            OkCancelDialog.AddButton("OK", "ok", true);
            OkCancelDialog.AddButton("Cancel", "cancel", false);
            OkCancelDialog.MarkReadOnly();

            ItemAlreadyExistsDialog = new MessageDialog {ShowAlwaysUseNextResultOption = true};
            ItemAlreadyExistsDialog.AddButton("Replace", "replace", true).ToolTip = "Replace the existing item with the new item";
            ItemAlreadyExistsDialog.AddButton("Add anyway", "keep", true).ToolTip = "Keeps the existing item and adds the new item, resulting in 2 items with the same file path";
            ItemAlreadyExistsDialog.AddButton("Ignore", "ignore", false).ToolTip = "Ignores the file, leaving the existing item as-is";
            ItemAlreadyExistsDialog.AddButton("Cancel", null, false).ToolTip = "Stop adding files and remove all files that have been added";

            UnknownFileFormatDialog = MessageDialogs.OkDialog.Clone();
            UnknownFileFormatDialog.ShowAlwaysUseNextResultOption = true;

            OpenFileFailureDialog = MessageDialogs.OkDialog.Clone();
            OpenFileFailureDialog.ShowAlwaysUseNextResultOption = true;
        }
    }
}