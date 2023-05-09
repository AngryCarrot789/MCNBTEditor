using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using MCNBTEditor.Core;
using MCNBTEditor.Core.Explorer;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.Explorer.Regions;
using MCNBTEditor.Core.Utils;
using MCNBTEditor.Core.Views.Dialogs.Message;
using MCNBTEditor.Settings;
using MCNBTEditor.Views.FilePicking;
using Microsoft.Win32;

namespace MCNBTEditor.Views.Main {
    public class MainViewModel : BaseViewModel {
        public RootTreeItemViewModel Root { get; }

        private BaseTreeItemViewModel currentFolderItem;
        private BaseTreeItemViewModel seletedTreeItem;
        private BaseTreeItemViewModel primaryListSelectedItem;

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
            set {
                this.RaisePropertyChanged(ref this.seletedTreeItem, value);
                this.RaisePropertyChanged(nameof(this.SelectedFilePathable));
            }
        }

        public IHaveFilePath SelectedFilePathable => this.SelectedTreeItem?.RootParent as IHaveFilePath;

        public IEnumerable<BaseTreeItemViewModel> SelectedFolderItems => this.ListView.SelectedItems;

        public BaseTreeItemViewModel PrimaryListSelectedItem {
            get => this.primaryListSelectedItem;
            set => this.RaisePropertyChanged(ref this.primaryListSelectedItem, value);
        }

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

        public AsyncRelayCommand<BaseTreeItemViewModel> UseItemCommand { get; }

        public ICommand ShowSettingsCommand { get; }

        public MainViewModel(IExtendedTree tree, IExtendedList list) {
            this.TreeView = tree;
            this.ListView = list;
            tree.SelectionChanged += this.TreeOnSelectionChanged;
            this.Root = new RootTreeItemViewModel();
            this.CreateDatFileCommand = new RelayCommand(() => this.Root.AddItem(new TagDataFileViewModel($"New {this.Root.ChildrenCount + 1}.dat")));
            this.OpenFileCommand = new AsyncRelayCommand(this.OpenFileAction);
            this.UseItemCommand = new AsyncRelayCommand<BaseTreeItemViewModel>(this.UseItemAction);
            this.ShowSettingsCommand = new RelayCommand(() => {
                new SettingsWindow().Show();
            });
        }

        public async Task UseItemAction(BaseTreeItemViewModel item) {
            if (item.CanHoldChildren) {
                if (this.TreeView.IsNavigating) {
                    await IoC.MessageDialogs.ShowMessageAsync("Already navigating", "A navigation is already being processed. Wait for it to finish first");
                    return;
                }

                if (!await this.TreeView.NavigateToItemAsync(item)) {
                    await IoC.MessageDialogs.ShowMessageAsync("Navigation failed", "Failed to navigate to the item!");
                }
            }
            else if (item is TagPrimitiveViewModel primitive) {
                await primitive.EditPrimitiveTagAction();
            }
        }

        private void TreeOnSelectionChanged(BaseTreeItemViewModel oldItem, BaseTreeItemViewModel newItem) {
            if (newItem == null) {
                newItem = this.Root;
            }

            this.SelectedTreeItem = newItem;
            if (newItem.CanHoldChildren) {
                this.CurrentFolderItem = newItem;
                this.PrimaryListSelectedItem = newItem.GetFirstChild<BaseTreeItemViewModel>();
            }
            else {
                this.CurrentFolderItem = newItem.ParentItem ?? this.Root;
                this.PrimaryListSelectedItem = newItem;
            }
        }

        private void ListOnSelectionChanged(IEnumerable<BaseTreeItemViewModel> oldItems, IEnumerable<BaseTreeItemViewModel> newItems) {

        }

        public async Task OpenFileAction() {
            // TODO: put this into a service maybe?
            OpenFileDialog ofd = new OpenFileDialog() {
                Filter = Filters.NBTCommonTypesWithAllFiles,
                Multiselect = true,
                Title = "Select one or more files to open"
            };

            if (ofd.ShowDialog(FolderPicker.GetCurrentActiveWindow()) == true) {
                await this.LoadFilesAction(ofd.FileNames, true);
            }
        }

        public async Task LoadFilesAction(string[] paths, bool checkAlreadyAdded = true) {
            int endIndex = paths.Length - 1;
            if (endIndex < 0) {
                return;
            }

            bool isLoadingMultiple = endIndex > 0;
            List<BaseTreeItemViewModel> added = new List<BaseTreeItemViewModel>();
            using (Dialogs.ItemAlreadyExistsDialog.Use()) {
                using (Dialogs.UnknownFileFormatDialog.Use()) {
                    if (isLoadingMultiple) {
                        Dialogs.ItemAlreadyExistsDialog.CanShowAlwaysUseNextResultForCurrentQueueOption = true;
                        Dialogs.UnknownFileFormatDialog.CanShowAlwaysUseNextResultForCurrentQueueOption = true;
                    }
                    else {
                        Dialogs.ItemAlreadyExistsDialog.RemoveButtonById("cancel");
                        Dialogs.UnknownFileFormatDialog.RemoveButtonById("cancel");
                        Dialogs.ItemAlreadyExistsDialog.CanShowAlwaysUseNextResultForCurrentQueueOption = false;
                        Dialogs.UnknownFileFormatDialog.CanShowAlwaysUseNextResultForCurrentQueueOption = false;
                    }

                    for (int i = 0; i <= endIndex; i++) {
                        BaseTreeItemViewModel itemThatAlreadyExists = null;
                        BaseTreeItemViewModel itemToRemove = null;
                        string path = paths[i];
                        if (checkAlreadyAdded) {
                            itemThatAlreadyExists = this.Root.FindChild(x => x is IHaveFilePath j && j.FilePath == path);
                            if (itemThatAlreadyExists != null) {
                                string result = await Dialogs.ItemAlreadyExistsDialog.ShowAsync("Item already added", path + " was already added. Do you want to replace it with the new file?");
                                if (result != null && result != "cancel") {
                                    if (result == "ignore") {
                                        continue;
                                    }
                                    else if (result != "replace") {
                                        itemThatAlreadyExists = null;
                                    }
                                }
                                else {
                                    foreach (BaseTreeItemViewModel item in added) {
                                        if (item is IDisposable disposable) {
                                            try {
                                                disposable.Dispose();
                                            }
                                            catch { /* ignored */ }
                                        }

                                        this.Root.RemoveItem(item);
                                    }

                                    return;
                                }
                            }
                        }

                        string extension = Path.GetExtension(path);
                        switch (extension) {
                            case ".mca": {
                                RegionFileViewModel vm = new RegionFileViewModel(path) {
                                    IsBigEndian = this.IsBigEndianDefault
                                };

                                added.Add(vm);
                                this.Root.AddItem(vm); // add before loading to see the entry counter + the IsLoading property working ;)
                                try {
                                    await vm.RefreshAction();
                                    itemToRemove = itemThatAlreadyExists;
                                }
                                catch (Exception e) {
                                    this.Root.RemoveItem(vm);
                                    try {
                                        vm.Dispose();
                                    }
                                    catch { /* ignored */ }

                                    await Dialogs.OpenFileFailureDialog.ShowAsync("Failed to open file", $"Failed to open region file at {path}: \n\n{e.Message}");
                                }

                                break;
                            }
                            case ".dat": {
                                TagDataFileViewModel file = new TagDataFileViewModel(Path.GetFileName(path)) {
                                    IsCompressed = this.IsCompressedDefault,
                                    IsBigEndian = this.IsBigEndianDefault,
                                    FilePath = path
                                };

                                added.Add(file);
                                this.Root.AddItem(file);
                                try {
                                    await file.RefreshAction();
                                    itemToRemove = itemThatAlreadyExists;
                                }
                                catch (Exception e) {
                                    this.Root.RemoveItem(file);
                                    await Dialogs.OpenFileFailureDialog.ShowAsync("Failed to open file", $"Failed to open region file at {path}: \n\n{e.Message}");
                                }

                                break;
                            }
                            default: {
                                await Dialogs.UnknownFileFormatDialog.ShowAsync("Unknown file format", $"Unknown file extension: {extension}");
                                break;
                            }
                        }

                        if (itemToRemove != null) {
                            this.Root.RemoveItem(itemToRemove);
                        }
                    }
                }
            }
        }

        public async Task NavigateToPath(string path) {
            if (this.TreeView.IsNavigating) {
                await IoC.MessageDialogs.ShowMessageAsync("Already navigating", "A navigation is already being processed. Wait for it to finish first");
                return;
            }

            List<BaseTreeItemViewModel> list = await BaseTreeItemViewModel.ResolvePathAction(this.Root, path);
            if (list != null && list.Count > 0) {
                await this.TreeView.NavigateAsync(list);
                this.CurrentFolderItem = list[list.Count - 1];
            }
        }
    }
}
