using System.Drawing;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Palettes
{
  public class SpeedSummaryPalette : PaletteBase
  {
    private Color OverSpeedRangeColour = Color.Red;
    private Color WithinSpeedRangeColour = Color.YellowGreen;
    private Color LowerSpeedRangeColour = Color.DodgerBlue;

    public SpeedSummaryPalette() : base(null)
    {
      // ...
    }

    public Color ChooseColour(MachineSpeedExtendedRecord measuredSpeed, MachineSpeedExtendedRecord targetSpeed)
    {
      var color = Color.Empty;

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
