using System;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.SubGridTrees.Interfaces
{
    public interface INodeSubGrid : ISubGrid
    {
        void DeleteSubgrid(byte SubGridX, byte SubGridY, bool DeleteIfLocked);

        ISubGrid GetSubGrid(byte X, byte Y);

        void SetSubGrid(byte X, byte Y, ISubGrid Value);

        bool GetSubGridContainingCell(uint CellX, uint CellY, out byte SubGridX, out byte SubGridY);

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

        bool ScanSubGrids(BoundingIntegerExtent2D Extent,
                          Func<ISubGrid, bool> leafFunctor = null,
                          Func<ISubGrid, SubGridProcessNodeSubGridResult> nodeFunctor = null);

        int CountChildren();
    }
}
