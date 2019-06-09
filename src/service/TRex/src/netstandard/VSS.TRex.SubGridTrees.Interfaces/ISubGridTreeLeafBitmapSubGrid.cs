using System;
using System.IO;
using VSS.TRex.Geometry;

namespace VSS.TRex.SubGridTrees.Interfaces
{
  public interface ISubGridTreeLeafBitmapSubGrid
  {
    /// <summary>
    /// CountBits counts the number of bits that are set to 1 (true) in the sub grid 
    /// </summary>
    /// <returns></returns>
    int CountBits();

    /// <summary>
    /// Computes the bounding extent of the cells (bits) in the sub grid that are set to 1 (true)
    /// </summary>
    /// <returns></returns>
    BoundingIntegerExtent2D ComputeCellsExtents();

    void ForEachSetBit(Action<int, int> functor);
    void ForEachSetBit(Func<int, int, bool> functor);

    void ForEachClearBit(Action<int, int> functor);

    void ForEach(Action<byte, byte> functor);
    void ForEach(Func<byte, byte, bool> functor);
  }
}
