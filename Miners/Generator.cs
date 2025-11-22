using System;

namespace Miners
{
    public class Generator
    {
        protected static Random random = new Random();

        public static int Next(int maxValue)
        {
            return random.Next(maxValue);
        }

        public static bool Chance(float chance)
        {
            return Next(100) / 100.0 < chance;
        }
    }
}
