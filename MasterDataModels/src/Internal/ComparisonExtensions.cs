using System.Collections.Generic;
using System.Linq;

namespace VSS.MasterData.Models.Models
{
  public static class ComparisonExtensions
  {
    public static bool ScrambledEquals<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
    {
      var cnt = new Dictionary<T, int>();
      foreach (T s in list1)
      {
        if (cnt.ContainsKey(s))
        {
          cnt[s]++;
        }
        else
        {
          cnt.Add(s, 1);
        }
      }
      foreach (T s in list2)
      {
        if (cnt.ContainsKey(s))
        {
          cnt[s]--;
        }
        else
        {
          return false;
        }
      }
      return cnt.Values.All(c => c == 0);
    }

    public static int GetListHashCode<T>(this IEnumerable<T> list)
    {
      unchecked
      {
        int hash = 19;
        foreach (var foo in list)
        {
          hash = hash * 31 + foo.GetHashCode();
        }
        return hash;
      }
    }
  }
}