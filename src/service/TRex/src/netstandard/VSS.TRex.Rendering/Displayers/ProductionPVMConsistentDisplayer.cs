using System;

namespace VSS.TRex.Rendering.Displayers
{
  public abstract class ProductionPVMConsistentDisplayer : ProductionPVMDisplayerBase
  {
    /// <summary>
    /// Performs a 'consistent' render across a 2D array of collated values from queried subgrids.
    /// Effectively this treats the passed array as if it were a subgrid of that size and renders it as
    /// such against the MapView.
    /// Essentially, this function should be called just once to render the entire set of data for a tile
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="valueStore"></param>
    /// <param name="worldOriginX"></param>
    /// <param name="worldOriginY"></param>
    /// <param name="valueCellSizeX"></param>
    /// <param name="valueCellSizeY"></param>
    /// <returns></returns>
    public bool PerformConsistentRender<T>(T[,] valueStore,
      double worldOriginX, double worldOriginY, double valueCellSizeX, double valueCellSizeY)
    {
      var xDimension = valueStore.GetLength(0);
      var yDimension = valueStore.GetLength(1);

      var stepsPerPixelX = MapView.XPixelSize / valueCellSizeX;
      var stepsPerPixelY = MapView.YPixelSize / valueCellSizeY;

      stepX = Math.Min(MAX_STEP_SIZE, Math.Max(1, (int)Math.Truncate(stepsPerPixelX)));
      stepY = Math.Min(MAX_STEP_SIZE, Math.Max(1, (int)Math.Truncate(stepsPerPixelY)));

      stepXIncrement = stepX * valueCellSizeX;
      stepYIncrement = stepY * valueCellSizeY;

      stepXIncrementOverTwo = stepXIncrement / 2;
      stepYIncrementOverTwo = stepYIncrement / 2;

      // Draw the cells in the grid in stripes, starting from the southern most
      // row in the grid and progressing from the western end to the eastern end
      // (ie: bottom to top, left to right)

      // See if this display supports cell strip rendering

      var drawCellStrips = SupportsCellStripRendering();

      // Skip-Iterate through the cells drawing them in strips

      var temp = worldOriginY / stepYIncrement;
      currentNorth = (Math.Truncate(temp) * stepYIncrement) - stepYIncrementOverTwo;
      north_row = (int)Math.Floor((currentNorth - worldOriginY) / valueCellSizeY);

      while (north_row < 0)
      {
        north_row += stepY;
        currentNorth += stepYIncrement;
      }

      while (north_row < yDimension)
      {
        temp = worldOriginX / stepXIncrement;
        currentEast = (Math.Truncate(temp) * stepXIncrement) + stepXIncrementOverTwo;
        east_col = (int)Math.Floor((currentEast - worldOriginX) / valueCellSizeX);

        while (east_col < 0)
        {
          east_col += stepX;
          currentEast += stepXIncrement;
        }

        if (drawCellStrips)
          DoStartRowScan();

        while (east_col < xDimension)
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

      return true;
    }
  }
}
