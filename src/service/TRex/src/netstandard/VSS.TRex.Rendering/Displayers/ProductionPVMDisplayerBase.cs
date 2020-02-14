﻿using System;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using System.Drawing;
using VSS.TRex.Rendering.Palettes.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  public abstract class ProductionPVMDisplayerBase<P, T> : ProductionPVMDisplayerBaseBase
    where P : class, IPlanViewPalette 
    where T : class, IClientLeafSubGrid
  {
    //private static readonly ILogger Log = Logging.Logger.CreateLogger<ProductionPVMDisplayerBase>();

    private const int MAX_STEP_SIZE = 10000;

    private T _subGrid;

    protected virtual void SetSubGrid(ISubGrid value)
    {
      _subGrid = value as T;
    }

    /// <summary>
    /// Production data holder.
    /// </summary>
    protected T SubGrid { get => _subGrid; set => SetSubGrid(value); }

    public P Palette;

    /// <summary>
    /// A palette set accessor for use when a palette is only known bbyb its IPlanViewPalette interface
    /// </summary>
    /// <param name="palette"></param>
    public override void SetPalette(IPlanViewPalette palette)
    {
      Palette = palette as P;
    }

    /// <summary>
    /// A palette get accessor for use when only the IPlanViewPalette is knowable in the accessing context
    /// </summary>
    /// <returns></returns>
    public override IPlanViewPalette GetPalette() => Palette;

    public MapSurface GetMapView() => MapView;

    /// <summary>
    /// Enables a displayer to advertise is it capable of rendering cell information in strips.
    /// </summary>
    /// <returns></returns>
    protected virtual bool SupportsCellStripRendering() => true;

    // Various quantities useful when displaying a sub grid full of grid data
    private int stepX;
    private int stepY;

    //protected int stepXWorld; 
    //protected int stepYWorld;

    private double stepXIncrement;
    private double stepYIncrement;
    private double stepXIncrementOverTwo;
    private double stepYIncrementOverTwo;

    // Various quantities useful when iterating across cells in a sub grid and drawing them

    protected int north_row, east_col;
    private double currentNorth;
    private double currentEast;

    private double cellSize;
    //protected double oneThirdCellSize;
    //protected double halfCellSize;
    //protected double twoThirdsCellSize;

    // accumulatingScanLine is a flag indicating we are accumulating cells together
    // to for a scan line of cells that we will display in one hit
    private bool accumulatingScanLine;

    // cellStripStartX and cellStripEndX record the start and end of the strip we are displaying
    private double cellStripStartX;
    private double cellStripEndX;

    // cellStripColour records the colour of the strip of cells we will draw
    private Color cellStripColour;

    // OriginX/y and LimitX/Y denote the extents of the physical world area covered by
    // the display context being drawn into
    // protected double OriginX, OriginY, LimitX, LimitY;

    // ICOptions is a transient reference an IC options object to be used while rendering
    // ICOptions : TSVOICOptions;

    private bool displayParametersCalculated;

    private void CalculateDisplayParameters()
    {
      // Set the cell size for displaying the grid. If we will be processing
      // representative grids then set cellSize to be the size of a leaf
      // sub grid in the sub grid tree
      // oneThirdCellSize = cellSize * (1 / 3.0);
      // halfCellSize = cellSize / 2.0;
      // twoThirdsCellSize = cellSize * (2 / 3.0);

      double StepsPerPixelX = MapView.XPixelSize / cellSize;
      double StepsPerPixelY = MapView.YPixelSize / cellSize;

      stepX = Math.Min(MAX_STEP_SIZE, Math.Max(1, (int)Math.Truncate(StepsPerPixelX)));
      stepY = Math.Min(MAX_STEP_SIZE, Math.Max(1, (int)Math.Truncate(StepsPerPixelY)));

      stepXIncrement = stepX * cellSize;
      stepYIncrement = stepY * cellSize;

      stepXIncrementOverTwo = stepXIncrement / 2;
      stepYIncrementOverTwo = stepYIncrement / 2;
    }

    protected virtual bool DoRenderSubGrid(T subGrid)
    {
      _subGrid = subGrid;

      bool DrawCellStrips;

      // Draw the cells in the grid in stripes, starting from the southern most
      // row in the grid and progressing from the western end to the eastern end
      // (ie: bottom to top, left to right)

      // See if this display supports cell strip rendering

      DrawCellStrips = SupportsCellStripRendering();

      // Calculate the world coordinate location of the origin (bottom left corner)
      // of this sub grid
      subGrid.CalculateWorldOrigin(out double subGridWorldOriginX, out double subGridWorldOriginY);

      // Draw the background of the sub grid if a pixel is less than 1 meter is width
      // if (MapView.XPixelSize < 1.0)
      //    MapView.DrawRect(SubGridWorldOriginX, SubGridWorldOriginY + cellSize * 32, cellSize * 32, cellSize * 32, true,
      //    ((SubGrid.OriginX >> 5) + (SubGrid.OriginY >> 5)) % 2 == 0 ? Color.Black : Color.Blue);

      // Skip-Iterate through the cells drawing them in strips

      double temp = subGridWorldOriginY / stepYIncrement;
      currentNorth = (Math.Truncate(temp) * stepYIncrement) - stepYIncrementOverTwo;
      north_row = (int)Math.Floor((currentNorth - subGridWorldOriginY) / cellSize);

      while (north_row < 0)
      {
        north_row += stepY;
        currentNorth += stepYIncrement;
      }

      while (north_row < SubGridTreeConsts.SubGridTreeDimension)
      {
        temp = subGridWorldOriginX / stepXIncrement;
        currentEast = (Math.Truncate(temp) * stepXIncrement) - stepXIncrementOverTwo;
        east_col = (int)Math.Floor((currentEast - subGridWorldOriginX) / cellSize);

        while (east_col < 0)
        {
          east_col += stepX;
          currentEast += stepXIncrement;
        }

        if (DrawCellStrips)
          DoStartRowScan();

        while (east_col < SubGridTreeConsts.SubGridTreeDimension)
        {
          if (DrawCellStrips)
            DoAccumulateStrip();
          else
            DoRenderCell();

          currentEast += stepXIncrement;
          east_col += stepX;
        }

        if (DrawCellStrips)
          DoEndRowScan();

        currentNorth += stepYIncrement;
        north_row += stepY;
      }

      return true;
    }

    private void DoRenderCell()
    {
      Color colour = DoGetDisplayColour();

      if (colour != Color.Empty)
        MapView.DrawRect(currentEast, currentNorth,
                         cellSize, cellSize, true, colour);
    }

    // DoGetDisplayColour queries the data at the current cell location and
    // determines the colour that should be displayed there. If there is no value
    // that should be displayed there (ie: it is <Null>, then the function returns
    // clNone as the colour).
    public abstract Color DoGetDisplayColour();

    private void DoStartRowScan() => accumulatingScanLine = false;

    private void DoEndRowScan()
    {
      if (accumulatingScanLine)
        DoRenderStrip();
    }

    private void DoAccumulateStrip()
    {
      Color displayColour = DoGetDisplayColour();

      if (displayColour != Color.Empty) // There's something to draw
      {
        // Set the end of the strip to current east
        cellStripEndX = currentEast;

        if (!accumulatingScanLine) // We should start accumulating one
        {
          accumulatingScanLine = true;
          cellStripColour = displayColour;
          cellStripStartX = currentEast;
        }
        else // ... We're already accumulating one, we might need to draw it and start again
        {
          if (cellStripColour != displayColour)
          {
            DoRenderStrip();

            accumulatingScanLine = true;
            cellStripColour = displayColour;
            cellStripStartX = currentEast;
          }
        }
      }
      else // The cell should not be drawn
      {
        if (accumulatingScanLine) // We have accumulated something that should be drawn
          DoRenderStrip();
      }
    }

    private void DoRenderStrip()
    {
      if (accumulatingScanLine && cellStripColour != Color.Empty)
      {
        MapView.DrawRect(cellStripStartX - stepXIncrementOverTwo,
          currentNorth - stepYIncrementOverTwo,
          (cellStripEndX - cellStripStartX) + stepXIncrement,
          stepYIncrement,
          true,
          cellStripColour);

        accumulatingScanLine = false;
      }
    }

    // property ICOptions : TSVOICOptions read FICOptions write FICOptions;

    public bool HasRenderedSubGrid { get; set; }

    public ProductionPVMDisplayerBase()
    {
    }

    public override bool RenderSubGrid(IClientLeafSubGrid clientSubGrid)
    {
      if (clientSubGrid != null)
      {
        if (clientSubGrid is T _clientSubGrid)
        {
          if (!displayParametersCalculated)
          {
            cellSize = _clientSubGrid.CellSize;
            CalculateDisplayParameters();
            displayParametersCalculated = true;
          }

          HasRenderedSubGrid = true;

          return DoRenderSubGrid(_clientSubGrid);
        }
      }

      return false;
    }
  }
}
