using System;

namespace MCNBTEditor.Core.Utils {
    public static class Maths {
        public static double Map(double input, double inA, double inB, double outA, double outB) {
            return outA + ((outB - outA) / (inB - inA) * (input - inA));
        }

        public static float Clamp(float value, float min, float max) {
            return Math.Max(Math.Min(value, max), min);
        }

        public static double Clamp(double value, double min, double max) {
            return Math.Max(Math.Min(value, max), min);
        }

        public static byte Clamp(byte value, byte min, byte max) {
            return value < min ? min : (value > max ? max : value);
        }

        public static int Clamp(int value, int min, int max) {
            return value < min ? min : (value > max ? max : value);
        }

        public static long Clamp(long value, long min, long max) {
            return value < min ? min : (value > max ? max : value);
        }
    }
}