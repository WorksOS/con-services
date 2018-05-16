using System;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.SubGridTrees.Interfaces
{
    public interface INodeSubGrid : ISubGrid
    {
        void DeleteSubgrid(byte SubGridX, byte SubGridY, bool DeleteIfLocked);

        bool GetSubGridContainingCell(uint CellX, uint CellY, out byte SubGridX, out byte SubGridY);

        void ForEachSubGrid(Func<ISubGrid, SubGridProcessNodeSubGridResult> functor,
            byte minSubGridCellX = 0,
            byte minSubGridCellY = 0,
            byte maxSubGridCellX = SubGridTree.SubGridTreeDimensionMinus1,
            byte maxSubGridCellY = SubGridTree.SubGridTreeDimensionMinus1);

        void ForEachSubGrid(Func<byte, byte, ISubGrid, SubGridProcessNodeSubGridResult> functor,
            byte minSubGridCellX = 0,
            byte minSubGridCellY = 0,
            byte maxSubGridCellX = SubGridTree.SubGridTreeDimensionMinus1,
            byte maxSubGridCellY = SubGridTree.SubGridTreeDimensionMinus1);

        bool ScanSubGrids(BoundingIntegerExtent2D Extent,
                          Func<ISubGrid, bool> leafFunctor = null,
                          Func<ISubGrid, SubGridProcessNodeSubGridResult> nodeFunctor = null);

        int CountChildren();
    }
}
