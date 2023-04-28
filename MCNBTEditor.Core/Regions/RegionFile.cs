using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zlib;
using REghZy.Streams;

namespace MCNBTEditor.Core.Regions {
    /// <summary>
    /// Minecraft 1.6.4's region file
    /// </summary>
    public class RegionFile : IDisposable { // TODO: implement writing to chunks
        public string FilePath { get; }

        public DateTime LastWriteTime { get; set; }

        public int SizeDelta { get; set; }

        public bool IsReadingBigEndian { get; }

        private readonly FileStream stream;
        private readonly IDataOutput writer;
        private readonly IDataInput reader;
        private readonly List<bool> sectorFree;
        private readonly int[] offsets;
        private readonly int[] chunkTimestamps;

        public RegionFile(string filePath, bool isBigEndian = true) {
            this.FilePath = filePath;
            FileInfo info = new FileInfo(filePath);

            this.stream = info.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            this.IsReadingBigEndian = isBigEndian;
            this.writer = isBigEndian ? (IDataOutput) new DataOutputStream(this.stream) : new DataOutputStreamLE(this.stream);
            this.reader = isBigEndian ? (IDataInput) new DataInputStream(this.stream) : new DataInputStreamLE(this.stream);
            this.LastWriteTime = info.LastWriteTimeUtc;
            this.offsets = new int[1024];
            this.chunkTimestamps = new int[1024];
            long length = info.Length;
            if (length < 4096L) {
                this.stream.Seek(0, SeekOrigin.Begin);
                for (int i = 0; i < 2048; ++i) {
                    this.writer.WriteInt(0);
                }

                // for (int i = 0; i < 1024; ++i) {
                //     this.writer.WriteInt(0);
                // }

                this.SizeDelta += 8192;
            }

            length = info.Length;
            if ((length & 4095L) != 0L) {
                for (long i = 0; i < (length & 4095L); ++i) {
                    this.writer.WriteInt(0);
                    length += 4; // Saves querying the file system
                }
            }

            int sectors = (int) (length / 4096L);
            this.sectorFree = new List<bool>(sectors);
            for (int i = 0; i < sectors; ++i) {
                this.sectorFree.Add(true);
            }

            this.sectorFree[0] = false;
            this.sectorFree[1] = false;
            this.stream.Seek(0, SeekOrigin.Begin);

            for (int i = 0; i < 1024; ++i) {
                int offset = this.reader.ReadInt();
                this.offsets[i] = offset;
                if (offset == 0 || (offset >> 8) + (offset & 255) > this.sectorFree.Count) {
                    continue;
                }

                for (int k = 0, end = offset & 255; k < end; ++k) {
                    this.sectorFree[(offset >> 8) + k] = false;
                }
            }

            for (int i = 0; i < 1024; ++i) {
                int lastModified = this.reader.ReadInt();
                this.chunkTimestamps[i] = lastModified;
            }
        }

        public bool ChunkExists(int x, int z) {
            if (this.IsOutOfBounds(x, z)) {
                return false;
            }

            int offset = this.GetOffset(x, z);
            if (offset == 0) {
                return false;
            }

            int a = offset >> 8;
            int b = offset & 255;
            if (a + b > this.sectorFree.Count) {
                return false;
            }

            this.stream.Seek(a * 4096, SeekOrigin.Begin);
            int c = this.reader.ReadInt();
            if (c > 4096 * b || c <= 0) {
                return false;
            }

            int d = this.stream.ReadByte();
            return d == 1 || d == 2;
        }

        public byte[] GetChunkData(int x, int z, out int type) {
            type = 0;
            if (this.IsOutOfBounds(x, z))
                return null;

            int offset = this.GetOffset(x, z);
            if (offset == 0)
                return null;

            int a = offset >> 8;
            int b = offset & 255;
            if (a + b > this.sectorFree.Count)
                return null;

            this.stream.Seek(a * 4096, SeekOrigin.Begin);
            int len = this.reader.ReadInt();
            if (len > (4096 * b) || len <= 0)
                return null;

            type = this.stream.ReadByte();
            if (type != 1 && type != 2)
                return null;

            byte[] array = new byte[len - 1];
            this.stream.Read(array, 0, array.Length);
            return array;
        }

        public Stream GetChunkInputStream(int x, int z) {
            byte[] array = this.GetChunkData(x, z, out int type);
            if (array == null) {
                return null;
            }

            MemoryStream memory = new MemoryStream(array);
            switch (type) {
                case 1: // GZIP mode
                    return new BufferedStream(new GZipStream(memory, CompressionMode.Decompress), 2048);
                case 2: // Deflate mode
                    return new BufferedStream(new ZlibStream(memory, CompressionMode.Decompress, true), 2048);
                default: return null;
            }
        }

        public bool IsOutOfBounds(int x, int z) {
            return x < 0 || x >= 32 || z < 0 || z >= 32;
        }

        public int GetOffset(int x, int z) {
            return this.offsets[x + z * 32];
        }

        public bool IsChunkSaved(int x, int z) {
            return this.GetOffset(x, z) != 0;
        }

        public void SetOffset(int x, int y, int offset) {
            this.offsets[x + y * 32] = offset;
            this.stream.Seek((x + y * 32) * 4, SeekOrigin.Begin);
            this.writer.WriteInt(offset);
        }

        public void SetChunkTimestamp(int x, int z, int offset) {
            this.chunkTimestamps[x + z * 32] = offset;
            this.stream.Seek(4096 + (x + z * 32) * 4, SeekOrigin.Begin);
            this.writer.WriteInt(offset);
        }

        public void Dispose() {
            this.stream?.Close();
            this.stream?.Dispose();
        }
    }
}