using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Utils;

namespace MCNBTEditor.Core.Explorer.NBT {
    public class TagLongArrayViewModel : BaseTagArrayViewModel {
        public long[] Data { get; set; }

        public TagLongArrayViewModel(string name = null) : base(name, NBTType.LongArray) {

        }

        public override NBTBase ToNBT() {
            return new NBTTagLongArray(this.Data);
        }

        public override BaseTagViewModel Clone() {
            return new TagLongArrayViewModel(this.Name) {Data = Arrays.Clone(this.Data)};
        }

        protected override void SetData(NBTBase nbt) {
            this.Data = ((NBTTagLongArray) nbt).data;
        }
    }
}