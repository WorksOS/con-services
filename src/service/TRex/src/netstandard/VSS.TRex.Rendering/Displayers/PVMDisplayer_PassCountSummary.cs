using Draw = System.Drawing;
using VSS.TRex.Common.CellPasses;
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
    /// <summary>
    /// The flag is to indicate wehther or not the machine Pass Count target range to be user overrides.
    /// </summary>
    private const bool UseMachineTargetPass = false;

    /// <summary>
    /// Pass Count target range.
    /// </summary>
    private PassCountRangeRecord TargetPassCountRange = new PassCountRangeRecord(5, 10);

    /// <summary>
    /// The colour, which Pass Count summary data displayed in on a plan view map, where pass count values are greater than target range.
    /// </summary>
    private Draw.Color AbovePassTargetRangeColour;

    /// <summary>
    /// The colour, which Pass Count summary data displayed in on a plan view map, where pass count values are within target range.
    /// </summary>
    private Draw.Color WithinPassTargetRangeColour;

    /// <summary>
    /// The colour, which Pass Count summary data displayed in on a plan view map, where pass count values are less than target range.
    /// </summary>
    private Draw.Color BelowPassTargetRangeColour;

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public PVMDisplayer_PassCountSummary()
    {
      AbovePassTargetRangeColour = PassCountPalette.AbovePassTargetRangeColour;
      WithinPassTargetRangeColour = PassCountPalette.WithinPassTargetRangeColour;
      BelowPassTargetRangeColour = PassCountPalette.BelowPassTargetRangeColour;
    }

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
    ///  Enables a displayer to advertise is it capable of rendering cell information in strips.
    /// </summary>
    /// <returns></returns>
    protected override bool SupportsCellStripRendering() => true;

    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Draw.Color DoGetDisplayColour()
    {
      var cellValue = ((ClientPassCountLeafSubGrid)SubGrid).Cells[east_col, north_row];

      if (cellValue.MeasuredPassCount == CellPassConsts.NullPassCountValue)
        return Draw.Color.Empty;

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
