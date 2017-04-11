using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Utilities
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
                Swap<double>(ref a, ref b);
            }
        }

        public static void SetMinMax<T>(ref T a, ref T b) where T : IComparable<T>
        {
            if (a.CompareTo(b) == 1)
            {
                Swap<T>(ref a, ref b);
            }
        }
    }
}
