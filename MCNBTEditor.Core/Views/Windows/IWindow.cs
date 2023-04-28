using System.Threading.Tasks;

namespace MCNBTEditor.Core.Views.Windows {
    public interface IWindow : IViewBase {
        void CloseWindow();

        Task CloseWindowAsync();
    }
}