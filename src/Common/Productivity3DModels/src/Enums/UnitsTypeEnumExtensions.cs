using System;

namespace VSS.Productivity3D.Models.Enums
{
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
