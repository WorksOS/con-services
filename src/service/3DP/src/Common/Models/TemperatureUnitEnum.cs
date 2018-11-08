using System;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// The temperature units for user preferences
  /// </summary>
  public enum TemperatureUnitEnum
  {
    None = 0,
    Celsius = 1,
    Fahrenheit = 2
  }

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
