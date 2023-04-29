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

        private bool isSearchActive;
        public bool IsSearchActive {
            get => this.isSearchActive;
            set => this.RaisePropertyChanged(ref this.isSearchActive, value);
        }

        public ObservableCollection<NBTMatchResult> FoundItems { get; }

        public IdleEventService IdleEventService { get; }

        private volatile bool stopTask;
        private Task searchTask;

        public BaseTreeItemViewModel Root { get; }

        public FindViewModel(BaseTreeItemViewModel root) {
            this.Root = root;
            this.isSearchTermEmpty = true;
            this.isSearchActive = false;
            this.CloseCommand = new RelayCommand(this.CloseDialogAction);
            this.FoundItems = new ObservableCollection<NBTMatchResult>();
            this.IdleEventService = new IdleEventService();
            this.IdleEventService.MinimumTimeSinceInput = TimeSpan.FromMilliseconds(200);
            this.IdleEventService.OnIdle += this.OnTickSearch;
        }

        public void StopActiveSearch(bool clearResults) {
            this.StopTaskAndWait(clearResults);
            this.IsSearchActive = false;
            if (clearResults) {
                this.FoundItems.Clear();
            }
        }

        private void OnInputChanged() {
            if (string.IsNullOrEmpty(this.SearchForNameText) && string.IsNullOrEmpty(this.searchForValueText)) {
                this.StopTaskAndWait(true);
                this.FoundItems.Clear();
                this.IdleEventService.CanFireNextTick = false;
                this.IsSearchTermEmpty = true;
                this.IsSearchActive = false;
            }
            else {
                this.IsSearchTermEmpty = false;
                this.IdleEventService.OnInput();
            }
        }

        private void StopTaskAndWait(bool clearItemQueue = true) {
            if (this.searchTask != null) {
                this.stopTask = true;
                this.searchTask.Wait();
            }

            if (clearItemQueue) {
                // lock (this.queuedItemsToAdd) {
                //     this.queuedItemsToAdd.Clear();
                // }
            }
        }

        public void OnTickSearch() {
            this.StopTaskAndWait();
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

            this.stopTask = false;
            string searchName = string.IsNullOrEmpty(this.SearchForNameText) ? null : this.SearchForNameText;
            string searchValue = string.IsNullOrEmpty(this.SearchForValueText) ? null : this.SearchForValueText;
            this.IsSearchActive = true;
            this.searchTask = Task.Run(() => this.TaskMain(searchName, searchValue, nf, vf));
        }

        // nf = name flags, vf = value flags
        private async Task TaskMain(string searchName, string searchValue, FindFlags nf, FindFlags vf) {
            // cheap way of avoid concurrent modification in most cases
            try {
                await this.FindItems(this.Root.Children.ToList(), searchName, searchValue, nf, vf);
            }
            catch (Exception e) {
                Debug.WriteLine(e.ToString());
                await IoC.MessageDialogs.ShowMessageAsync("Search error", $"An error occurred while searching all files: {e.Message}");
            }
            finally {
                this.IsSearchActive = false;
            }
        }

        private async Task FindItems(List<BaseTreeItemViewModel> items, string searchName, string searchValue, FindFlags nf, FindFlags vf) {
            foreach (BaseTreeItemViewModel child in items) {
                bool isMatched = false;
                List<TextRange> nameMatchesNormal = new List<TextRange>();
                List<TextRange> valueMatchesNormal = new List<TextRange>();
                if (this.stopTask) {
                    return;
                }

                if (searchName != null) {
                    if (child is BaseTagCollectionViewModel childCollection) {
                        if (!string.IsNullOrEmpty(childCollection.Name) && this.includeCollectionNameMatches) {
                            isMatched = AcceptName(searchName, childCollection, nf, nameMatchesNormal);
                        }
                    }
                    else if (child is BaseTagViewModel tag && !string.IsNullOrEmpty(tag.Name) && AcceptName(searchName, tag, nf, nameMatchesNormal)) {
                        isMatched = true;
                    }
                }

                string foundValue = null;
                if (searchValue != null) {
                    if (child is TagPrimitiveViewModel || child is BaseTagArrayViewModel) {
                        isMatched = AcceptValue(searchValue, (BaseTagViewModel) child, vf, valueMatchesNormal, out foundValue);
                    }
                }

                if (isMatched && (nameMatchesNormal.Count > 0 || valueMatchesNormal.Count > 0)) {
                    await this.AddItemAsync(new NBTMatchResult((BaseTagViewModel) child, searchName, searchValue, foundValue, nameMatchesNormal, valueMatchesNormal));
                }

                await this.FindItems(child.Children.ToList(), null, searchValue, nf, vf);
            }
        }

        private Task AddItemAsync(NBTMatchResult result) {
            return IoC.Dispatcher.InvokeLaterAsync(() => this.FoundItems.Add(result));
        }

        public void CloseDialogAction() {
            this.Window.CloseWindow();
        }

        public void Dispose() {
            this.IdleEventService?.Dispose();
            this.StopTaskAndWait(true);
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
                default:
                    foundValue = null;
                    return false;
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