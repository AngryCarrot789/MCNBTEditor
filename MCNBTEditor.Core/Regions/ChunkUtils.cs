using System.IO;
using MCNBTEditor.Core.NBT;
using REghZy.Streams;

namespace MCNBTEditor.Core.Regions {
    public static class ChunkLoader {
        public static NBTTagCompound ReadChunkTag(RegionFile file, int x, int z) {
            Stream input = file.GetChunkInputStream(x, z);
            if (input == null) {
                return null;
            }

            IDataInput dataInput = CompressedStreamTools.CreateInput(input, true);
            if (NBTBase.ReadTag(dataInput, 0, out _, out NBTBase nbt) && nbt is NBTTagCompound compound) {
                return compound;
            }

            return null;
        }
    }
}