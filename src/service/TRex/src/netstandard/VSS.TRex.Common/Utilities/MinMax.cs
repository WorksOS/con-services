using System;

namespace VSS.TRex.Common.Utilities
{
    public static class MinMax
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        public static void SetMinMax(ref double a, ref double b)
        {
            if (a > b)
            {
                Swap(ref a, ref b);
            }
        }

        public static void SetMinMax<T>(ref T a, ref T b) where T : IComparable<T>
        {
            if (a.CompareTo(b) == 1)
            {
                Swap(ref a, ref b);
            }
        }
    }
}
