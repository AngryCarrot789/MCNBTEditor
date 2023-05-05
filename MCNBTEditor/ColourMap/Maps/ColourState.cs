namespace MCNBTEditor.ColourMap.Maps {
    public readonly struct ColourState {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public byte A { get; }

        public ColourState(byte r, byte g, byte b, byte a) {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }
    }
}