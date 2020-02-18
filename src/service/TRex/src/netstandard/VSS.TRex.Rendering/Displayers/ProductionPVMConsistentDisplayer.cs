using System;
using VSS.TRex.DataSmoothing;
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
    /// A palette set accessor for use when a palette is only known by its IPlanViewPalette interface
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

    /// <summary>
    /// Constructs a PVM task accumulator tailored to accumulate cell information to be rendered by this displayer
    /// Note: This intentionally does not pin the bounds of the accumulator to the bounds of the rendered map view
    /// as mapview rotation and smoothing operations may require large areas of data to be requested to supply the final
    /// rendered outcome.
    /// </summary>
    /// <param name="valueStoreCellSizeX">The world X dimension size of cells in the value store</param>
    /// <param name="valueStoreCellSizeY">The world X dimension size of cells in the value store</param>
    /// <param name="cellsWidth">The number of cells in the X axis in the value store</param>
    /// <param name="cellsHeight">The number of cells in the X axis in the value store</param>
    /// <param name="worldX">The world coordinate width of the area covered by the value store</param>
    /// <param name="worldY">The world coordinate width of the area covered by the value store</param>
    /// <param name="originX">The world coordinate origin on the X axis of the area covered by the value store</param>
    /// <param name="originY">The world coordinate origin on the X axis of the area covered by the value store</param>
    /// <param name="sourceCellSize">The (square) size of the cells data elements are extracted from in the source data set</param>
    /// <returns></returns>
    public IPVMTaskAccumulator GetPVMTaskAccumulator(double valueStoreCellSizeX, double valueStoreCellSizeY,
      int cellsWidth, int cellsHeight,
      double originX, double originY, double worldX, double worldY, double sourceCellSize
      )
    {
      _taskAccumulator = new PVMTaskAccumulator<TC, TS>(valueStoreCellSizeX, valueStoreCellSizeY, cellsWidth, cellsHeight, worldX, worldY, originX, originY, sourceCellSize);
      return _taskAccumulator;
    }

    private PVMTaskAccumulator<TC, TS> _taskAccumulator;

    public IDataSmoother DataSmoother { get; set; }

 /// <summary>
    /// Performs a 'consistent' render across a 2D array of collated values from queried sub grids.
    /// Effectively this treats the passed array as if it were a subgrid of that size and renders it as
    /// such against the MapView.
    /// This function should be called just once to render the entire set of data for a tile
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
        // There is no data to render, return success
        return true;
      }

      // If there is a defined elevation smoother for ths rendering context then use it to modify the data assembled
      // for the tile to be rendered and replace the value store with the result of the smooth operation;

      ValueStore = (DataSmoother as IArrayDataSmoother<TC>)?.Smooth(ValueStore) ?? ValueStore;

      // Draw the cells in the grid in stripes, starting from the southern most
      // row in the grid and progressing from the western end to the eastern end
      // (ie: bottom to top, left to right)
      // If data smoothing has occured, inset the range of values to be drawn by the additional border size 
      // requirement of the supplied data smoother

      var insetSize = DataSmoother?.AdditionalBorderSize ?? 0;

      DoIterate(_taskAccumulator.OriginX, _taskAccumulator.OriginY,
        _taskAccumulator.WorldX, _taskAccumulator.WorldY,
        insetSize, insetSize,
        ValueStore.GetLength(0) - insetSize - 1, ValueStore.GetLength(1) - insetSize - 1);

      return true;
    }
  }
}
