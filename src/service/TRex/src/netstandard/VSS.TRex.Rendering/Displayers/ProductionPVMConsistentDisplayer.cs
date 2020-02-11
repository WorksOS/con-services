using System;
using VSS.TRex.Rendering.Executors.Tasks;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  public abstract class ProductionPVMConsistentDisplayer<TP, TS, TC> : ProductionPVMDisplayer, IProductionPVMConsistentDisplayer
    where TP : class, IPlanViewPalette
    where TS : GenericClientLeafSubGrid<TC>, IClientLeafSubGrid
  {
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
    /// Copy of value store provided to render from 
    /// </summary>
    public TC[,] ValueStore;

    /// <summary>
    /// A palette get accessor for use when only the IPlanViewPalette is knowable in the accessing context
    /// </summary>
    /// <returns></returns>
    public override IPlanViewPalette GetPalette() => Palette;

    // public MapSurface GetMapView() => MapView;

    /// <summary>
    /// Constructs a PVM task accumulator tailored to accumulate cell information to be rendered by this displayer
    /// Note: This intentionally does not pin the bounds of the accumulator to the bounds of the rendered map view
    /// as mapview rotation and smoothign operations may require large areas of data to be requested to supply thye final
    /// rendered outcome.
    /// </summary>
    /// <param name="cellsWidth"></param>
    /// <param name="cellsHeight"></param>
    /// <param name="worldX"></param>
    /// <param name="worldY"></param>
    /// <param name="originX"></param>
    /// <param name="originY"></param>
    /// <returns></returns>
    public IPVMTaskAccumulator GetPVMTaskAccumulator(int cellsWidth, int cellsHeight,
      double worldX, double worldY,
      double originX, double originY)
    {
      _taskAccumulator = new PVMTaskAccumulator<TC, TS>(cellsWidth, cellsHeight, worldX, worldY, originX, originY);
      return _taskAccumulator;
    }

    private PVMTaskAccumulator<TC, TS> _taskAccumulator;

    /// <summary>
    /// Pre-calculates a set of parameters for the rendering context 
    /// </summary>
    /// <param name="valueCellSizeX"></param>
    /// <param name="valueCellSizeY"></param>
    private void CalculateDisplayParameters(double valueCellSizeX, double valueCellSizeY)
    {
      var stepsPerPixelX = MapView.XPixelSize / valueCellSizeX;
      var stepsPerPixelY = MapView.YPixelSize / valueCellSizeY;

      stepX = Math.Min(MAX_STEP_SIZE, Math.Max(1, (int)Math.Truncate(stepsPerPixelX)));
      stepY = Math.Min(MAX_STEP_SIZE, Math.Max(1, (int)Math.Truncate(stepsPerPixelY)));

      stepXIncrement = stepX * valueCellSizeX;
      stepYIncrement = stepY * valueCellSizeY;

      stepXIncrementOverTwo = stepXIncrement / 2;
      stepYIncrementOverTwo = stepYIncrement / 2;
    }


    /// <summary>
    /// Performs a 'consistent' render across a 2D array of collated values from queried subgrids.
    /// Effectively this treats the passed array as if it were a subgrid of that size and renders it as
    /// such against the MapView.
    /// Essentially, this function should be called just once to render the entire set of data for a tile
    /// </summary>
    /// <returns></returns>
    public bool PerformConsistentRender()
    {
      if (_taskAccumulator == null)
      {
        throw new ArgumentException("Task accumulator not available");
      }

      ValueStore = _taskAccumulator?.ValueStore;

      if (ValueStore == null)
      {
        throw new ArgumentException("Task accumulator value store is not available");
      }

      CalculateDisplayParameters(_taskAccumulator.valueStoreCellSizeX, _taskAccumulator.valueStoreCellSizeX);

      // Draw the cells in the grid in stripes, starting from the southern most
      // row in the grid and progressing from the western end to the eastern end
      // (ie: bottom to top, left to right)

      // Skip-Iterate through the cells drawing them in strips
      DoSkipIterate(_taskAccumulator.OriginX, _taskAccumulator.OriginY, ValueStore.GetLength(0), ValueStore.GetLength(1));

      return true;
    }
  }
}
