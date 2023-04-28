using System;
using MCNBTEditor.Core.Utils;
using REghZy.Streams;

namespace MCNBTEditor.Core.NBT {
    public class NBTTagByteArray : NBTBase {
        public byte[] data;

        public override NBTType TagType => NBTType.ByteArray;

        public NBTTagByteArray() {
        }

        public NBTTagByteArray(byte[] var2) {
            this.data = var2;
        }

        public override void Write(IDataOutput output) {
            output.WriteInt(this.data.Length);
            output.Write(this.data);
        }

        public override void Read(IDataInput input, int deep) {
            int size = input.ReadInt();
            this.data = new byte[size];
            input.ReadFully(this.data);
        }

        public override string ToString() {
            return "[" + this.data.Length + " bytes]";
        }

        public override NBTBase CloneTag() {
            byte[] var1 = new byte[this.data.Length];
            Array.Copy(this.data, 0, var1, 0, this.data.Length);
            return new NBTTagByteArray(var1);
        }

        public override bool Equals(object obj) {
            if (base.Equals(obj) && obj is NBTTagByteArray arr) {
                return Arrays.Equals(this.data, arr.data, (a, b) => a == b);
            }
            else {
                return false;
            }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ Arrays.Hash(this.data);
        }
    }
}