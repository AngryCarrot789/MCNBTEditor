using System.Threading.Tasks;
using MCNBTEditor.Core.Views.Windows;

namespace MCNBTEditor.Views {
    public class BaseWindow : BaseWindowCore, IWindow {
        public void CloseWindow() {
            this.Close();
        }

        public async Task CloseWindowAsync() {
            await this.Dispatcher.InvokeAsync(this.CloseWindow);
        }
    }
}