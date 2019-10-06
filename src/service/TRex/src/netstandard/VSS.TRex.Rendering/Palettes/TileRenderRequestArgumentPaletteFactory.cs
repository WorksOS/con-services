using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Rendering.Palettes.Interfaces;

namespace VSS.TRex.Rendering.Palettes
{
  public static class TileRenderRequestArgumentPaletteFactory
  {
    public static IPlanViewPalette GetPalette(DisplayMode mode)
    {
      switch (mode)
      {
        case DisplayMode.CCA:
          return new CCAPalette();
        case DisplayMode.CCASummary:
          return new CCASummaryPalette();
        case DisplayMode.CCV:
          return new CMVPalette();
        case DisplayMode.CCVPercentSummary:
          return new CMVSummaryPalette();
        case DisplayMode.CMVChange:
          return new CMVPercentChangePalette();
        case DisplayMode.CutFill:
          return new CutFillPalette();
        case DisplayMode.Height:
          return new HeightPalette();
        case DisplayMode.MDP:
          return new MDPPalette();
        case DisplayMode.MDPPercentSummary:
          return new MDPSummaryPalette();
        case DisplayMode.PassCount:
          return new PassCountPalette();
        case DisplayMode.PassCountSummary:
          return new PassCountSummaryPalette();
        case DisplayMode.MachineSpeed:
          return new SpeedPalette();
        case DisplayMode.TargetSpeedSummary:
          return new SpeedSummaryPalette();
        case DisplayMode.TemperatureDetail:
          return new TemperaturePalette();
        case DisplayMode.TemperatureSummary:
          return new TemperatureSummaryPalette();
        default:
          throw new TRexException($"No implemented colour palette for this mode ({mode})");
      }
    }

  }
}
