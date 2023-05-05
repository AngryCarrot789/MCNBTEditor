using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using MCNBTEditor.Core.AdvancedContextService;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Regions;
using MCNBTEditor.Core.Shortcuts;

namespace MCNBTEditor.Core.Explorer.Regions {
    public class RegionFileViewModel : BaseTreeItemViewModel, IHaveChildren, IHaveFilePath, IHaveTreePath, IContextProvider, IDisposable, IShortcutToCommand {
        private string filePath;
        public string FilePath {
            get => this.filePath;
            set {
                this.RaisePropertyChanged(ref this.filePath, value);
                this.Name = value == null ? "<unnamed region>" : Path.GetFileName(value);
            }
        }

        private string name;
        public string Name {
            get => this.name;
            set => this.RaisePropertyChanged(ref this.name, value);
        }

        private bool isBigEndian = true;
        public bool IsBigEndian {
            get => this.isBigEndian;
            set => this.RaisePropertyChanged(ref this.isBigEndian, value);
        }

        private volatile bool isLoading;
        public bool IsLoading {
            get => this.isLoading;
            set {
                this.isLoading = value;
                this.RaisePropertyChanged();
            }
        }

        public RegionFile Region { get; set; }

        public override bool CanHoldChildren => true;

        public AsyncRelayCommand RefreshCommand { get; }
        public AsyncRelayCommand SaveFileCommand { get; }
        public AsyncRelayCommand SaveFileAsCommand { get; }
        public RelayCommand CopyFilePathToClipboardCommand { get; }
        public AsyncRelayCommand ShowInExplorerCommand { get; }
        public AsyncRelayCommand DeleteFileCommand { get; }
        public ICommand RemoveFromParentCommand { get; }

        public string TreePathPartName => this.Name;

        public RegionFileViewModel() {
            this.RemoveFromParentCommand = new RelayCommand(() => this.RemoveFromParentItem(), () => this.RemoveFromParentItem(true));
            this.RefreshCommand = new AsyncRelayCommand(this.RefreshAction, () => File.Exists(this.FilePath));
            this.SaveFileCommand = new AsyncRelayCommand(async () => await IoC.MessageDialogs.ShowMessageAsync("TODO", "Coming soon..."), () => false);
            this.SaveFileAsCommand = new AsyncRelayCommand(async () => await IoC.MessageDialogs.ShowMessageAsync("TODO", "Coming soon..."), () => false);
            this.ShowInExplorerCommand = new AsyncRelayCommand(this.OpenInExplorerAction, () => IoC.ExplorerService != null && File.Exists(this.FilePath));
            this.DeleteFileCommand = new AsyncRelayCommand(this.DeleteFileAction, () => File.Exists(this.FilePath));
            this.CopyFilePathToClipboardCommand = new RelayCommand(() => {
                if (!string.IsNullOrEmpty(this.FilePath)) {
                    if (IoC.Clipboard != null) {
                        IoC.Clipboard.ReadableText = this.FilePath;
                    }
                }
            }, () => IoC.Clipboard != null && !string.IsNullOrEmpty(this.FilePath));
        }

        public async Task DeleteFileAction() {
            bool canRemove = false;
            if (File.Exists(this.FilePath)) {
                try {
                    File.Delete(this.FilePath);
                    canRemove = true;
                }
                catch (Exception e) {
                    await IoC.MessageDialogs.ShowMessageAsync("Failed to delete file", $"Failed to delete {this.filePath}:\n{e.Message}");
                }
            }

            if (canRemove && await IoC.MessageDialogs.ShowYesNoDialogAsync("Remove dat file?", "Do you want to also remove the DAT file from the list?")) {
                this.RemoveFromParentItem(false); // removes from root
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

            await Task.Run(() => {
                this.IsLoading = true;
                try {
                    this.Region = new RegionFile(this.filePath, this.IsBigEndian);
                    for (int x = 0; x < 32; x++) {
                        for (int z = 0; z < 32; z++) {
                            NBTTagCompound chunk = ChunkLoader.ReadChunkTag(this.Region, x, z);
                            if (chunk != null) {
                                TagCompoundViewModel tag = BaseTagViewModel.CreateFrom($"Chunk ({x}, {z})", chunk);
                                IoC.Dispatcher.InvokeLaterAsync(() => this.Add(tag));
                            }
                        }
                    }
                }
                finally {
                    this.IsLoading = false;
                }
            });
        }

        public bool RemoveFromParentItem(bool isFakeRemove = false) { // minecraft mod style of "doRemove parameter" functions ;)
            return this.ParentItem is IHaveChildren parent && (isFakeRemove || parent.RemoveItem(this));
        }

        public void Dispose() {
            this.Region?.Dispose();
        }

        public virtual void GetContext(List<IContextEntry> list) {
            list.Add(new CommandContextEntry("Refresh", this.RefreshCommand));
            list.Add(new CommandContextEntry("Save (coming soon)", this.SaveFileCommand));
            list.Add(new CommandContextEntry("Save as (coming soon)", this.SaveFileAsCommand));
            list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/CopyFilePath", this.CopyFilePathToClipboardCommand));
            list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/OpenInExplorer", this.ShowInExplorerCommand));

            list.Add(SeparatorEntry.Instance);
            list.Add(new ShortcutCommandContextEntry("Application/EditorView/NBTTag/RemoveFromParent", this.RemoveFromParentCommand));
            list.Add(new CommandContextEntry("Delete FILE", this.DeleteFileCommand));
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

        public virtual ICommand GetCommandForShortcut(string shortcutId) {
            switch (shortcutId) {
                case "Application/EditorView/NBTTag/CopyFilePath": return this.CopyFilePathToClipboardCommand;
                case "Application/EditorView/NBTTag/OpenInExplorer": return this.ShowInExplorerCommand;
                case "Application/EditorView/NBTTag/RemoveFromParent": return this.RemoveFromParentCommand;
            }

            return null;
        }
    }
}