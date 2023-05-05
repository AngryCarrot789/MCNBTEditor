using MCNBTEditor.Core.Utils;

namespace MCNBTEditor.ColourMap {
    public readonly struct ColourRGBA {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public byte A { get; }

        public float ScR { get => this.R / 255F; }
        public float ScG { get => this.G / 255F; }
        public float ScB { get => this.B / 255F; }
        public float ScA { get => this.A / 255F; }

        public ColourRGBA(byte r, byte g, byte b, byte a) {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        public static ColourRGBA FromScRGB(float r, float g, float b, float a) {
            return new ColourRGBA(
                (byte) Maths.Clamp(r * 255F, 0, 255F),
                (byte) Maths.Clamp(g * 255F, 0, 255F),
                (byte) Maths.Clamp(b * 255F, 0, 255F),
                (byte) Maths.Clamp(a * 255F, 0, 255F));
        }
    }
}