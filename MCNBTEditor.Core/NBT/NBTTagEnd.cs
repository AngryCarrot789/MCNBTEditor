using REghZy.Streams;

namespace MCNBTEditor.Core.NBT {
    public class NBTTagEnd : NBTBase {
        public override NBTType TagType => NBTType.End;

        public NBTTagEnd() {

        }

        public override void Write(IDataOutput output) {
        }

        public override void Read(IDataInput input, int deep) {

        }

        public override NBTBase CloneTag() {
            return new NBTTagEnd();
        }

        public override string ToString() {
            return "END";
        }
    }
}