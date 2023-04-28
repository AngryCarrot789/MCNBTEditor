using System;

namespace MCNBTEditor.Core.NBT {
    public enum NBTType : byte {
        End       = 0,
        Byte      = 1,
        Short     = 2,
        Int       = 3,
        Long      = 4,
        Float     = 5,
        Double    = 6,
        String    = 8,
        ByteArray = 7,
        IntArray  = 11,
        LongArray = 12,
        List      = 9,
        Compound  = 10
    }

    public static class NBTypeExtensions {
        /// <summary>
        /// Compares the 2 types by primary group; primitive, array, list, compound
        /// </summary>
        /// <returns>A comparison value; -1, 0 or +1</returns>
        public static int Compare4(this NBTType a, NBTType b) {
            if (a == b) {
                return 0;
            }
            else if (a.IsPrimitive()) {
                return b.IsPrimitive() ? 0 : 1;
            }
            else if (a.IsArray()) {
                if (b.IsPrimitive()) {
                    return -1;
                }
                else if (b.IsArray()) {
                    return 0;
                }
                else {
                    return 1;
                }
            }
            else if (a == NBTType.List) {
                // Top if statement should handle this, but this is just for clear reading
                if (b == NBTType.List) {
                    return 0;
                }
                else {
                    return b == NBTType.Compound ? 1 : -1;
                }
            }
            else if (a == NBTType.Compound) {
                return b == NBTType.Compound ? 0 : -1;
            }
            else {
                throw new Exception("Unknown NBT Type for first parameter: " + a);
            }
        }

        public static bool IsPrimitive(this NBTType type) {
            switch (type) {
                case NBTType.End:
                case NBTType.Byte:
                case NBTType.Short:
                case NBTType.Int:
                case NBTType.Long:
                case NBTType.Float:
                case NBTType.Double:
                case NBTType.String:    return true;
                case NBTType.ByteArray:
                case NBTType.IntArray:
                case NBTType.LongArray:
                case NBTType.List:
                case NBTType.Compound:  return false;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static bool IsArray(this NBTType type) {
            return type == NBTType.IntArray || type == NBTType.ByteArray || type == NBTType.LongArray;
        }

        public static bool IsCollection(this NBTType type) {
            return type == NBTType.List || type == NBTType.Compound;
        }
    }
}