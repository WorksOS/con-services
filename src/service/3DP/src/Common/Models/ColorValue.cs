using System.Globalization;

namespace VSS.Productivity3D.Common.Models
{
  public class ColorValue
  {
    public uint color;
    public double value;

    public ColorValue() { }

    public static uint ColorFromString(string colorElement)
    {
      uint color = Colors.None;
      if (!string.IsNullOrEmpty(colorElement) && colorElement.StartsWith("#") && (colorElement.Length >= 7 && colorElement.Length <= 9))
      {
        uint.TryParse(colorElement.Substring(1), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out color);
      }
      return color;
    }
  }
}