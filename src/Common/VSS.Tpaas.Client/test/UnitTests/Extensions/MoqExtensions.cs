using System;
using System.Collections;
using System.Collections.Generic;
using Moq.Language.Flow;

namespace VSS.Tpaas.Client.UnitTests.Extensions
{
  public static class MoqExtensions
  {
    public static void ReturnsInOrder<T, TResult>(
      this ISetup<T, TResult> setup,
      params TResult[] results)
      where T : class
    {
      setup.Returns(new Func<TResult>(new Queue<TResult>((IEnumerable<TResult>) results).Dequeue));
    }

    public static void ReturnsInOrder<T, TResult>(
      this ISetup<T, TResult> setup,
      params object[] results)
      where T : class
    {
      var queue = new Queue((ICollection) results);
      setup.Returns((Func<TResult>) (() =>
      {
        var obj = queue.Dequeue();
        
        if (obj is Exception exception)
        {
          throw exception;
        }

        return (TResult) obj;
      }));
    }
  }
}
