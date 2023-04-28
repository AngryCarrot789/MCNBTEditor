using MCNBTEditor.Core.NBT;

namespace MCNBTEditor.Core.Explorer.NBT {
    public abstract class BaseTagArrayViewModel : BaseTagViewModel {
        protected BaseTagArrayViewModel(string name, NBTType type) : base(name, type) {

        }

        protected abstract void SetData(NBTBase nbt);
    }
}