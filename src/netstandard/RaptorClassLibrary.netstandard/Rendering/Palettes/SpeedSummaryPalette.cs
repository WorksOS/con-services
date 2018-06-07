using System.Drawing;
using VSS.TRex.Cells;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Palettes
{
  public class SpeedSummaryPalette : PaletteBase
  {
    private Color OverSpeedRangeColour = Color.Purple;
    private Color WithinSpeedRangeColour = Color.Lime;
    private Color LowerSpeedRangeColour = Color.Aqua;

    public SpeedSummaryPalette() : base(null)
    {
      // ...
    }
    public Color ChooseColour(MachineSpeedExtendedRecord measuredSpeed, MachineSpeedExtendedRecord targetSpeed)
    {
      if (targetSpeed.Max == CellPass.NullMachineSpeed)
        return Color.Empty;
      else
      {
        if (measuredSpeed.Max > targetSpeed.Max)
          return OverSpeedRangeColour;
        else if (measuredSpeed.Min < targetSpeed.Min && measuredSpeed.Max < targetSpeed.Min)
          return LowerSpeedRangeColour;
        else
          return WithinSpeedRangeColour;
      }
    }

  }
}
