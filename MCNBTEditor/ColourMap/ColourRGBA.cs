using MCNBTEditor.Core.Utils;

namespace MCNBTEditor.ColourMap {
    public readonly struct Colour {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public byte A { get; }

        public Colour(byte r, byte g, byte b, byte a) {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        public static Colour FromScRGB(float r, float g, float b, float a) {
            return new Colour(
                (byte) Maths.Clamp(r * 255F, 0, 255F),
                (byte) Maths.Clamp(g * 255F, 0, 255F), 
                (byte) Maths.Clamp(b * 255F, 0, 255F),
                (byte) Maths.Clamp(a * 255F, 0, 255F));
        }
    }
}