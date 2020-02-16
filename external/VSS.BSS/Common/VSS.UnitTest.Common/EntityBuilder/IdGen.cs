using System;

namespace VSS.UnitTest.Common
{
  public class IdGen
  {
    public static int GetId()
    {
      return Math.Abs(Guid.NewGuid().GetHashCode());
    }

    public static string StringId()
    {
      return GetId().ToString();
    }
  }
}