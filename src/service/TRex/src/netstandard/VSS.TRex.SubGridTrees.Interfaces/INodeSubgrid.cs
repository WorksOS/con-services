using System;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.SubGridTrees.Interfaces
{
  public interface INodeSubGrid : ISubGrid
  {
    void DeleteSubGrid(int subGridX, int subGridY);

    new ISubGrid GetSubGrid(int x, int y);

    new void SetSubGrid(int x, int y, ISubGrid value);

    ISubGrid GetSubGridContainingCell(int cellX, int cellY);

    void ForEachSubGrid(Func<ISubGrid, SubGridProcessNodeSubGridResult> functor);

    void ForEachSubGrid(Func<ISubGrid, SubGridProcessNodeSubGridResult> functor,
      byte minSubGridCellX,
      byte minSubGridCellY,
      byte maxSubGridCellX,
      byte maxSubGridCellY);

    void ForEachSubGrid(Func<byte, byte, ISubGrid, SubGridProcessNodeSubGridResult> functor);

    void ForEachSubGrid(Func<byte, byte, ISubGrid, SubGridProcessNodeSubGridResult> functor,
      byte minSubGridCellX,
      byte minSubGridCellY,
      byte maxSubGridCellX,
      byte maxSubGridCellY);

    bool ScanSubGrids(BoundingIntegerExtent2D extent,
      Func<ISubGrid, bool> leafFunctor = null,
      Func<ISubGrid, SubGridProcessNodeSubGridResult> nodeFunctor = null);

    int CountChildren();
  }
}
