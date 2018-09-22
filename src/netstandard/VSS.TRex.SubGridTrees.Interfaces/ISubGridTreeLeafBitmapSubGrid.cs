using System;
using System.IO;
using VSS.TRex.Geometry;

namespace VSS.TRex.SubGridTrees.Interfaces
{
  public interface ISubGridTreeLeafBitmapSubGrid
  {
    /// <summary>
    /// Writes the contents of the subgrid bit mask to the writer
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="buffer"></param>
    void Write(BinaryWriter writer, byte[] buffer);

    /// <summary>
    /// Reads the contents of the subgrid bit mask from the reader
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="buffer"></param>
    void Read(BinaryReader reader, byte[] buffer);

    /// <summary>
    /// CountBits counts the number of bits that are set to 1 (true) in the subgrid 
    /// </summary>
    /// <returns></returns>
    uint CountBits();

    /// <summary>
    /// Computes the bounding extent of the cells (bits) in the subgrid that are set to 1 (true)
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
