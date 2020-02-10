using System;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Executors.Tasks
{
  public class PVMTaskAccumulator
  {
    public readonly float[,] ValueStore;
    private readonly double sourceCellSize;
    private readonly double valueStoreCellSizeX, valueStoreCellSizeY;
    private readonly int cellsWidth, cellsHeight;
    private readonly double worldX, worldY;
    private readonly double originX, originY;
    private readonly int stepX, stepY;
    private readonly double stepXIncrement, stepYIncrement;
    private readonly double stepXIncrementOverTwo, stepYIncrementOverTwo;

    /// <summary>
    /// Constructor that instantiates intermediary value storage for the PVM rendering task
    /// </summary>
    /// <param name="sourceCellSize">The physical size of cells in the subgrids being transcribed</param>
    /// <param name="cellsWidth">The number of cells 'wide' (x ordinate) in the set of cell values requested</param>
    /// <param name="cellsHeight">The number of cells 'high' (y ordinate) in the set of cell values requested</param>
    /// <param name="worldX">The world coordinate width (X axis) of the value store</param>
    /// <param name="worldY">The world coordinate width (X axis) of the value store</param>
    /// <param name="originX">The default north oriented world X coordinate or the _valueStore origin</param>
    /// <param name="originY">The default north oriented world Y coordinate or the _valueStore origin</param>
    public PVMTaskAccumulator(double sourceCellSize,
      int cellsWidth, int cellsHeight,
      double worldX, double worldY, 
      double originX, double originY)
    {
      this.sourceCellSize = sourceCellSize;
      this.cellsWidth = cellsWidth;
      this.cellsHeight = cellsHeight;
      this.originX = originX;
      this.originY = originY;
      this.worldX = worldX;
      this.worldY = worldY;

      ValueStore = new float[this.cellsWidth, this.cellsHeight];
      valueStoreCellSizeX = this.worldX / this.cellsWidth;
      valueStoreCellSizeY = this.worldY / this.cellsHeight;

      var stepsPerPixelX = (worldX / this.cellsWidth) / sourceCellSize;
      var stepsPerPixelY = (worldY / this.cellsHeight) / sourceCellSize;

      stepX = Math.Max(1, (int)Math.Truncate(stepsPerPixelX));
      stepY = Math.Max(1, (int)Math.Truncate(stepsPerPixelY));

      stepXIncrement = stepX * sourceCellSize;
      stepYIncrement = stepY * sourceCellSize;

      stepXIncrementOverTwo = stepXIncrement / 2;
      stepYIncrementOverTwo = stepYIncrement / 2;
    }

    /// <summary>
    /// Transcribes values of interest from subgrids into a contiguous collection of values
    /// </summary>
    /// <param name="subGridResponses"></param>
    /// <returns></returns>
    public bool Transcribe(IClientLeafSubGrid[] subGridResponses)
    {
      var cells = subGridResponses?.Length == 1 ? subGridResponses[0]?.ToFloatArray() : null;

      if (cells == null)
      {
        return false;
      }

      // Calculate the world coordinate location of the origin (bottom left corner) of this sub grid
      subGridResponses[0].CalculateWorldOrigin(out var subGridWorldOriginX, out var subGridWorldOriginY);

      // Skip-Iterate through the cells assigning them to the value store

      var temp = subGridWorldOriginY / stepYIncrement;
      var currentNorth = (Math.Truncate(temp) * stepYIncrement) - stepYIncrementOverTwo;
      var northRow = (int) Math.Floor((currentNorth - subGridWorldOriginY) / sourceCellSize);
      while (northRow < 0)
      {
        northRow += stepY;
        currentNorth += stepYIncrement;
      }

      while (northRow < SubGridTreeConsts.SubGridTreeDimension)
      {
        var valueStoreY = (int) Math.Floor((currentNorth - originY) / valueStoreCellSizeY);

        temp = subGridWorldOriginX / stepXIncrement;
        var currentEast = (Math.Truncate(temp) * stepXIncrement) - stepXIncrementOverTwo;
        var eastCol = (int) Math.Floor((currentEast - subGridWorldOriginX) / sourceCellSize);

        while (eastCol < 0)
        {
          eastCol += stepX;
          currentEast += stepXIncrement;
        }

        while (eastCol < SubGridTreeConsts.SubGridTreeDimension)
        {
          // Transcribe the value at [east_col, north_row] in the subgrid in to the matching location in the value store
          var valueStoreX = (int) Math.Floor((currentEast - originX) / valueStoreCellSizeX);
          ValueStore[valueStoreX, valueStoreY] = cells[eastCol, northRow];

          currentEast += stepXIncrement;
          eastCol += stepX;
        }

        currentNorth += stepYIncrement;
        northRow += stepY;
      }

      return true;
    }
  }
}
