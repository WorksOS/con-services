using System;
using System.Collections.Generic;

namespace VSS.TRex.Common.Extensions
{
  public static class IEnumerableExtensions
  {
    // Iterate over all elements in the IEnumerable supplying them to the given action
    public static void ForEach<T>(this IEnumerable<T> obj, Action<T> action)
    {
      foreach (var i in obj)
        action(i);
    }
  }
}
