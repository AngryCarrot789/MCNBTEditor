using System;
using System.Collections.Generic;
using REghZy.Streams;

namespace MCNBTEditor.Core.NBT {
    public class NBTTagList : NBTBase {
        public readonly List<NBTBase> tags;
        public byte tagType;

        public override NBTType TagType => NBTType.List;

        public NBTTagList() {
            this.tags = new List<NBTBase>();
        }

        public NBTTagList(byte tagType) : this() {
            this.tagType = tagType;
        }

        public override void Write(IDataOutput output) {
            this.tagType = this.tags.Count == 0 ? (byte) 0 : (byte) this.tags[0].TagType;
            output.WriteByte(this.tagType);
            output.WriteInt(this.tags.Count);
            foreach (NBTBase tag in this.tags) {
                tag.Write(output);
            }
        }

        public override void Read(IDataInput input, int deep) {
            if (deep <= 512 || deep <= AlternativeMaxStackDepth) {
                this.tagType = input.ReadByte();
                int count = input.ReadInt();
                if (this.tagType == 0 && count > 0) {
                    throw new Exception("Missing tag type for tags. Expected " + count + " items of TagEnd");
                }

                this.tags.Clear();
                if (count > this.tags.Count) {
                    this.tags.Capacity = count; // slight performance helper
                }

                for (int k = 0; k < count; ++k) {
                    NBTBase nbtbase = CreateTag(this.tagType);
                    nbtbase.Read(input, deep + 1);
                    this.tags.Add(nbtbase);
                }
            }
            else {
                throw new Exception("Tried to read NBT tag with too high complexity, depth > 512");
            }
        }

        public override NBTBase CloneTag() {
            NBTTagList copy = new NBTTagList(this.tagType);
            foreach (NBTBase nbtBase in this.tags) {
                copy.tags.Add(nbtBase.CloneTag());
            }

            return copy;
        }

        public override bool Equals(object obj) {
            if (base.Equals(obj) && obj is NBTTagList list && this.tagType == list.tagType) {
                return this.tags.Equals(list.tags);
            }

            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ this.tags.GetHashCode();
        }
    }
}