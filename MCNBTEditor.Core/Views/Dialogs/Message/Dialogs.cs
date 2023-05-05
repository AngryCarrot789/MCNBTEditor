namespace MCNBTEditor.Core.Views.Dialogs.Message {
    /// <summary>
    /// A static class that contains some of the general message dialogs
    /// </summary>
    public static class Dialogs {
        public static readonly MessageDialog OkDialog;
        public static readonly MessageDialog OkCancelDialog;
        public static readonly MessageDialog YesNoDialog;
        public static readonly MessageDialog YesNoCancelDialog;

        public static readonly MessageDialog ItemAlreadyExistsDialog;
        public static readonly MessageDialog OpenFileFailureDialog;
        public static readonly MessageDialog UnknownFileFormatDialog;
        public static readonly MessageDialog ClipboardUnavailableDialog;
        public static readonly MessageDialog InvalidClipboardDataDialog;
        public static readonly MessageDialog InvalidPathDialog;
        public static readonly MessageDialog RemoveDatFileWhenDeletingDialog;

        static Dialogs() {
            YesNoCancelDialog = new MessageDialog("yes");
            YesNoCancelDialog.AddButton("Yes", "yes", true);
            YesNoCancelDialog.AddButton("No", "no", true);
            YesNoCancelDialog.AddButton("Cancel", "cancel", false);
            YesNoCancelDialog.MarkReadOnly();

            YesNoDialog = new MessageDialog("yes");
            YesNoDialog.AddButton("Yes", "yes", true);
            YesNoDialog.AddButton("No", "no", true);
            YesNoDialog.MarkReadOnly();

            OkDialog = new MessageDialog("ok");
            OkDialog.AddButton("OK", "ok", true);
            OkDialog.MarkReadOnly();

            OkCancelDialog = new MessageDialog("ok");
            OkCancelDialog.AddButton("OK", "ok", true);
            OkCancelDialog.AddButton("Cancel", "cancel", false);
            OkCancelDialog.MarkReadOnly();

            ClipboardUnavailableDialog = OkDialog.Clone();
            ClipboardUnavailableDialog.ShowAlwaysUseNextResultOption = true;
            ClipboardUnavailableDialog.MarkReadOnly();

            InvalidClipboardDataDialog = OkDialog.Clone();
            InvalidClipboardDataDialog.ShowAlwaysUseNextResultOption = true;
            InvalidClipboardDataDialog.MarkReadOnly();

            ItemAlreadyExistsDialog = new MessageDialog("replace") {ShowAlwaysUseNextResultOption = true};
            ItemAlreadyExistsDialog.AddButton("Replace", "replace", true).ToolTip = "Replace the existing item with the new item";
            ItemAlreadyExistsDialog.AddButton("Add anyway", "keep", true).ToolTip = "Keeps the existing item and adds the new item, resulting in 2 items with the same file path";
            ItemAlreadyExistsDialog.AddButton("Ignore", "ignore", true).ToolTip = "Ignores the file, leaving the existing item as-is";
            ItemAlreadyExistsDialog.AddButton("Cancel", "cancel", false).ToolTip = "Stop adding files and remove all files that have been added";

            UnknownFileFormatDialog = new MessageDialog("ok") {ShowAlwaysUseNextResultOption = true};
            UnknownFileFormatDialog.AddButton("OK", "ok", true);
            UnknownFileFormatDialog.AddButton("Cancel", "cancel", false);

            OpenFileFailureDialog = OkDialog.Clone();
            OpenFileFailureDialog.ShowAlwaysUseNextResultOption = true;
            OpenFileFailureDialog.MarkReadOnly();

            InvalidPathDialog = OkDialog.Clone();
            InvalidPathDialog.ShowAlwaysUseNextResultOption = true;
            InvalidPathDialog.MarkReadOnly();

            RemoveDatFileWhenDeletingDialog = new MessageDialog("yes");
            RemoveDatFileWhenDeletingDialog.ShowAlwaysUseNextResultOption = true;
            RemoveDatFileWhenDeletingDialog.AddButton("Yes", "yes", false);
            RemoveDatFileWhenDeletingDialog.AddButton("No", "no", false);
            RemoveDatFileWhenDeletingDialog.MarkReadOnly();
        }
    }
}