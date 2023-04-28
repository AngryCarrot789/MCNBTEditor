using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MCNBTEditor.Core;
using MCNBTEditor.Core.Explorer;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.Explorer.Regions;
using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Utils;
using MCNBTEditor.Views.FilePicking;
using Microsoft.Win32;

namespace MCNBTEditor.Views.Main {
    public class MainViewModel : BaseViewModel {
        public RootTreeItemViewModel Root { get; }

        private BaseTreeItemViewModel currentFolderItem;
        private BaseTreeItemViewModel seletedTreeItem;

        /// <summary>
        /// The current item being used for the folder preview
        /// </summary>
        public BaseTreeItemViewModel CurrentFolderItem {
            get => this.currentFolderItem;
            set => this.RaisePropertyChanged(ref this.currentFolderItem, value);
        }

        /// <summary>
        /// The currently selected item in the tree (not the list)
        /// </summary>
        public BaseTreeItemViewModel SelectedTreeItem {
            get => this.seletedTreeItem;
            set => this.RaisePropertyChanged(ref this.seletedTreeItem, value);
        }

        public IEnumerable<BaseTreeItemViewModel> SelectedFolderItems => this.ListView.SelectedItems;

        public bool SortTagCompoundByDefault {
            get => IoC.SortTagCompoundByDefault;
            set {
                IoC.SortTagCompoundByDefault = value;
                this.RaisePropertyChanged();
            }
        }

        private bool isCompressedDefault = true;
        public bool IsCompressedDefault {
            get => this.isCompressedDefault;
            set => this.RaisePropertyChanged(ref this.isCompressedDefault, value);
        }

        private bool isBigEndianDefault = true;
        public bool IsBigEndianDefault {
            get => this.isBigEndianDefault;
            set => this.RaisePropertyChanged(ref this.isBigEndianDefault, value);
        }

        public ICommand CreateDatFileCommand { get; }
        public ICommand OpenFileCommand { get; }

        public IExtendedTree TreeView { get; }
        public IExtendedList ListView { get; }

        public MainViewModel(IExtendedTree tree, IExtendedList list) {
            this.TreeView = tree;
            this.ListView = list;
            this.Root = new RootTreeItemViewModel();
            this.CreateDatFileCommand = new RelayCommand(() => this.Root.AddItem(new TagDataFileViewModel("New Dat File.dat")));
            this.OpenFileCommand = new AsyncRelayCommand(this.OpenFileAction);
        }

        public async Task OpenFileAction() {
            // TODO: put this into a service maybe?
            OpenFileDialog ofd = new OpenFileDialog() {
                Filter = Filters.NBTCommonTypesWithAllFiles,
                Multiselect = true,
                Title = "Select one or more files to open"
            };

            if (ofd.ShowDialog(FolderPicker.GetCurrentActiveWindow()) == true) {
                HashSet<string> existing = new HashSet<string>();
                foreach (BaseTreeItemViewModel item in this.Root.Children) {
                    if (item is IHaveFilePath file && !string.IsNullOrEmpty(file.FilePath)) {
                        existing.Add(file.FilePath);
                    }
                }

                foreach (string file in ofd.FileNames) {
                    if (!existing.Contains(file)) {
                        await this.LoadFileAction(file, false);
                    }
                }
            }
        }

        public async Task LoadFileAction(string path, bool checkAlreadyAdded = true) {
            if (checkAlreadyAdded) {
                BaseTreeItemViewModel found = this.Root.FindChild(x => x is IHaveFilePath j && j.FilePath == path);
                if (found != null) {
                    bool result = await IoC.MessageDialogs.ShowYesNoDialogAsync("Item already added", path + " was already added. Do you want to replace it with the new file?");
                    if (result) {
                        this.Root.RemoveItem(found);
                    }
                    else {
                        return;
                    }
                }
            }

            string extension = Path.GetExtension(path);
            switch (extension) {
                case ".mca": {
                    RegionFileViewModel vm = new RegionFileViewModel() {
                        FilePath = path,
                        IsBigEndian = this.IsBigEndianDefault
                    };

                    this.Root.AddItem(vm); // add before loading to see the entry counter + the IsLoading property working ;)
                    try {
                        await vm.RefreshAction();
                    }
                    catch (Exception e) {
                        this.Root.RemoveItem(vm);
                        try {
                            vm.Dispose();
                        }
                        catch { /* ignored */ }
                        await IoC.MessageDialogs.ShowMessageAsync("Failed to open file", $"Failed to open region file at {path}: \n\n{e.Message}");
                    }

                    break;
                }
                case ".dat": {
                    TagDataFileViewModel file = new TagDataFileViewModel(Path.GetFileName(path)) {
                        IsCompressed = this.IsCompressedDefault,
                        IsBigEndian = this.IsBigEndianDefault,
                        FilePath = path
                    };

                    this.Root.AddItem(file);
                    try {
                        await file.RefreshAction();
                    }
                    catch (Exception e) {
                        this.Root.RemoveItem(file);
                        await IoC.MessageDialogs.ShowMessageAsync("Failed to open file", $"Failed to open region file at {path}: \n\n{e.Message}");
                    }

                    break;
                }
                default: {
                    await IoC.MessageDialogs.ShowMessageAsync("Unknown file format", $"Unknown file extension: {extension}");
                    break;
                }
            }
        }

        public async Task NavigateToPath(string path) {
            List<BaseTreeItemViewModel> list = await this.Root.ResolvePathAction(path);
            if (list != null) {
                await this.TreeView.RepeatExpandHierarchyFromRootAsync(list);
            }
        }
    }
}
