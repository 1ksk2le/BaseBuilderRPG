using System;

namespace BaseBuilderRPG.Content
{
    public static class RandomExtensions
    {
        public static float NextFloat(this Random random, float minValue, float maxValue)
        {
            return (float)(random.NextDouble() * (maxValue - minValue) + minValue);
        }

        public static float NextFloat(this Random random)
        {
            return (float)random.NextDouble();
        }
    }
}