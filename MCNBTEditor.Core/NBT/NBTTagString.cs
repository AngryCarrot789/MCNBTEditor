using REghZy.Streams;

namespace MCNBTEditor.Core.NBT {
    public class NBTTagString : NBTBase {
        public string data;

        public override NBTType TagType => NBTType.String;

        public NBTTagString() : this("") {

        }

        public NBTTagString(string data) {
            this.data = data ?? "";
        }

        public override void Write(IDataOutput output) {
            if (string.IsNullOrEmpty(this.data)) {
                output.WriteUShort(0);
            }
            else {
                output.WriteStringLabelledUTF8(this.data);
            }
        }

        public override void Read(IDataInput input, int deep) {
            this.data = input.ReadStringUTF8Labelled();
        }

        public override string ToString() {
            return this.data;
        }

        public override NBTBase CloneTag() {
            return new NBTTagString(this.data);
        }

        public override bool Equals(object obj) {
            if (base.Equals(obj) && obj is NBTTagString str) {
                return this.data == null && str.data == null || string.Equals(this.data, str.data);
            }
            else {
                return false;
            }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ (this.data != null ? this.data.GetHashCode() : 0);
        }
    }
}