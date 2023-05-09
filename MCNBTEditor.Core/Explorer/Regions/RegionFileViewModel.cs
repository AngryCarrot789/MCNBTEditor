using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Regions;
using MCNBTEditor.Core.Shortcuts;
using MCNBTEditor.Core.Views.Dialogs.Message;

namespace MCNBTEditor.Core.Explorer.Regions {
    public class RegionFileViewModel : BaseTreeItemViewModel, IHaveChildren, IHaveFilePath, IHaveTreePath, IDisposable, IShortcutToCommand, IRefreshable {
        private string filePath;
        private string name;
        private bool isBigEndian = true;
        private volatile int isLoading;

        public string FilePath {
            get => this.filePath;
            private set {
                this.RaisePropertyChanged(ref this.filePath, value);
                this.Name = string.IsNullOrWhiteSpace(value) ? "<unnamed region>" : Path.GetFileName(value);
            }
        }

        public string Name {
            get => this.name;
            set => this.RaisePropertyChanged(ref this.name, value);
        }

        public bool IsBigEndian {
            get => this.isBigEndian;
            set => this.RaisePropertyChanged(ref this.isBigEndian, value);
        }


        public bool IsLoading {
            get => this.isLoading == 1;
            private set {
                this.isLoading = value ? 1 : 0;
                this.RaisePropertyChanged();
            }
        }

        public RegionFile Region { get; set; }

        public override bool CanHoldChildren => true;

        public AsyncRelayCommand RefreshCommand { get; }
        public AsyncRelayCommand SaveFileCommand { get; }
        public AsyncRelayCommand SaveFileAsCommand { get; }
        public RelayCommand CopyFilePathToClipboardCommand { get; }
        public AsyncRelayCommand OpenInExplorerCommand { get; }
        public AsyncRelayCommand DeleteFileCommand { get; }
        public AsyncRelayCommand RemoveFromParentCommand { get; }

        public string TreePathPartName => this.Name;

        public RegionFileViewModel(string filePath = null) {
            this.RemoveFromParentCommand = new AsyncRelayCommand(this.RemoveFromParentAction, () => this.ParentItem is IHaveChildren);
            this.RefreshCommand = new AsyncRelayCommand(this.RefreshAction, () => File.Exists(this.FilePath));
            this.SaveFileCommand = new AsyncRelayCommand(async () => await IoC.MessageDialogs.ShowMessageAsync("TODO", "Coming soon..."), () => false);
            this.SaveFileAsCommand = new AsyncRelayCommand(async () => await IoC.MessageDialogs.ShowMessageAsync("TODO", "Coming soon..."), () => false);
            this.OpenInExplorerCommand = new AsyncRelayCommand(this.OpenInExplorerAction, () => IoC.ExplorerService != null && File.Exists(this.FilePath));
            this.DeleteFileCommand = new AsyncRelayCommand(this.DeleteFileAction, () => File.Exists(this.FilePath));
            this.CopyFilePathToClipboardCommand = new RelayCommand(() => {
                if (!string.IsNullOrEmpty(this.FilePath)) {
                    if (IoC.Clipboard != null) {
                        IoC.Clipboard.ReadableText = this.FilePath;
                    }
                }
            }, () => IoC.Clipboard != null && !string.IsNullOrEmpty(this.FilePath));
            this.FilePath = filePath;
        }

        public async Task DeleteFileAction() {
            string result = await Dialogs.RemoveItemWhenDeletingDialog.ShowAsync("Remove file from tree?", "Do you want to also remove the DAT file from the tree/list?");
            if (result == "cancel") {
                return;
            }

            await this.TryDispose(); // required, because the file cannot be deleted when the file stream is open
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
                this.RemoveFromParentItem(); // removes from root
            }
        }

        public Task OpenInExplorerAction() {
            IoC.ExplorerService?.OpenFileInExplorer(this.FilePath);
            return Task.CompletedTask;
        }

        public async Task RefreshAction() {
            if (string.IsNullOrWhiteSpace(this.FilePath) || !File.Exists(this.FilePath)) {
                return;
            }

            if (Interlocked.CompareExchange(ref this.isLoading, 1, 0) == 1) {
                return;
            }

            this.RaisePropertyChanged(nameof(this.IsLoading));
            this.IsReadOnly = true;
            this.Clear();
            await Task.Run(async () => {
                try {
                    this.Region = new RegionFile(this.filePath, this.IsBigEndian);
                    for (int x = 0; x < 32; x++) {
                        for (int z = 0; z < 32; z++) {
                            NBTTagCompound chunk = ChunkLoader.ReadChunkTag(this.Region, x, z);
                            if (chunk != null) {
                                TagCompoundViewModel tag = BaseTagViewModel.CreateFrom($"Chunk ({x}, {z})", chunk);

                                // not awaiting because it's like 4x slower to load a region file
                                IoC.Dispatcher.InvokeLaterAsync(() => this.Add(tag));
                            }
                        }
                    }
                }
                catch (Exception e) {
                    await IoC.MessageDialogs.ShowMessageExAsync("Error reading region", "Error reading region file or chunks", e.ToString());
                }
            });

            this.IsLoading = false;
            this.IsReadOnly = false;
        }

        public virtual async Task<bool> RemoveFromParentAction() {
            // minecraft mod style of "doRemove parameter" functions ;)
            await this.TryDispose();
            return this.ParentItem is IHaveChildren children && children.RemoveItem(this);
        }

        public bool RemoveFromParentItem() {
            try {
                this.Dispose();
            }
            catch (Exception e) {
                Debug.WriteLine("Failed to dispose region file:\n" + e);
            }

            return this.ParentItem is IHaveChildren parent && parent.RemoveItem(this);
        }

        public void Dispose() {
            this.Region?.Dispose();
        }

        public async Task<bool> TryDispose() {
            try {
                this.Dispose();
                return true;
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Failed to dispose region", "Failed to dispose the resources used by the region", e.ToString());
                return false;
            }
        }

        public void AddItem(BaseTreeItemViewModel item) {
            base.Add(item);
        }

        public bool RemoveItem(BaseTreeItemViewModel item) {
            return base.Remove(item);
        }

        public int IndexOfItem(BaseTreeItemViewModel item) {
            return base.IndexOf(item);
        }

        public void ClearItems() {
            base.Clear();
        }

        // TODO: remove this because it's such a bad design
        public virtual ICommand GetCommandForShortcut(string shortcutId) {
            switch (shortcutId) {
                case "Application/EditorView/NBTTag/CopyFilePath": return this.CopyFilePathToClipboardCommand;
                case "Application/EditorView/NBTTag/RemoveFromParent": return this.RemoveFromParentCommand;
            }

            return null;
        }

        public Task RefreshAsync() {
            return this.RefreshAction();
        }
    }
}