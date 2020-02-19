using System;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Executors.Tasks
{
  public class PVMTaskAccumulator<TC, TS> : IPVMTaskAccumulator where TS : GenericClientLeafSubGrid<TC>
  {
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

    private int _stepX, _stepY;
    private double _stepXIncrement, _stepYIncrement;
    private double _stepXIncrementOverTwo, _stepYIncrementOverTwo;

    private void InitialiseValueStore(TC nullCellValue)
    {
      ValueStore = new TC[CellsWidth, CellsHeight];

      // Initialise value store to the supplied null values to ensure colour choosing from the palette is
      // correct for values that are not populated from inbound subgrids
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
      var stepsPerPixelX = ValueStoreCellSizeX / SourceCellSize;
      var stepsPerPixelY = ValueStoreCellSizeY / SourceCellSize;

      _stepX = Math.Max(1, (int)Math.Truncate(stepsPerPixelX));
      _stepY = Math.Max(1, (int)Math.Truncate(stepsPerPixelY));

      _stepXIncrement = _stepX * SourceCellSize;
      _stepYIncrement = _stepY * SourceCellSize;

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
    /// <param name="worldX">The world coordinate width (X axis) of the value store</param>
    /// <param name="worldY">The world coordinate width (X axis) of the value store</param>
    /// <param name="originX">The default north oriented world X coordinate or the _valueStore origin</param>
    /// <param name="originY">The default north oriented world Y coordinate or the _valueStore origin</param>
    /// <param name="sourceCellSize">The (square) size of the underlying cells in the site model that is the source of rendered data</param>
    public PVMTaskAccumulator(double valueStoreCellSizeX, double valueStoreCellSizeY, int cellsWidth, int cellsHeight,
      double worldX, double worldY,
      double originX, double originY,
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
    /// Transcribes values of interest from subgrids into a contiguous collection of values
    /// </summary>
    /// <param name="subGridResponses"></param>
    /// <returns>Whether the content of subGridResponses contained a transcribable sub grid</returns>
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
        CalculateAccumulatorParameters();
        InitialiseValueStore(subGrid.NullCell());
      }

      // Calculate the world coordinate location of the origin (bottom left corner) of this sub grid
      subGridResponses[0].CalculateWorldOrigin(out var subGridWorldOriginX, out var subGridWorldOriginY);

      // Skip-Iterate through the cells assigning them to the value store

      var temp = subGridWorldOriginY / _stepYIncrement;
      var currentNorth = (Math.Truncate(temp) * _stepYIncrement) - _stepYIncrementOverTwo;
      var northRow = (int) Math.Floor((currentNorth - subGridWorldOriginY) / SourceCellSize);
      while (northRow < 0)
      {
        northRow += _stepY;
        currentNorth += _stepYIncrement;
      }

      var valueStoreY = (int)Math.Floor((currentNorth - OriginY) / ValueStoreCellSizeY);
      while (northRow < SubGridTreeConsts.SubGridTreeDimension)
      {
        if (valueStoreY >= 0 && valueStoreY < CellsHeight)
        {
          temp = subGridWorldOriginX / _stepXIncrement;
          var currentEast = (Math.Truncate(temp) * _stepXIncrement) - _stepXIncrementOverTwo;
          var eastCol = (int) Math.Floor((currentEast - subGridWorldOriginX) / SourceCellSize);

          while (eastCol < 0)
          {
            eastCol += _stepX;
            currentEast += _stepXIncrement;
          }

          var valueStoreX = (int)Math.Floor((currentEast - OriginX) / ValueStoreCellSizeX);

          while (eastCol < SubGridTreeConsts.SubGridTreeDimension)
          {
            // Transcribe the value at [east_col, north_row] in the subgrid in to the matching location in the value store
            if (valueStoreX >= 0 && valueStoreX < CellsWidth)
            {
              ValueStore[valueStoreX, valueStoreY] = cells[eastCol, northRow];
            }

            currentEast += _stepXIncrement;
            eastCol += _stepX;
            valueStoreX++;
          }
        }

        currentNorth += _stepYIncrement;
        northRow += _stepY;
        valueStoreY++;
      }

      return true;
    }
  }
}
