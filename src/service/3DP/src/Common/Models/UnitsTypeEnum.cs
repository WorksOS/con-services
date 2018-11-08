using System;

namespace VSS.Productivity3D.Common.Models
{

  /// <summary>
  /// The types of units for user preferences
  /// </summary>
  public enum UnitsTypeEnum
  {
    None = -1,
    US = 0,
    Metric = 1,
    Imperial = 2
  }

  public static class UnitsTypeEnumExtensions
  {
    /// <summary>
    /// Convert from the name of the units to the type
    /// </summary>
    /// <param name="unitsName"></param>
    /// <returns></returns>
    public static UnitsTypeEnum UnitsType(this string unitsName)
    {
      if (string.IsNullOrEmpty(unitsName))
        return UnitsTypeEnum.US;

      if (unitsName == "US Standard")
        unitsName = "US";

      return (UnitsTypeEnum)Enum.Parse(typeof(UnitsTypeEnum), unitsName, true);
    }
  }
}
