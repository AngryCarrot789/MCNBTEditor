using System.Threading.Tasks;
using MCNBTEditor.Core.Actions;

namespace MCNBTEditor.Core.Explorer.Actions {
    public class RefreshAction : AnAction {
        public RefreshAction() : base() {
        }

        public override Presentation GetPresentation(AnActionEventArgs e) {
            return e.DataContext.HasContext<IRefreshable>() ? Presentation.VisibleAndEnabled : Presentation.VisibleAndDisabled;
        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (e.DataContext.TryGetContext(out IRefreshable refreshable)) {
                await refreshable.RefreshAsync();
                return true;
            }

            return false;
        }
    }
}