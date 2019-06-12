using System;
using System.IO;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees
{
  /// <summary>
    ///  A sub grid variant that contains a bit mask construct to represent a one-bit-per-pixel map
    /// </summary>
    public class SubGridTreeLeafBitmapSubGrid : LeafSubGridBase, ILeafSubGrid, ISubGridTreeLeafBitmapSubGrid
    {
        public SubGridTreeBitmapSubGridBits Bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

        /// <summary>
        /// Writes the contents of the sub grid bit mask to the writer
        /// </summary>
        /// <param name="writer"></param>
        public override void Write(BinaryWriter writer)
        {
            Bits.Write(writer);
        }

        /// <summary>
        /// Reads the contents of the sub grid bit mask from the reader
        /// </summary>
        /// <param name="reader"></param>
        public override void Read(BinaryReader reader)
        {
            Bits.Read(reader);
        }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SubGridTreeLeafBitmapSubGrid()
        {
        }

        /// <summary>
        /// Constructor taking the tree reference, parent and level of the sub grid to be created
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        public SubGridTreeLeafBitmapSubGrid(ISubGridTree owner,
            ISubGrid parent,
            byte level) : base(owner, parent, level)
        {
        }

        /// <summary>
        /// CountBits counts the number of bits that are set to 1 (true) in the sub grid 
        /// </summary>
        /// <returns></returns>
        public int CountBits() => Bits.CountBits();

        /// <summary>
        /// Computes the bounding extent of the cells (bits) in the sub grid that are set to 1 (true)
        /// </summary>
        /// <returns></returns>
        public BoundingIntegerExtent2D ComputeCellsExtents()
        {
            BoundingIntegerExtent2D Result = Bits.ComputeCellsExtents();

            if (Result.IsValidExtent)
                Result.Offset((int)OriginX, (int)OriginY);

            return Result;
        }

      public void ForEachSetBit(Action<int, int> functor) => Bits.ForEachSetBit(functor);
      public void ForEachSetBit(Func<int, int, bool> functor) => Bits.ForEachSetBit(functor);
      public void ForEachClearBit(Action<int, int> functor) => Bits.ForEachClearBit(functor);

      public new void ForEach(Action<byte, byte> functor) => Bits.ForEach(functor);
      public void ForEach(Func<byte, byte, bool> functor) => Bits.ForEach(functor);
    }
}
