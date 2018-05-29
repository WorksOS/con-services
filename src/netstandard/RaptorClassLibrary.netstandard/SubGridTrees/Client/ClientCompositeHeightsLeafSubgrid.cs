using System;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;

namespace VSS.TRex.SubGridTrees.Client
{
  public struct SubGridCellCompositeHeightsRecord
  {
    public float LowestHeight,
      HighestHeight,
      LastHeight,
      FirstHeight;

    public DateTime LowestHeightTime,
      HighestHeightTime,
      LastHeightTime,
      FirstHeightTime;

    public void Clear()
    {
      LowestHeight = Consts.NullHeight;
      HighestHeight= Consts.NullHeight; 
      LastHeight= Consts.NullHeight; 
      FirstHeight= Consts.NullHeight; 
      LowestHeightTime = DateTime.MinValue;
      HighestHeightTime = DateTime.MinValue;
      LastHeightTime = DateTime.MinValue;
      FirstHeightTime = DateTime.MinValue;
    }
  }

  public class ClientCompositeHeightsLeafSubgrid : GenericClientLeafSubGrid<SubGridCellCompositeHeightsRecord>
  {
    /// <summary>
    /// Constructor. Set the grid to HeightAndTime.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="parent"></param>
    /// <param name="level"></param>
    /// <param name="cellSize"></param>
    /// <param name="indexOriginOffset"></param>
    public ClientCompositeHeightsLeafSubgrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
    {
      _gridDataType = TRex.Types.GridDataType.CompositeHeights;
    }

    /// <summary>
    /// Determines if the height at the cell location is null or not.
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <returns></returns>
    public override bool CellHasValue(byte cellX, byte cellY)
    {
        return Cells[cellX, cellY].LowestHeight != Consts.NullHeight ||
        Cells[cellX, cellY].HighestHeight != Consts.NullHeight ||
        Cells[cellX, cellY].LastHeight != Consts.NullHeight ||
        Cells[cellX, cellY].FirstHeight != Consts.NullHeight;
    }

    public void SetToZeroHeight()
    {
      SubGridUtilities.SubGridDimensionalIterator((i, j) =>
      {
        Cells[i, j].LowestHeight = 0;
        Cells[i, j].HighestHeight = 0;
        Cells[i, j].LastHeight = 0;
        Cells[i, j].FirstHeight = 0;
      });
    }

    public void SetHeightsToNull()
    {
      SubGridUtilities.SubGridDimensionalIterator((i, j) =>
      {
        Cells[i, j].LowestHeight = Consts.NullHeight;
        Cells[i, j].HighestHeight = Consts.NullHeight;
        Cells[i, j].LastHeight = Consts.NullHeight;
        Cells[i, j].FirstHeight = Consts.NullHeight;
      });
    }
  }
}
