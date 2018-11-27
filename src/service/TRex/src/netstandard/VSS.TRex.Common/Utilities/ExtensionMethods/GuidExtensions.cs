using System;

namespace VSS.TRex.Common.Utilities.ExtensionMethods
{
  public static class GuidExtensions
  {
    /// <summary>
    /// Determines if one array of GUIDs is equal to another
    /// </summary>
    /// <param name="guidArray"></param>
    /// <param name="otherGuidArray"></param>
    /// <returns></returns>
    public static bool GuidsEqual(this Guid[] guidArray, Guid[] otherGuidArray)
    {
      if (guidArray == otherGuidArray) return true;

      if ((guidArray?.Length ?? 0) == 0 && (otherGuidArray?.Length ?? 0) == 0) return true;

      if (guidArray == null || otherGuidArray == null) return false;

      if (guidArray.Length != otherGuidArray.Length) return false;

      for (int i = 0; i < guidArray.Length - 1; i++)
        if (!guidArray[i].Equals(otherGuidArray[i]))
          return false;

      return true;
    }
  }
}
