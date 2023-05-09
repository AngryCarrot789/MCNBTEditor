using System.Threading.Tasks;
using MCNBTEditor.Core.Views.Windows;

namespace MCNBTEditor.Views {
    public class BaseWindow : WindowViewBase, IWindow {
        public void CloseWindow() {
            this.Close();
        }

        public Task CloseWindowAsync() {
            return base.CloseAsync();
        }
    }
}