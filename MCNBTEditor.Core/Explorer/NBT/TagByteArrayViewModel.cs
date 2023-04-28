using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Utils;

namespace MCNBTEditor.Core.Explorer.NBT {
    public class TagByteArrayViewModel : BaseTagArrayViewModel {
        public byte[] Data { get; set; }

        public TagByteArrayViewModel(string name = null) : base(name, NBTType.ByteArray) {

        }

        public override NBTBase ToNBT() {
            return new NBTTagByteArray(this.Data);
        }

        public override BaseTagViewModel Clone() {
            return new TagByteArrayViewModel(this.Name) {Data = Arrays.Clone(this.Data)};
        }

        protected override void SetData(NBTBase nbt) {
            this.Data = ((NBTTagByteArray) nbt).data;
        }
    }
}