using REghZy.Streams;

namespace MCNBTEditor.Core.NBT {
    public class NBTTagByte : NBTBase {
        public byte data;

        public override NBTType TagType => NBTType.Byte;

        public NBTTagByte() {
        }

        public NBTTagByte(byte var2) {
            this.data = var2;
        }

        public override void Write(IDataOutput output) {
            output.WriteByte(this.data);
        }

        public override void Read(IDataInput input, int deep) {
            this.data = input.ReadByte();
        }

        public override string ToString() {
            return this.data.ToString();
        }

        public override NBTBase CloneTag() {
            return new NBTTagByte(this.data);
        }

        public override bool Equals(object obj) {
            if (base.Equals(obj) && obj is NBTTagByte tagByte) {
                return this.data == tagByte.data;
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