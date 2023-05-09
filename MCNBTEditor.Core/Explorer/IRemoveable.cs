using System.Threading.Tasks;

namespace MCNBTEditor.Core.Explorer {
    public interface IRemoveable {
        bool CanRemoveFromParent();

        Task RemoveFromParentAction();
    }
}