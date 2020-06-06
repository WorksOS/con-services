using System;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Executors.Tasks
{
  public class PVMTaskAccumulator<TC, TS> : IPVMTaskAccumulator where TS : GenericClientLeafSubGrid<TC>
  {
    private readonly object _lockObj = new object();

    public TC[,] ValueStore;

    /// <summary>
    /// The cell size of the cells contained in the accumulated sub grids
    /// </summary>
    public readonly double SourceCellSize;

    /// <summary>
    /// The X and Y dimensions of the cells in the value accumulator array
    /// </summary>
    public readonly double ValueStoreCellSizeX, ValueStoreCellSizeY;

    /// <summary>
    /// The number of cells wide and high in the accumulator array
    /// </summary>
    public readonly int CellsWidth, CellsHeight;

    public readonly double WorldX;
    public readonly double WorldY;
    public readonly double OriginX;
    public readonly double OriginY;

    private double _stepXIncrement, _stepYIncrement;
    private double _stepXIncrementOverTwo, _stepYIncrementOverTwo;

    private void InitialiseValueStore(TC nullCellValue)
    {
      ValueStore = new TC[CellsWidth, CellsHeight];

      // Initialise value store to the supplied null values to ensure color choosing from the palette is
      // correct for values that are not populated from inbound sub grids
      for (var i = 0; i < CellsWidth; i++)
      {
        for (var j = 0; j < CellsHeight; j++)
        {
          ValueStore[i, j] = nullCellValue;
        }
      }
    }

    private void CalculateAccumulatorParameters()
    {
      _stepXIncrement = ValueStoreCellSizeX;
      _stepYIncrement = ValueStoreCellSizeY;

      _stepXIncrementOverTwo = _stepXIncrement / 2;
      _stepYIncrementOverTwo = _stepYIncrement / 2;
    }

    /// <summary>
    /// Constructor that instantiates intermediary value storage for the PVM rendering task
    /// </summary>
    /// <param name="valueStoreCellSizeX">The world X dimension size of cells in the value store</param>
    /// <param name="valueStoreCellSizeY">The world X dimension size of cells in the value store</param>
    /// <param name="cellsWidth">The number of cells 'wide' (x ordinate) in the set of cell values requested</param>
    /// <param name="cellsHeight">The number of cells 'high' (y ordinate) in the set of cell values requested</param>
    /// <param name="originX">The default north oriented world X coordinate or the _valueStore origin</param>
    /// <param name="originY">The default north oriented world Y coordinate or the _valueStore origin</param>
    /// <param name="worldX">The world coordinate width (X axis) of the value store</param>
    /// <param name="worldY">The world coordinate width (X axis) of the value store</param>
    /// <param name="sourceCellSize">The (square) size of the underlying cells in the site model that is the source of rendered data</param>
    public PVMTaskAccumulator(double valueStoreCellSizeX, double valueStoreCellSizeY, int cellsWidth, int cellsHeight,
      double originX, double originY,
      double worldX, double worldY,
      double sourceCellSize)
    {
      ValueStoreCellSizeX = valueStoreCellSizeX;
      ValueStoreCellSizeY = valueStoreCellSizeY;
      CellsWidth = cellsWidth;
      CellsHeight = cellsHeight;
      OriginX = originX;
      OriginY = originY;
      WorldX = worldX;
      WorldY = worldY;
      SourceCellSize = sourceCellSize;
    }

    /// <summary>
    /// Transcribes values of interest from sub grids into a contiguous collection of values
    /// </summary>
    /// <param name="subGridResponses"></param>
    /// <returns>Whether the content of subGridResponses contains a sub grid to be transcribed</returns>
    public bool Transcribe(IClientLeafSubGrid[] subGridResponses)
    {
      var subGrid = subGridResponses?.Length == 1 ? subGridResponses[0] as TS : null;
      var cells = subGrid?.Cells;

      if (cells == null)
      {
        return false;
      }

      if (ValueStore == null)
      {
        lock (_lockObj)
        {
          if (ValueStore == null)
          {
            CalculateAccumulatorParameters();
            InitialiseValueStore(subGrid.NullCell());
          }
        }
      }

      // Calculate the world coordinate location of the origin (bottom left corner) of this sub grid
      subGridResponses[0].CalculateWorldOrigin(out var subGridWorldOriginX, out var subGridWorldOriginY);
      var subGridWorldLimitX = subGridWorldOriginX + SubGridTreeConsts.SubGridTreeDimension * SourceCellSize;
      var subGridWorldLimitY = subGridWorldOriginY + SubGridTreeConsts.SubGridTreeDimension * SourceCellSize;

      // Skip-Iterate through the cells assigning them to the value store

      var originEast = (Math.Truncate(subGridWorldOriginX / _stepXIncrement) * _stepXIncrement) - _stepXIncrementOverTwo;
      while (originEast < subGridWorldOriginX)
      {
        originEast += _stepXIncrement;
      }

      var currentNorth = (Math.Truncate(subGridWorldOriginY / _stepYIncrement) * _stepYIncrement) - _stepYIncrementOverTwo;
      while (currentNorth < subGridWorldOriginY)
      {
        currentNorth += _stepYIncrement;
      }

      var valueStoreY = (int)Math.Floor((currentNorth - OriginY) / _stepYIncrement);

      while (currentNorth < subGridWorldLimitY)
      {
        if (valueStoreY >= 0 && valueStoreY < CellsHeight)
        {
          var northRow = (int)Math.Floor((currentNorth - subGridWorldOriginY) / SourceCellSize);
          var currentEast = originEast;
          var valueStoreX = (int)Math.Floor((currentEast - OriginX) / _stepXIncrement);

          while (currentEast < subGridWorldLimitX)
          {
            // Transcribe the value at [eastCol, northRow] in the sub grid into the matching location in the value store
            if (valueStoreX >= 0 && valueStoreX < CellsWidth)
            {
              var eastCol = (int)Math.Floor((currentEast - subGridWorldOriginX) / SourceCellSize);
              ValueStore[valueStoreX, valueStoreY] = cells[eastCol, northRow];
            }

            currentEast += _stepXIncrement;
            valueStoreX++;
          }
        }

        currentNorth += _stepYIncrement;
        valueStoreY++;
      }

      return true;
    }
  }
}
