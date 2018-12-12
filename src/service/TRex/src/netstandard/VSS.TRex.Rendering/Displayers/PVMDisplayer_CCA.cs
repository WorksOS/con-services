using System;
using System.ComponentModel;
using System.Linq;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using Draw = System.Drawing;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for machine speed information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_CCA : PVMDisplayerBase
  {
    /// <summary>
    /// CCA data holder.
    /// </summary>
    private ClientCCALeafSubGrid SubGrid;

    public PVMDisplayer_CCA(DisplayMode displayMode) : base(displayMode)
    {
      if (DisplayMode != DisplayMode.CCA && DisplayMode != DisplayMode.CCASummary)
        throw new InvalidEnumArgumentException("Unsupported DisplayMode type.");
    }

    /// <summary>
    /// Renders CCA summary data as tiles. 
    /// </summary>
    /// <param name="subGrid"></param>
    /// <returns></returns>
    protected override bool DoRenderSubGrid(ISubGrid subGrid)
    {
      if (subGrid is ClientCCALeafSubGrid grid)
      {
        SubGrid = grid;
        return base.DoRenderSubGrid(SubGrid);
      }

      return false;
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
      const byte HALF_PASS_FACTOR = 2;
      const byte UNDERCOMPACTED_COLOR_IDX = 0;
      const byte COMPACTED_COLOR_IDX = 1;
      const byte OVERCOMPACTED_COLOR_IDX = 2;

      var cellValue = SubGrid.Cells[east_col, north_row];

      if (cellValue.MeasuredCCA == CellPassConsts.NullCCA)
        return Draw.Color.Empty;

      var ccaPalette = (CCAPalette)Palette;

      if (DisplayMode == DisplayMode.CCA)
      {
        var ccaValue = cellValue.MeasuredCCA / HALF_PASS_FACTOR;

        if (ccaValue <= ccaPalette.PaletteTransitions.Length - 1)
          return ccaPalette.PaletteTransitions[ccaValue].Color;

        if (ccaValue >= CellPassConsts.THICK_LIFT_CCA_VALUE / HALF_PASS_FACTOR)
          return Draw.Color.Empty;

        return ccaPalette.PaletteTransitions[ccaPalette.PaletteTransitions.Length - 1].Color;
      }

      if (cellValue.TargetCCA == CellPassConsts.NullCCATarget)
        return Draw.Color.Empty;

      if (cellValue.IsUndercompacted)
        return ccaPalette.PaletteTransitions[UNDERCOMPACTED_COLOR_IDX].Color;

      if (cellValue.IsOvercompacted)
        return ccaPalette.PaletteTransitions[OVERCOMPACTED_COLOR_IDX].Color;

      return ccaPalette.PaletteTransitions[COMPACTED_COLOR_IDX].Color;
    }
  }
}
