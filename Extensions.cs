using System;

namespace Nanopath
{
    public static class Extensions
    {
        public static T CoerceToLimits<T>(this T value, T min, T max)
            where T : IComparable
        {
            bool coerced;
            value = value.CoerceToLimits(min, max, out coerced);
            return value;
        }

        public static T CoerceToLimits<T>(this T value, T min, T max, out bool coerced) 
            where T : IComparable
        {
            coerced = false;
            if (value != null)
            {
                if (value.CompareTo(min) < 0)
                {
                    value = min;
                    coerced = true;
                }
                else if (value.CompareTo(max) > 0)
                {
                    value = max;
                    coerced = true;
                }
            }
            return value;
        }
    }
}
