using System.Drawing;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for Pass Count summary information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_PassCountSummary : PVMDisplayerBase
  {
    /// <summary>
    /// The flag is to indicate whether or not the machine Pass Count target range to be user overrides.
    /// </summary>
    private bool UseMachineTargetPass = false;

    /// <summary>
    /// Pass Count target range.
    /// </summary>
    public PassCountRangeRecord TargetPassCountRange = new PassCountRangeRecord(3, 5);

    /// <summary>
    /// Renders Pass Count summary data as tiles. 
    /// </summary>
    /// <param name="subGrid"></param>
    /// <returns></returns>
    protected override bool DoRenderSubGrid<T>(ISubGrid subGrid)
    {
      return base.DoRenderSubGrid<ClientPassCountLeafSubGrid>(subGrid);
    }

    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      var cellValue = ((ClientPassCountLeafSubGrid)SubGrid).Cells[east_col, north_row];

      var passTargetRange = new PassCountRangeRecord(cellValue.TargetPassCount, cellValue.TargetPassCount);

      // If we are not using the machine Pass Target value then we need to replace the
      // Pass Count Target report from the machine, with the override value specified in the options
      if (!UseMachineTargetPass)
        passTargetRange = TargetPassCountRange;

      var returnedColour = cellValue.MeasuredPassCount == CellPassConsts.NullPassCountValue || passTargetRange.Min == CellPassConsts.NullPassCountValue || passTargetRange.Max == CellPassConsts.NullPassCountValue
        ? Color.Empty
        : ((PassCountSummaryPalette)Palette).ChooseColour(cellValue.MeasuredPassCount, passTargetRange);

      return returnedColour;
    }
  }
}
