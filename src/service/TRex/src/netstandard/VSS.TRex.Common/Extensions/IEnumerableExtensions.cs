using System;
using System.Collections.Generic;

namespace VSS.TRex.Common.Extensions
{
  public static class IEnumerableExtensions
  {
    /// <summary>
    /// Iterate over all elements in the IEnumerable supplying them to the given action
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="action"></param>
    public static void ForEach<T>(this IEnumerable<T> obj, Action<T> action)
    {
      foreach (var i in obj)
        action(i);
    }

    /// <summary>
    /// Iterate over all elements in the IEnumerable supplying them to the given action along with
    /// the index of the item in the IEnumerable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="action"></param>
    public static void ForEach<T>(this IEnumerable<T> obj, Action<T, int> action)
    {
      int counter = 0;
      foreach (var i in obj)
        action(i, counter++);
    }
  }
}
