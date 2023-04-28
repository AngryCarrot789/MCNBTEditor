using System;
using System.Collections.Generic;
using REghZy.Streams;

namespace MCNBTEditor.Core.NBT {
    public class NBTTagCompound : NBTBase {
        public readonly Dictionary<string, NBTBase> map;

        public override NBTType TagType => NBTType.Compound;

        public NBTTagCompound() {
            this.map = new Dictionary<string, NBTBase>();
        }

        public override void Read(IDataInput input, int deep) {
            if (deep <= 512 || deep <= AlternativeMaxStackDepth) {
                this.map.Clear();
                while (ReadTag(input, deep + 1, out string name, out NBTBase nbt)) {
                    this.map[name ?? ""] = nbt;
                }
            }
            else {
                throw new Exception("Tried to read NBT tag with too high complexity, depth > 512");
            }
        }

        public override void Write(IDataOutput output) {
            foreach (KeyValuePair<string, NBTBase> pair in this.map) {
                WriteTag(output, pair.Key, pair.Value);
            }
            output.WriteByte(0);
        }

        public void Put(string key, NBTBase nbt) {
            this.map[key] = nbt;
        }

        public override NBTBase CloneTag() {
            NBTTagCompound nbt = new NBTTagCompound();
            foreach (KeyValuePair<string, NBTBase> pair in this.map) {
                nbt.map[pair.Key] = Clone(pair.Value);
            }
            return nbt;
        }
    }
}