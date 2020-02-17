using System;
using System.Drawing;
using VSS.TRex.DataSmoothing;
using VSS.TRex.Rendering.Palettes.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  public abstract class ProductionPVMDisplayer: IDisposable
  {
    protected const int MAX_STEP_SIZE = 10000;

    public MapSurface MapView;

    // Various quantities useful when displaying grid data
    protected int stepX;
    protected int stepY;

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

    protected void DoSkipIterate(double worldOriginY, double worldOriginX, int dimensionX, int dimensionY)
    {
      var drawCellStrips = SupportsCellStripRendering();

      // Skip-Iterate through the cells drawing them in strips

      var temp = worldOriginY / stepYIncrement;
      currentNorth = (Math.Truncate(temp) * stepYIncrement) - stepYIncrementOverTwo;
      north_row = (int)Math.Floor((currentNorth - worldOriginY) / cellSizeY);

      while (north_row < 0)
      {
        north_row += stepY;
        currentNorth += stepYIncrement;
      }

      while (north_row < dimensionY)
      {
        temp = worldOriginX / stepXIncrement;
        currentEast = (Math.Truncate(temp) * stepXIncrement) - stepXIncrementOverTwo;
        east_col = (int)Math.Floor((currentEast - worldOriginX) / cellSizeX);

        while (east_col < 0)
        {
          east_col += stepX;
          currentEast += stepXIncrement;
        }

        if (drawCellStrips)
          DoStartRowScan();

        while (east_col < dimensionX)
        {
          if (drawCellStrips)
            DoAccumulateStrip();
          else
            DoRenderCell();

          currentEast += stepXIncrement;
          east_col += stepX;
        }

        if (drawCellStrips)
          DoEndRowScan();

        currentNorth += stepYIncrement;
        north_row += stepY;
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
