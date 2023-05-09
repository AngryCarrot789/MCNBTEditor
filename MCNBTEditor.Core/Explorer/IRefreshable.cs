using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MCNBTEditor.Core.Explorer {
    public interface IRefreshable {
        Task RefreshAsync();
    }
}