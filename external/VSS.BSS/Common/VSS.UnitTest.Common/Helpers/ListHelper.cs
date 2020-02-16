using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VSS.UnitTest.Common
{
  public static class ListHelper
  {
    public delegate bool FindT<T>(T item, List<T> list);

    public static void ValidateList<T>(List<T> expectedList, List<T> actualList, string context, FindT<T> findT)
    {
      if (expectedList == null)
        Assert.IsNull(actualList, context + " should be null");
      else
      {
        Assert.AreEqual(expectedList.Count, actualList.Count, "Wrong number of " + context);
        foreach (T item in expectedList)
        {
          Assert.IsTrue(findT(item, actualList), "Failed to find item in list: " + context);
        }
      }
    }
  }
}
