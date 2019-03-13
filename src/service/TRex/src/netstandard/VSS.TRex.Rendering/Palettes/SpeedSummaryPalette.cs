using Draw = System.Drawing;
using VSS.TRex.Common.CellPasses;
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
      var color = Draw.Color.Empty;

      if (targetSpeed.Max != CellPassConsts.NullMachineSpeed)
      {
        if (measuredSpeed.Max > targetSpeed.Max)
          color = OverSpeedRangeColour;
        else 
          color = measuredSpeed.Min < targetSpeed.Min && measuredSpeed.Max < targetSpeed.Min 
            ? LowerSpeedRangeColour
            : WithinSpeedRangeColour;
      }

      return color;
    }
  }
}
