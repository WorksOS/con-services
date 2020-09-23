using System;
using System.Collections.Generic;
using System.Linq;

namespace CCSS.WorksOS.Reports.Common.Helpers
{
  public static class IEnumerableExtensions
  {
    /// <summary>
    /// Sort one collection based on keys defined in another
    /// </summary>
    /// <returns>Items sorted</returns>
    public static IEnumerable<TResult> SortBy<TResult, TKey>(
      this IEnumerable<TResult> itemsToSort,
      IEnumerable<TKey> sortKeys,
      Func<TResult, TKey> matchFunc)
    {
      return sortKeys.Join(itemsToSort,
        key => key,
        matchFunc,
        (key, iitem) => iitem);
    }
  }
}
