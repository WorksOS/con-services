using System;
using System.Drawing;
using VSS.TRex.Rendering.Palettes.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  public abstract class ProductionPVMDisplayer: IDisposable
  {
    public MapSurface MapView;

    // Various quantities useful when displaying grid data
    protected double stepXIncrement;
    protected double stepYIncrement;
    protected double stepXIncrementOverTwo;
    protected double stepYIncrementOverTwo;

    // Various quantities useful when iterating across cells and drawing them
    protected int north_row, east_col;
    protected double currentNorth;
    protected double currentEast;

    protected double cellSizeX;
    protected double cellSizeY;

    // accumulatingScanLine is a flag indicating we are accumulating cells together
    // to for a scan line of cells that we will display in one hit
    private bool _accumulatingScanLine;

    // cellStripStartX and cellStripEndX record the start and end of the strip we are displaying
    private double _cellStripStartX;
    private double _cellStripEndX;

    // cellStripColour records the colour of the strip of cells we will draw
    private Color _cellStripColour;

    public abstract void SetPalette(IPlanViewPalette palette);

    public abstract IPlanViewPalette GetPalette();

    protected void DoRenderCell()
    {
      var colour = DoGetDisplayColour();

      if (colour != Color.Empty)
      {
        MapView.DrawRect(currentEast, currentNorth,
          cellSizeX, cellSizeY, true, colour);
      }
    }

    protected void DoStartRowScan() => _accumulatingScanLine = false;

    protected void DoEndRowScan()
    {
      if (_accumulatingScanLine)
        DoRenderStrip();
    }

    // DoGetDisplayColour queries the data at the current cell location and
    // determines the colour that should be displayed there. If there is no value
    // that should be displayed there (ie: it is <Null>, then the function returns
    // clNone as the colour).
    public abstract Color DoGetDisplayColour();

    private void DoRenderStrip()
    {
      if (_accumulatingScanLine && _cellStripColour != Color.Empty)
      {
        MapView.DrawRect(_cellStripStartX - stepXIncrementOverTwo,
          currentNorth - stepYIncrementOverTwo,
          (_cellStripEndX - _cellStripStartX) + stepXIncrement,
          stepYIncrement,
          true,
          _cellStripColour);

        _accumulatingScanLine = false;
      }
    }

    protected void DoAccumulateStrip()
    {
      var displayColour = DoGetDisplayColour();

      if (displayColour != Color.Empty) // There's something to draw
      {
        // Set the end of the strip to current east
        _cellStripEndX = currentEast;

        if (!_accumulatingScanLine) // We should start accumulating one
        {
          _accumulatingScanLine = true;
          _cellStripColour = displayColour;
          _cellStripStartX = currentEast;
        }
        else // ... We're already accumulating one, we might need to draw it and start again
        {
          if (_cellStripColour != displayColour)
          {
            DoRenderStrip();

            _accumulatingScanLine = true;
            _cellStripColour = displayColour;
            _cellStripStartX = currentEast;
          }
        }
      }
      else // The cell should not be drawn
      {
        if (_accumulatingScanLine) // We have accumulated something that should be drawn
          DoRenderStrip();
      }
    }

    /// <summary>
    /// Enables a displayer to advertise is it capable of rendering cell information in strips.
    /// </summary>
    /// <returns></returns>
    protected virtual bool SupportsCellStripRendering() => true;

    /// <summary>
    /// Performs iteration across a region of a single 2D array of values
    /// </summary>
    /// <param name="valueStoreCellSizeY"></param>
    /// <param name="valueStoreCellSizeX"></param>
    /// <param name="worldOriginX"></param>
    /// <param name="worldOriginY"></param>
    /// <param name="worldWidth"></param>
    /// <param name="worldHeight"></param>
    /// <param name="originX"></param>
    /// <param name="originY"></param>
    /// <param name="limitX"></param>
    /// <param name="limitY"></param>
    protected void DoIterate(double valueStoreCellSizeX, double valueStoreCellSizeY, double worldOriginX, double worldOriginY, double worldWidth, double worldHeight, int originX, int originY, int limitX, int limitY)
    {
      var drawCellStrips = SupportsCellStripRendering();

      cellSizeX = valueStoreCellSizeX; //worldWidth / (limitX - originX + 1);
      cellSizeY = valueStoreCellSizeY; //worldHeight / (limitY - originY + 1);

      stepXIncrement = cellSizeX;
      stepXIncrementOverTwo = cellSizeX / 2;

      stepYIncrement = cellSizeY;
      stepYIncrementOverTwo = cellSizeY / 2;

      north_row = originY;
      currentNorth = worldOriginY;

      for (var y = originY; y <= limitY; y++)
      {
        currentEast = worldOriginX;

        if (drawCellStrips)
          DoStartRowScan();

        east_col = originX;
        for (var x = originX; x <= limitX; x++)
        {
          if (drawCellStrips)
            DoAccumulateStrip();
          else
            DoRenderCell();

          currentEast += cellSizeX;
          east_col++;
        }

        if (drawCellStrips)
          DoEndRowScan();

        currentNorth += cellSizeY;
        north_row++;
      }
    }

    #region IDisposable Support
    private bool _disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          MapView?.Dispose();
          MapView = null;
        }

        _disposedValue = true;
      }
    }

    // Override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~ProductionPVMDisplayerBase()
    // {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      // Uncomment the following line if the finalizer is overridden above.
      // GC.SuppressFinalize(this);
    }
    #endregion
  }
}
