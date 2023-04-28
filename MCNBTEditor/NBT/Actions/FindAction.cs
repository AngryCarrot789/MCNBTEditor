using System;
using System.Threading.Tasks;
using MCNBTEditor.Core.Actions;
using MCNBTEditor.Core.Explorer;
using MCNBTEditor.Views.Main;
using MCNBTEditor.Views.NBT.Finding;

namespace MCNBTEditor.NBT.Actions {
    [ActionRegistration("actions.nbt.find")]
    public class FindAction : AnAction {
        public const string GlobalFindKey = "IsGlobalFind";

        private FindNBTWindow currentWindow;

        public FindAction() : base("Find", "Search for a tag") {

        }

        // Too lazy to implement a FindViewService so i'm implementing finding here lolol

        public override Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (this.currentWindow != null) {
                this.currentWindow.Activate();
                return Task.FromResult(true);
            }

            BaseTreeItemViewModel root = null;
            if (e.DataContext.TryGetContext(out BaseTreeItemViewModel item)) {
                root = item;
            }
            else if (e.DataContext.TryGetContext(out MainViewModel mvm)) {
                root = mvm.Root;
            }

            if (root == null)
                return Task.FromResult(false);

            bool isRoot = e.DataContext.HasFlag(GlobalFindKey);
            if (isRoot) {
                root = root.RootParent;
            }

            FindNBTWindow window = new FindNBTWindow(root);
            this.currentWindow = window;
            window.Closed += this.WindowOnClosed;
            if (isRoot || root.IsRoot) {
                window.Title = "Searching for anything (at root level)";
            }
            else {
                window.Title = $"Searching in {root.TreeFullPath}";
            }

            window.Show(); // no point in ShowAsync() because we should be on the main thread
            return Task.FromResult(true);
        }

        private void WindowOnClosed(object sender, EventArgs e) {
            if (this.currentWindow?.DataContext is FindViewModel fvm) {
                try {
                    fvm.Dispose();
                }
                catch { /* ignored */ }
            }

            this.currentWindow = null;
        }
    }
}