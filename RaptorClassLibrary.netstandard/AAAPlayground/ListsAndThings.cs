using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.TRex.AAAPlayground
{
    public class ListsAndThings
    {
        public bool IsZero(int x)
        {
            return x == 0;
        }

        public void DoSomething()
        {
            int[] Bob = new int[4];

            int sum = Bob.Sum();

            bool AllZeroQ = Bob.All(IsZero);
            bool AnyZeroQ = Bob.Any(IsZero);

            List<int> Bob2 = Bob.ToList();

            List<int> Mary = new List<int>{1, 2, 3, 4, 5, 6, 7, 8, 9};

            int sum2 = Mary.Select(x => x*x).Where(x => x < 5).Sum();

            List<int> Mary2 = Mary.Select(x => x * x).ToList();

            IEnumerable<int> Mary2b = Mary.Select(x => x * x);
            Mary2b = Mary2b.Where(x => x < 100);

            List<int> Mary3 = Mary2b.ToList();
            int Mary3Count = Mary3.Count();

            IEnumerable<int> v = Mary;
        }
    }
}
