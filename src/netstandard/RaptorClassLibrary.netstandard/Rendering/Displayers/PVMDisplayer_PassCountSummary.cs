using System.Drawing;
using VSS.TRex.Cells;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for Pass Count information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_PassCountSummary : PVMDisplayerBase
  {
    private const bool UseMachineTargetPass = false;
    private PassCountRangeRecord TargetPassCountRange = new PassCountRangeRecord(5, 10);

    private ClientPassCountLeafSubGrid SubGrid;

    private Color AbovePassTargetRangeColour;
    private Color WithinPassTargetRangeColour;
    private Color BelowPassTargetRangeColour;

    public PVMDisplayer_PassCountSummary()
    {
      ((PassCountPalette) Palette).InitSummatyColors(out AbovePassTargetRangeColour, out WithinPassTargetRangeColour, out BelowPassTargetRangeColour);
    }

    protected override bool DoRenderSubGrid(ISubGrid subGrid)
    {
      if (subGrid is ClientPassCountLeafSubGrid grid)
      {
        SubGrid = grid;
        return base.DoRenderSubGrid(SubGrid);
      }

      return false;
    }

    protected override bool SupportsCellStripRendering() => true;

    protected override Color DoGetDisplayColour()
    {
      var cellValue = SubGrid.Cells[east_col, north_row];

      if (cellValue.MeasuredPassCount == CellPass.NullPassCountValue)
        return Color.Empty;

      var passTargetRange = new PassCountRangeRecord(cellValue.TargetPassCount, cellValue.TargetPassCount);

      // If we are not using the machine Pass Target value then we need to replace the
      // Pass Count Target report from the machine, with the override value specified in the options
      if (UseMachineTargetPass)
        passTargetRange = TargetPassCountRange;

      if (cellValue.MeasuredPassCount < passTargetRange.Min)
        return BelowPassTargetRangeColour;

      if (cellValue.MeasuredPassCount > passTargetRange.Max)
        return AbovePassTargetRangeColour;
      
      return WithinPassTargetRangeColour;
    }
  }
}
