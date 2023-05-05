using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using MCNBTEditor.Core;
using MCNBTEditor.Core.Explorer;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.Timing;
using MCNBTEditor.Core.Utils;
using MCNBTEditor.Core.Views.Windows;
using MCNBTEditor.Core.Views.Dialogs.Message;

namespace MCNBTEditor.Views.NBT.Finding {
    public class FindViewModel : BaseWindowViewModel, IDisposable {
        public ICommand CloseCommand { get; }

        private bool isNameRegex;
        public bool IsNameRegex {
            get => this.isNameRegex;
            set {
                this.RaisePropertyChangedIfChanged(ref this.isNameRegex, value);
                if (value && this.IsNameSearchingWholeWord) {
                    this.IsNameSearchingWholeWord = false;
                }
            }
        }

        private bool isNameSearchingWholeWord;
        public bool IsNameSearchingWholeWord {
            get => this.isNameSearchingWholeWord;
            set {
                this.RaisePropertyChangedIfChanged(ref this.isNameSearchingWholeWord, value);
                if (value && this.IsNameRegex) {
                    this.IsNameRegex = false;
                }
            }
        }

        private bool isNameCaseSensitive;
        public bool IsNameCaseSensitive {
            get => this.isNameCaseSensitive;
            set => this.RaisePropertyChangedIfChanged(ref this.isNameCaseSensitive, value);
        }

        private bool includeCollectionNameMatches;
        public bool IncludeCollectionNameMatches {
            get => this.includeCollectionNameMatches;
            set => this.RaisePropertyChangedIfChanged(ref this.includeCollectionNameMatches, value);
        }

        private bool isValueRegex;
        public bool IsValueRegex {
            get => this.isValueRegex;
            set {
                this.RaisePropertyChangedIfChanged(ref this.isValueRegex, value);
                if (value && this.IsValueSearchingWholeWord) {
                    this.IsValueSearchingWholeWord = false;
                }
            }
        }

        private bool isValueSearchingWholeWord;
        public bool IsValueSearchingWholeWord {
            get => this.isValueSearchingWholeWord;
            set {
                this.RaisePropertyChangedIfChanged(ref this.isValueSearchingWholeWord, value);
                if (value && this.IsValueRegex) {
                    this.IsValueRegex = false;
                }
            }
        }

        private bool isValueCaseSentsitive;
        public bool IsValueCaseSentsitive {
            get => this.isValueCaseSentsitive;
            set => this.RaisePropertyChangedIfChanged(ref this.isValueCaseSentsitive, value);
        }

        private string searchForNameText;
        public string SearchForNameText {
            get => this.searchForNameText;
            set {
                this.RaisePropertyChanged(ref this.searchForNameText, value);
                this.OnInputChanged();
            }
        }

        private string searchForValueText;
        public string SearchForValueText {
            get => this.searchForValueText;
            set {
                this.RaisePropertyChanged(ref this.searchForValueText, value);
                this.OnInputChanged();
            }
        }

        private bool isSearchTermEmpty;
        public bool IsSearchTermEmpty {
            get => this.isSearchTermEmpty;
            set => this.RaisePropertyChanged(ref this.isSearchTermEmpty, value);
        }

        private volatile bool isSearchActive;
        public bool IsSearchActive {
            get => this.isSearchActive;
            set {
                if (this.isSearchActive == value)
                    return;
                this.isSearchActive = value;
                this.RaisePropertyChanged();
                this.StopSearchCommand.RaiseCanExecuteChanged();
            }
        }

        private volatile bool isSearchBarEnabled;
        public bool IsSearchBarEnabled {
            get => this.isSearchBarEnabled;
            set {
                this.isSearchBarEnabled = value;
                this.RaisePropertyChanged();
            }
        }

        public AsyncRelayCommand StopSearchCommand { get; }

        public ObservableCollection<NBTMatchResult> FoundItems { get; }
        private readonly List<NBTMatchResult> queuedResults;
        private int callbackFlag;

        public IdleEventService IdleEventService { get; }

        private volatile bool stopTask;
        private Task searchTask;

        public BaseTreeItemViewModel Root { get; }

        public FindViewModel(BaseTreeItemViewModel root) {
            this.Root = root;
            this.isSearchTermEmpty = true;
            this.isSearchActive = false;
            this.isSearchBarEnabled = true;
            this.CloseCommand = new RelayCommand(this.CloseDialogAction);
            this.FoundItems = new ObservableCollection<NBTMatchResult>();
            this.IdleEventService = new IdleEventService();
            this.IdleEventService.MinimumTimeSinceInput = TimeSpan.FromMilliseconds(200);
            this.IdleEventService.OnIdle += this.OnTickSearch;
            this.queuedResults = new List<NBTMatchResult>();
            this.StopSearchCommand = new AsyncRelayCommand(async () => {
                this.MarkTaskToStop();
                if (this.searchTask != null) {
                    this.IsSearchBarEnabled = false;
                    await Task.Run(async () => await this.searchTask);
                    this.IsSearchBarEnabled = true;
                }
            }, () => this.IsSearchActive);
        }

        private void OnInputChanged() {
            if (string.IsNullOrEmpty(this.SearchForNameText) && string.IsNullOrEmpty(this.searchForValueText)) {
                this.MarkTaskToStop();
                this.IdleEventService.CanFireNextTick = false;
                this.IsSearchTermEmpty = true;
                this.IsSearchActive = false;
            }
            else {
                this.IsSearchTermEmpty = false;
                this.IdleEventService.OnInput();
                this.MarkTaskToStop();
            }
        }

        private void MarkTaskToStop() {
            this.stopTask = true;
        }

        public void OnTickSearch() {
            this.MarkTaskToStop();
            if (string.IsNullOrEmpty(this.SearchForNameText) && string.IsNullOrEmpty(this.SearchForValueText)) {
                this.IsSearchTermEmpty = true;
                this.IsSearchActive = false;
                return;
            }

            this.FoundItems.Clear();

            FindFlags nf = FindFlags.None;
            if (this.isNameRegex) nf |= FindFlags.Regex;
            if (this.isNameSearchingWholeWord) nf |= FindFlags.Words;
            if (this.isNameCaseSensitive) nf |= FindFlags.Cases;

            FindFlags vf = FindFlags.None;
            if (this.isValueRegex) vf |= FindFlags.Regex;
            if (this.isValueSearchingWholeWord) vf |= FindFlags.Words;
            if (this.isValueCaseSentsitive) vf |= FindFlags.Cases;

            string searchName = string.IsNullOrEmpty(this.SearchForNameText) ? null : this.SearchForNameText;
            string searchValue = string.IsNullOrEmpty(this.SearchForValueText) ? null : this.SearchForValueText;
            this.IsSearchActive = true;
            Task oldTask = this.searchTask;
            this.searchTask = Task.Run(async () => {
                if (oldTask != null && !oldTask.IsCompleted) {
                    await oldTask;
                }

                this.stopTask = false;
                await this.TaskMain(searchName, searchValue, nf, vf);
            });
        }

        // nf = name flags, vf = value flags
        private async Task TaskMain(string searchName, string searchValue, FindFlags nf, FindFlags vf) {
            try {
                // Using non-async is much faster... however, adding items to the GUI via the dispatcher call must be done on
                // background priority because the search function is too fast to the point where it would lag the UI

                // But... the async version is slower due to the async overhead, and as a result of that, it can get away with
                // adding items on normal dispatcher priority (not background), because the async version is just slow enough
                // And adding items on background priority in the async version makes it waaay slower than non-async

                // Which technically means, async is faster than non-async in this case (when non-async uses background and async uses normal)
                // await this.FindItemsAsync(this.Root.Children.ToList(), searchName, searchValue, nf, vf);
                this.IsSearchActive = true;
                await this.FindItemsAsync(this.Root.Children.ToList(), searchName, searchValue, nf, vf);
            }
            catch (Exception e) {
                await this.HandleTaskMainError(e);
            }
            finally {
                this.IsSearchActive = false;
            }
        }

        private async Task HandleTaskMainError(Exception e) {
            string etos = e.ToString();
            Debug.WriteLine(etos);
            string msg;
            if (e is InvalidOperationException) {
                msg = "This is possibly due to items being modified during a search (added/removed tag, sorting, etc). " +
                    "\nThe search feature uses multithreading so that the UI doesn't freeze up, but as a result of this, " +
                    "it opens the possibility of errors when it scans something just as it's being modified." +
                    "\n\nFull Exception: " + etos;
            }
            else {
                msg = "Full exception: " + etos;
            }

            await Dialogs.OkDialog.ShowAsync(e.GetType().Name, "An error occurred while searching all files", msg);
        }

        private async Task FindItemsAsync(List<BaseTreeItemViewModel> items, string searchName, string searchValue, FindFlags nf, FindFlags vf) {
            foreach (BaseTreeItemViewModel child in items) {
                bool isMatched = false;
                if (this.stopTask) {
                    return;
                }

                List<TextRange> nameMatches = new List<TextRange>();
                List<TextRange> valueMatches = new List<TextRange>();
                if (searchName != null) {
                    if (child is BaseTagCollectionViewModel childCollection) {
                        if (!string.IsNullOrEmpty(childCollection.Name) && this.includeCollectionNameMatches) {
                            isMatched = AcceptName(searchName, childCollection, nf, nameMatches);
                        }
                    }
                    else if (child is BaseTagViewModel tag && !string.IsNullOrEmpty(tag.Name) && AcceptName(searchName, tag, nf, nameMatches)) {
                        isMatched = true;
                    }
                }

                string foundValue = null;
                if (searchValue != null) {
                    if (child is TagPrimitiveViewModel || child is BaseTagArrayViewModel) {
                        isMatched = AcceptValue(searchValue, (BaseTagViewModel) child, vf, valueMatches, out foundValue);
                    }
                }

                if (this.stopTask) {
                    return;
                }

                if (isMatched && (nameMatches.Count > 0 || valueMatches.Count > 0)) {
                    await this.AddItemAsync(new NBTMatchResult((BaseTagViewModel) child, searchName, searchValue, foundValue, nameMatches, valueMatches));
                }

                await this.FindItemsAsync(child.Children.ToList(), searchName, searchValue, nf, vf);
            }
        }

        private void FindItems(List<BaseTreeItemViewModel> items, string searchName, string searchValue, FindFlags nf, FindFlags vf) {
            foreach (BaseTreeItemViewModel child in items) {
                bool isMatched = false;
                if (this.stopTask) {
                    return;
                }

                List<TextRange> nameMatches = new List<TextRange>();
                List<TextRange> valueMatches = new List<TextRange>();
                if (searchName != null) {
                    if (child is BaseTagCollectionViewModel childCollection) {
                        if (!string.IsNullOrEmpty(childCollection.Name) && this.includeCollectionNameMatches) {
                            isMatched = AcceptName(searchName, childCollection, nf, nameMatches);
                        }
                    }
                    else if (child is BaseTagViewModel tag && !string.IsNullOrEmpty(tag.Name) && AcceptName(searchName, tag, nf, nameMatches)) {
                        isMatched = true;
                    }
                }

                string foundValue = null;
                if (searchValue != null) {
                    if (child is TagPrimitiveViewModel || child is BaseTagArrayViewModel) {
                        isMatched = AcceptValue(searchValue, (BaseTagViewModel) child, vf, valueMatches, out foundValue);
                    }
                }

                if (isMatched && (nameMatches.Count > 0 || valueMatches.Count > 0)) {
                    NBTMatchResult result = new NBTMatchResult((BaseTagViewModel) child, searchName, searchValue, foundValue, nameMatches, valueMatches);
                    IoC.Dispatcher.InvokeLater(() => this.FoundItems.Add(result), true);
                    // await this.AddemAsync(new NBTMatchResult((BaseTagViewModel) child, searchName, searchValue, foundValue, nameMatches, valueMatches));
                }

                this.FindItems(child.Children.ToList(), searchName, searchValue, nf, vf);
            }
        }

        private Task AddItemAsync(NBTMatchResult result) {
            return IoC.Dispatcher.InvokeLaterAsync(() => this.FoundItems.Add(result), true);
        }

        public void CloseDialogAction() {
            this.Window.CloseWindow();
        }

        public void Dispose() {
            this.IdleEventService?.Dispose();
            this.MarkTaskToStop();
        }

        private static bool AcceptName(string pattern, BaseTagViewModel nbt, FindFlags flags, List<TextRange> matches) {
            if (flags.IsRegex()) {
                if (!AcceptValueRegex(pattern, nbt.Name, flags.IsIgnoreCase(), matches)) {
                    return false;
                }
            }
            else {
                if (!AcceptValue(pattern, nbt.Name, flags.IsIgnoreCase(), flags.IsWords(), matches)) {
                    return false;
                }
            }

            return true;
        }

        private static bool AcceptValue(string pattern, BaseTagViewModel nbt, FindFlags flags, List<TextRange> matches, out string foundValue) {
            switch (nbt) {
                case TagPrimitiveViewModel model: foundValue = model.Data ?? "";
                    break;
                case TagLongArrayViewModel model: {
                    long[] data = model.Data;
                    foundValue = data != null && data.Length > 0 ? string.Join(",", data) : "";
                    break;
                }
                case TagIntArrayViewModel model: {
                    int[] data = model.Data;
                    foundValue = data != null && data.Length > 0 ? string.Join(",", data) : "";
                    break;
                }
                case TagByteArrayViewModel model: {
                    byte[] data = model.Data;
                    foundValue = data != null && data.Length > 0 ? string.Join(",", data) : "";
                    break;
                }
                default: {
                    foundValue = null;
                    return false;
                }
            }

            if (flags.IsRegex()) {
                if (!AcceptValueRegex(pattern, foundValue, flags.IsIgnoreCase(), matches)) {
                    return false;
                }
            }
            else {
                if (!AcceptValue(pattern, foundValue, flags.IsIgnoreCase(), flags.IsWords(), matches)) {
                    return false;
                }
            }

            return true;
        }

        private static bool AcceptValueRegex(string pattern, string value, bool ignoreCase, List<TextRange> matches) {
            MatchCollection collection = Regex.Matches(value, pattern, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            if (collection.Count < 1) {
                return false;
            }

            foreach (Match match in collection) {
                matches.Add(new TextRange(match.Index, match.Length));
            }

            return true;
        }

        private static bool AcceptValue(string pattern, string value, bool ignoreCase, bool words, List<TextRange> matches) {
            int len = pattern.Length, i = -len;
            StringComparison compType = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
            while ((i = value.IndexOf(pattern, i + len, compType)) != -1) {
                int endIndex = i + len;
                if (!words || ((i == 0 || value[i - 1] == ' ') && (endIndex >= value.Length || value[endIndex] == ' '))) {
                    matches.Add(new TextRange(i, len));
                }
            }

            return matches.Count > 0;
        }
    }
}