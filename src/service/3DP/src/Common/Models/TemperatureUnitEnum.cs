using System;
using VSS.Productivity3D.Models.Enums;

namespace VSS.Productivity3D.Common.Models
{
  public static class TemperatureUnitEnumExtensions
  {
    /// <summary>
    /// Convert from the name of the temperature units to the type
    /// </summary>
    /// <param name="temperatureName"></param>
    /// <returns></returns>
    public static TemperatureUnitEnum TemperatureUnitType(this string temperatureName)
    {
      if (string.IsNullOrEmpty(temperatureName))
        return TemperatureUnitEnum.Celsius;

      return (TemperatureUnitEnum)Enum.Parse(typeof(TemperatureUnitEnum), temperatureName, true);
    }
  }
}
