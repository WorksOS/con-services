using Draw = System.Drawing;
using VSS.TRex.Cells;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Palettes
{
  public class SpeedSummaryPalette : PaletteBase
  {
    private Draw.Color OverSpeedRangeColour = Draw.Color.Purple;
    private Draw.Color WithinSpeedRangeColour = Draw.Color.Lime;
    private Draw.Color LowerSpeedRangeColour = Draw.Color.Aqua;

    public SpeedSummaryPalette() : base(null)
    {
      // ...
    }
    public Draw.Color ChooseColour(MachineSpeedExtendedRecord measuredSpeed, MachineSpeedExtendedRecord targetSpeed)
    {
      if (targetSpeed.Max == CellPass.NullMachineSpeed)
        return Draw.Color.Empty;
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
