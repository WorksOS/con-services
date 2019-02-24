using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Core.Helpers
{
    /// <summary>
    /// A helper class that contains definitions of Clear and Filled Bits sub grid bitmask bit arrays as well as
    /// the number of elements in the Bits array
    /// </summary>
    public static class SubGridBitsHelper
    {
    /// <summary>
    /// The number of elements in the Bits array. Defined as:
    /// (SubGridTreeConsts.SubGridTreeCellsPerSubGrid * 8) / 2 unsigned integers
    /// </summary>
    public const int BitsArrayLength = SubGridTreeConsts.SubGridTreeDimension;

        /// <summary>
        /// The number of bytes occupied by the elements in a Bits array
        /// </summary>
        public const int BytesInBitsArray = BitsArrayLength * 4;

        /// <summary>
        /// A predefined array of cleared bits for a sub grid bitmask
        /// </summary>
        public static readonly uint[] SubGridTreeLeafBitmapSubGridBits_Clear = 
            {
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0
            };

        /// <summary>
        /// A predefined array of filled bits for a sub grid bitmask
        /// </summary>
        public static readonly uint[] SubGridTreeLeafBitmapSubGridBits_Fill = 
            {
            0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
            0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
            0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
            0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF
            };
    }
}
