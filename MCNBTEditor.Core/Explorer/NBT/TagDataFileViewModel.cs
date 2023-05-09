using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Utils;
using MCNBTEditor.Core.Views.Dialogs;
using MCNBTEditor.Core.Views.Dialogs.Message;

namespace MCNBTEditor.Core.Explorer.NBT {
    public class TagDataFileViewModel : TagCompoundViewModel, IHaveFilePath {
        private bool hasFileSavedOnce;
        private string filePath;
        private bool isCompressed = true;
        private bool isBigEndian = true;
        private bool isModified;

        /// <summary>
        /// The file path of this tag
        /// </summary>
        public string FilePath {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value);
        }

        public bool IsCompressed {
            get => this.isCompressed;
            set => this.RaisePropertyChanged(ref this.isCompressed, value);
        }

        public bool IsBigEndian {
            get => this.isBigEndian;
            set => this.RaisePropertyChanged(ref this.isBigEndian, value);
        }

        /// <summary>
        /// Whether or not this tag has dirty data that the user may want to save
        /// </summary>
        public bool IsModified {
            get => this.isModified;
            set => this.RaisePropertyChanged(ref this.isModified, value);
        }

        public AsyncRelayCommand RefreshDataCommand { get; }
        public AsyncRelayCommand OpenInExplorerCommand { get; }
        public AsyncRelayCommand DeleteFileCommand { get; }
        public RelayCommand CopyFilePathToClipboardCommand { get; }
        public AsyncRelayCommand SaveFileCommand { get; }
        public AsyncRelayCommand SaveFileAsCommand { get; }


        public TagDataFileViewModel(BaseTagCollectionViewModel nbt, bool deepCopy = false) : this(nbt.Name) {
            this.AddItems(deepCopy ? nbt.ChildTags.Select(x => CreateFrom(x.Name, x.ToNBT())) : nbt.ChildTags);
        }

        public TagDataFileViewModel(string name, NBTTagCompound nbt) : this(name) {
            this.AddItem(nbt);
        }

        public TagDataFileViewModel(string name) : base(name) {
            this.RefreshDataCommand = new AsyncRelayCommand(this.RefreshAction, () => File.Exists(this.FilePath));
            this.DeleteFileCommand = new AsyncRelayCommand(this.DeleteFileAction, () => File.Exists(this.FilePath));
            this.OpenInExplorerCommand = new AsyncRelayCommand(this.OpenInExplorerAction, () => IoC.ExplorerService != null && File.Exists(this.FilePath));
            this.SaveFileCommand = new AsyncRelayCommand(() => this.SaveToFileAction(false));
            this.SaveFileAsCommand = new AsyncRelayCommand(() => this.SaveToFileAction(true));
            this.CopyFilePathToClipboardCommand = new RelayCommand(() => {
                if (!string.IsNullOrEmpty(this.FilePath)) {
                    if (IoC.Clipboard != null) {
                        IoC.Clipboard.ReadableText = this.FilePath;
                    }
                }
            }, () => IoC.Clipboard != null && !string.IsNullOrEmpty(this.FilePath));
        }

        public async Task RefreshAction() {
            if (File.Exists(this.FilePath)) {
                NBTTagCompound compound;
                try {
                    compound = CompressedStreamTools.Read(this.FilePath, out _, this.IsCompressed, this.IsBigEndian);
                }
                catch (Exception e) {
                    await IoC.MessageDialogs.ShowMessageAsync("Failed to refresh NBT", $"Failed to read NBT file at:\n\n{this.FilePath}\n\n{e.Message}");
                    return;
                }

                try {
                    this.Clear();
                    this.AddRange(compound.map.Select(x => CreateFrom(x.Key, x.Value)));
                }
                catch (Exception e) {
                    await IoC.MessageDialogs.ShowMessageAsync("Failed to parse NBT", $"Failed to parse NBT into UI elements for file at:\n\n{this.FilePath}\n\n{e.Message}");
                }
            }
        }

        public async Task DeleteFileAction() {
            string result = await Dialogs.RemoveItemWhenDeletingDialog.ShowAsync("Remove file from tree?", "Do you want to also remove the DAT file from the tree/list?");
            if (result == "cancel") {
                return;
            }

            if (File.Exists(this.FilePath)) {
                try {
                    File.Delete(this.FilePath);
                }
                catch (Exception e) {
                    result = "no";
                    await IoC.MessageDialogs.ShowMessageExAsync("Failed to delete file", $"Failed to delete {this.filePath}", e.ToString());
                }
            }

            if (result == "yes") {
                this.RemoveFromParent(); // removes from root
            }
        }

        public override async Task RemoveFromParentAction() {
            if (this.IsModified) {
                string yesNoDialog = await Dialogs.YesNoCancelDialog.ShowAsync("Modified tag", "This .DAT file has been modified since you opened it. " + (string.IsNullOrEmpty(this.FilePath) ? "Do you want to save the changes?" : $"Do you want to save the changes to {this.FilePath}"));
                if (yesNoDialog == "cancel") {
                    return;
                }
                else if (yesNoDialog == "yes") {
                    string path;
                    if (string.IsNullOrEmpty(this.FilePath) || (!this.hasFileSavedOnce && !File.Exists(this.FilePath))) {
                        DialogResult<string> result = IoC.FilePicker.ShowSaveFileDialog(Filters.NBTCommonTypesWithAllFiles, titleBar: "Select a save location for the NBT file");
                        if (!result.IsSuccess || string.IsNullOrEmpty(result.Value)) {
                            return;
                        }

                        path = result.Value;
                    }
                    else {
                        path = this.FilePath;
                    }

                    try {
                        this.SaveToFile(path);
                        this.hasFileSavedOnce = true;
                        if (!path.Equals(this.FilePath)) {
                            this.FilePath = path;
                        }

                        this.Name = Path.GetFileName(path);
                    }
                    catch (Exception e) {
                        await IoC.MessageDialogs.ShowMessageAsync("Failed to write NBT", $"Failed to write compressed NBT to file at:\n{this.FilePath}\n{e.Message}");
                    }
                }
            }

            await base.RemoveFromParentAction();
        }

        public Task OpenInExplorerAction() {
            if (IoC.ExplorerService != null) {
                IoC.ExplorerService.OpenFileInExplorer(this.FilePath);
            }

            return Task.CompletedTask;
        }

        public void SaveToFile() {
            this.SaveToFile(this.FilePath);
        }

        public void SaveToFile(string filePath) {
            CompressedStreamTools.Write(this.ToNBT(), filePath, this.IsCompressed, this.IsBigEndian);
        }

        public async Task SaveToFileAction(bool forceSaveAs) {
            string path;
            if (forceSaveAs || string.IsNullOrEmpty(this.FilePath) || (!this.hasFileSavedOnce && !File.Exists(this.FilePath))) {
                DialogResult<string> result = IoC.FilePicker.ShowSaveFileDialog(Filters.NBTCommonTypesWithAllFiles, titleBar: "Select a save location for the NBT file");
                if (!result.IsSuccess || string.IsNullOrEmpty(result.Value)) {
                    return;
                }

                path = result.Value;
            }
            else {
                path = this.FilePath;
            }

            string pathFileName = Path.GetFileName(path);
            BaseViewModel alreadyExists = this.ParentItem.Children.FirstOrDefault(x => x is BaseTagViewModel nbt && nbt.Name == pathFileName);
            if (alreadyExists != null && alreadyExists != this) {
                if (!await IoC.MessageDialogs.ShowYesNoDialogAsync("Same name already in tree", $"A DAT file with the name '{pathFileName}' already exists in the tree. Continue saving anyway?")) {
                    return;
                }
            }

            try {
                this.SaveToFile(path);
                this.hasFileSavedOnce = true;
                if (!path.Equals(this.FilePath)) {
                    this.FilePath = path;
                }

                this.Name = pathFileName;
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageAsync("Failed to write NBT", $"Failed to write compressed NBT to file at:\n{this.FilePath}\n{e.Message}");
            }
        }

        public void DoSaveFile(string path) {
            this.SaveToFile(path);
            this.hasFileSavedOnce = true;
            if (!path.Equals(this.FilePath)) {
                this.FilePath = path;
            }
        }

        public override BaseTagViewModel Clone() {
            TagDataFileViewModel tag = new TagDataFileViewModel(this.Name) {
                FilePath = this.FilePath, IsBigEndian = this.IsBigEndian, IsCompressed = this.IsCompressed
            };
            foreach (BaseTagViewModel item in this.ChildTags)
                tag.Add(item.Clone());
            return tag;
        }
    }
}