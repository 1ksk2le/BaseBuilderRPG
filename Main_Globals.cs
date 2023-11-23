using System;

namespace BaseBuilderRPG
{
    public static class Main_Globals
    {
        private static readonly Random random = new Random();

        public static Random GetRandomInstance()
        {
            int seed = Guid.NewGuid().GetHashCode();
            return new Random(seed);
        }
    }

}
