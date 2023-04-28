using REghZy.Streams;

namespace MCNBTEditor.Core.NBT {
    public class NBTTagFloat : NBTBase {
        public float data;

        public override NBTType TagType => NBTType.Float;

        public NBTTagFloat() {
        }

        public NBTTagFloat(float var2) {
            this.data = var2;
        }

        public override void Write(IDataOutput output) {
            output.WriteFloat(this.data);
        }

        public override void Read(IDataInput input, int deep) {
            this.data = input.ReadFloat();
        }


        public override string ToString() {
            return this.data.ToString();
        }

        public override NBTBase CloneTag() {
            return new NBTTagFloat(this.data);
        }

        public override bool Equals(object obj) {
            if (base.Equals(obj)) {
                NBTTagFloat var2 = (NBTTagFloat) obj;
                return this.data == var2.data;
            }
            else {
                return false;
            }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ this.data.GetHashCode();
        }
    }
}