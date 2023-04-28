using System;

namespace MCNBTEditor.Views.NBT.Finding {
    [Flags]
    public enum FindFlags {
        None = 0,
        Regex = 1,
        Words = 2,
        Cases = 4
    }

    public static class FindFlagExtensions {
        public static bool IsRegex(this FindFlags flags) {
            return (flags & FindFlags.Regex) != 0;
        }

        public static bool IsWords(this FindFlags flags) {
            return (flags & FindFlags.Words) != 0;
        }

        public static bool IsCaseSensitive(this FindFlags flags) {
            return (flags & FindFlags.Cases) != 0;
        }

        public static bool IsIgnoreCase(this FindFlags flags) {
            return (flags & FindFlags.Cases) ==  0;
        }
    }
}