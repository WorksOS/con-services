using System.Drawing;

namespace VSS.TRex.Common.Utilities
{
  public static class ColorUtility
  {
    private const byte BIT_SHIFT_COUNT_RED = 16;
    private const byte BIT_SHIFT_COUNT_GREEN = 8;
    private const byte BIT_SHIFT_COUNT_BLUE = 0;

    public static Color UIntToColor(uint color)
    {
      return Color.FromArgb((byte)(color >> BIT_SHIFT_COUNT_RED), (byte)(color >> BIT_SHIFT_COUNT_GREEN), (byte)(color >> BIT_SHIFT_COUNT_BLUE));
    }

    public static uint ColorToUInt(int r, int g, int b)
    {
      return (uint)((r << BIT_SHIFT_COUNT_RED) | (g << BIT_SHIFT_COUNT_GREEN) | (b << BIT_SHIFT_COUNT_BLUE));
    }
  }
}
