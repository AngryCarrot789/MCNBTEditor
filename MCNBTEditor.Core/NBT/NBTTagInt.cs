using REghZy.Streams;

namespace MCNBTEditor.Core.NBT {
    public class NBTTagInt : NBTBase {
        public int data;

        public override NBTType TagType => NBTType.Int;

        public NBTTagInt() {
        }

        public NBTTagInt(int var2) {
            this.data = var2;
        }

        public override void Write(IDataOutput output) {
            output.WriteInt(this.data);
        }

        public override void Read(IDataInput input, int deep) {
            this.data = input.ReadInt();
        }

        public override string ToString() {
            return this.data.ToString();
        }

        public override NBTBase CloneTag() {
            return new NBTTagInt(this.data);
        }

        public override bool Equals(object obj) {
            if (base.Equals(obj)) {
                NBTTagInt var2 = (NBTTagInt) obj;
                return this.data == var2.data;
            }
            else {
                return false;
            }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ this.data;
        }
    }
}