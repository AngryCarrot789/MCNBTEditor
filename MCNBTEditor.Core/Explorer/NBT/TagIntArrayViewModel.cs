using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Utils;

namespace MCNBTEditor.Core.Explorer.NBT {
    public class TagIntArrayViewModel : BaseTagArrayViewModel {
        public int[] Data { get; set; }

        public TagIntArrayViewModel(string name = null) : base(name, NBTType.IntArray) {

        }

        public override NBTBase ToNBT() {
            return new NBTTagIntArray(this.Data);
        }

        public override BaseTagViewModel Clone() {
            return new TagIntArrayViewModel(this.Name) {Data = Arrays.Clone(this.Data)};
        }

        protected override void SetData(NBTBase nbt) {
            this.Data = ((NBTTagIntArray) nbt).data;
        }
    }
}