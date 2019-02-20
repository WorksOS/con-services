using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using Draw = System.Drawing;

namespace VSS.TRex.Rendering.Displayers
{
  public abstract class ProductionPVMDisplayerBase
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ProductionPVMDisplayerBase>();

    private const int MAX_STEP_SIZE = 10000;

    /// <summary>
    /// Production data holder.
    /// </summary>
    protected ISubGrid SubGrid;

    // Various quantities useful when displaying a sub grid full of grid data
    private int StepX;
    private int StepY;

    //protected int StepXWorld;
    //protected int StepYWorld;

    private double StepXIncrement;
    private double StepYIncrement;
    private double StepXIncrementOverTwo;
    private double StepYIncrementOverTwo;

    // Various quantities useful when iterating across cells in a sub grid and drawing them

    protected int north_row, east_col;
    private double CurrentNorth;
    private double CurrentEast;

    private double _CellSize;
    //protected double _OneThirdCellSize;
    //protected double _HalfCellSize;
    //protected double _TwoThirdsCellSize;

    // AccumulatingScanLine is a flag indicating we are accumulating cells together
    // to for a scan line of cells that we will display in one hit
    private bool AccumulatingScanLine;

    // CellStripStartX and CellStripEndX record the start and end of the strip we are displaying
    private double CellStripStartX;
    private double CellStripEndX;

    // CellStripColour records the colour of the strip of cells we will draw
    private Draw.Color CellStripColour;

    // OriginX/y and LimitX/Y denote the extents of the physical world area covered by
    // the display context being drawn into
    // protected double OriginX, OriginY, LimitX, LimitY;

    // ICOptions is a transient reference an IC options object to be used while rendering
    // ICOptions : TSVOICOptions;

    private bool FDisplayParametersCalculated;

    private void CalculateDisplayParameters()
    {
      // Set the cell size for displaying the grid. If we will be processing
      // representative grids then set _CellSize to be the size of a leaf
      // sub grid in the sub grid tree
      //_OneThirdCellSize = _CellSize * (1 / 3.0);
      //_HalfCellSize = _CellSize / 2.0;
      //_TwoThirdsCellSize = _CellSize * (2 / 3.0);

      double StepsPerPixelX = MapView.XPixelSize / _CellSize;
      double StepsPerPixelY = MapView.YPixelSize / _CellSize;

      StepX = Math.Min(MAX_STEP_SIZE, Math.Max(1, (int)Math.Truncate(StepsPerPixelX)));
      StepY = Math.Min(MAX_STEP_SIZE, Math.Max(1, (int)Math.Truncate(StepsPerPixelY)));

      StepXIncrement = StepX * _CellSize;
      StepYIncrement = StepY * _CellSize;

      StepXIncrementOverTwo = StepXIncrement / 2;
      StepYIncrementOverTwo = StepYIncrement / 2;
    }

    protected virtual bool DoRenderSubGrid<T>(ISubGrid subGrid)
    {
      if (!(subGrid is T grid))
        return false;

      SubGrid = (ISubGrid)grid;

      bool DrawCellStrips;

      // Draw the cells in the grid in stripes, starting from the southern most
      // row in the grid and progressing from the western end to the eastern end
      // (ie: bottom to top, left to right)

      // See if this display supports cell strip rendering

      DrawCellStrips = SupportsCellStripRendering();

      // Calculate the world coordinate location of the origin (bottom left corner)
      // of this sub grid
      SubGrid.CalculateWorldOrigin(out double SubGridWorldOriginX, out double SubGridWorldOriginY);

      // Draw the background of the sub grid if a pixel is less than 1 meter is width
      // if (MapView.XPixelSize < 1.0)
      //    MapView.DrawRect(SubGridWorldOriginX, SubGridWorldOriginY + _CellSize * 32, _CellSize * 32, _CellSize * 32, true,
      //    ((SubGrid.OriginX >> 5) + (SubGrid.OriginY >> 5)) % 2 == 0 ? Draw.Color.Black : Draw.Color.Blue);

      // Skip-Iterate through the cells drawing them in strips

      double Temp = SubGridWorldOriginY / StepYIncrement;
      CurrentNorth = (Math.Truncate(Temp) * StepYIncrement) - StepYIncrementOverTwo;
      north_row = (int)Math.Floor((CurrentNorth - SubGridWorldOriginY) / _CellSize);

      while (north_row < 0)
      {
        north_row += StepY;
        CurrentNorth += StepYIncrement;
      }

      while (north_row < SubGridTreeConsts.SubGridTreeDimension)
      {
        Temp = SubGridWorldOriginX / StepXIncrement;
        CurrentEast = (Math.Truncate(Temp) * StepXIncrement) + StepXIncrementOverTwo;
        east_col = (int)Math.Floor((CurrentEast - SubGridWorldOriginX) / _CellSize);

        while (east_col < 0)
        {
          east_col += StepX;
          CurrentEast += StepXIncrement;
        }

        if (DrawCellStrips)
          DoStartRowScan();

        while (east_col < SubGridTreeConsts.SubGridTreeDimension)
        {
          if (DrawCellStrips)
            DoAccumulateStrip();
          else
            DoRenderCell();

          CurrentEast += StepXIncrement;
          east_col += StepX;
        }

        if (DrawCellStrips)
          DoEndRowScan();

        CurrentNorth += StepYIncrement;
        north_row += StepY;
      }

      return true;
    }

    protected virtual void DoRenderCell()
    {
      Draw.Color Colour = DoGetDisplayColour();

      if (Colour != Draw.Color.Empty)
        MapView.DrawRect(CurrentEast, CurrentNorth,
                         _CellSize, _CellSize, true, Colour);
    }

    // SupportsCellStripRendering enables a displayer to advertise is it capable
    // of rendering cell information in strips
    protected virtual bool SupportsCellStripRendering() => false;

    // DoGetDisplayColour queries the data at the current cell location and
    // determines the colour that should be displayed there. If there is no value
    // that should be displayed there (ie: it is <Null>, then the function returns
    // clNone as the colour).
    protected abstract Draw.Color DoGetDisplayColour();

    private void DoStartRowScan() => AccumulatingScanLine = false;

    private void DoEndRowScan()
    {
      if (AccumulatingScanLine)
        DoRenderStrip();
    }

    private void DoAccumulateStrip()
    {
      Draw.Color DisplayColour = DoGetDisplayColour();

      if (DisplayColour != Draw.Color.Empty) // There's something to draw
      {
        // Set the end of the strip to current east
        CellStripEndX = CurrentEast;

        if (!AccumulatingScanLine) // We should start accumulating one
        {
          AccumulatingScanLine = true;
          CellStripColour = DisplayColour;
          CellStripStartX = CurrentEast;
        }
        else // ... We're already accumulating one, we might need to draw it and start again
        {
          if (CellStripColour != DisplayColour)
          {
            DoRenderStrip();

            AccumulatingScanLine = true;
            CellStripColour = DisplayColour;
            CellStripStartX = CurrentEast;
          }
        }
      }
      else // The cell should not be drawn
      {
        if (AccumulatingScanLine) // We have accumulated something that should be drawn
          DoRenderStrip();
      }
    }

    private void DoRenderStrip()
    {
      if (!AccumulatingScanLine)
        return;

      if (CellStripColour == Draw.Color.Empty)
        return;

      MapView.DrawRect(CellStripStartX - StepXIncrementOverTwo,
                       CurrentNorth - StepYIncrementOverTwo,
                       (CellStripEndX - CellStripStartX) + StepXIncrement,
                       StepYIncrement,
                       true,
                       CellStripColour);

      AccumulatingScanLine = false;
    }

    // public double CellSize { get => _CellSize; set => _CellSize = value; }

    // property ICOptions : TSVOICOptions read FICOptions write FICOptions;

    public MapSurface MapView { get; set; }

    public bool HasRenderedSubGrid { get; set; }

    public ProductionPVMDisplayerBase()
    {
    }

    public bool RenderSubGrid(SubGridTreeLeafSubGridBaseResult subGridResult)
    {
      if (!(subGridResult.SubGrid is IClientLeafSubGrid))
      {
        Log.LogError($"Sub grid type {subGridResult.SubGrid} does not implement IClientLeafSubGrid");
        return false;
      }

      if (!FDisplayParametersCalculated)
      {
        _CellSize = subGridResult.SubGrid.CellSize;
        CalculateDisplayParameters();
        FDisplayParametersCalculated = true;
      }

      HasRenderedSubGrid = true;

      return DoRenderSubGrid<IClientLeafSubGrid>(subGridResult.SubGrid);
    }

    public bool RenderSubGrid(IClientLeafSubGrid ClientSubGrid)
    {
      if (ClientSubGrid == null)
        return true;

      if (!FDisplayParametersCalculated)
      {
        _CellSize = ClientSubGrid.CellSize;
        CalculateDisplayParameters();
        FDisplayParametersCalculated = true;
      }

      HasRenderedSubGrid = true;

      return DoRenderSubGrid<IClientLeafSubGrid>(ClientSubGrid);
    }
  }
}
