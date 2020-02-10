using System;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Rendering.Palettes.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  public abstract class ProductionPVMDisplayerBase<TP, TS> : ProductionPVMDisplayerBaseBase
    where TP : class, IPlanViewPalette
    where TS : class, IClientLeafSubGrid
  {
    //private static readonly ILogger Log = Logging.Logger.CreateLogger<ProductionPVMDisplayerBase>();

    private TS _subGrid;

    protected virtual void SetSubGrid(ISubGrid value)
    {
      _subGrid = value as TS;
    }

    /// <summary>
    /// Production data holder.
    /// </summary>
    protected TS SubGrid
    {
      get => _subGrid;
      set => SetSubGrid(value);
    }

    public TP Palette;

    /// <summary>
    /// A palette set accessor for use when a palette is only known bbyb its IPlanViewPalette interface
    /// </summary>
    /// <param name="palette"></param>
    public override void SetPalette(IPlanViewPalette palette)
    {
      Palette = palette as TP;
    }

    /// <summary>
    /// A palette get accessor for use when only the IPlanViewPalette is knowable in the accessing context
    /// </summary>
    /// <returns></returns>
    public override IPlanViewPalette GetPalette() => Palette;

    public MapSurface GetMapView() => MapView;

    // OriginX/y and LimitX/Y denote the extents of the physical world area covered by
    // the display context being drawn into
    // protected double OriginX, OriginY, LimitX, LimitY;

    // ICOptions is a transient reference an IC options object to be used while rendering
    // ICOptions : TSVOICOptions;

    private bool _displayParametersCalculated;

    private void CalculateDisplayParameters()
    {
      // Set the cell size for displaying the grid. If we will be processing
      // representative grids then set cellSize to be the size of a leaf
      // sub grid in the sub grid tree

      var stepsPerPixelX = MapView.XPixelSize / cellSizeX;
      var stepsPerPixelY = MapView.YPixelSize / cellSizeY;

      stepX = Math.Min(MAX_STEP_SIZE, Math.Max(1, (int) Math.Truncate(stepsPerPixelX)));
      stepY = Math.Min(MAX_STEP_SIZE, Math.Max(1, (int) Math.Truncate(stepsPerPixelY)));

      stepXIncrement = stepX * cellSizeX;
      stepYIncrement = stepY * cellSizeY;

      stepXIncrementOverTwo = stepXIncrement / 2;
      stepYIncrementOverTwo = stepYIncrement / 2;
    }

    protected virtual bool DoRenderSubGrid(TS subGrid)
    {
      _subGrid = subGrid;

      // Draw the cells in the grid in stripes, starting from the southern most
      // row in the grid and progressing from the western end to the eastern end
      // (ie: bottom to top, left to right)

      // Calculate the world coordinate location of the origin (bottom left corner) of this sub grid
      subGrid.CalculateWorldOrigin(out var subGridWorldOriginX, out var subGridWorldOriginY);

      DoSkipIterate(subGridWorldOriginX, subGridWorldOriginY, SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension);

      return true;
    }

    public bool HasRenderedSubGrid { get; set; }

    public /*override */ bool RenderSubGrid(IClientLeafSubGrid clientSubGrid)
    {
      throw new NotImplementedException();
      /*
      if (clientSubGrid != null && clientSubGrid is TS subGrid)
      {
        if (!_displayParametersCalculated)
        {
          cellSizeX = subGrid.CellSize;
          cellSizeY = subGrid.CellSize;
          CalculateDisplayParameters();
          _displayParametersCalculated = true;
        }

        HasRenderedSubGrid = true;

        return DoRenderSubGrid(subGrid);
      }
      */
      return false;
    }
  }
}
