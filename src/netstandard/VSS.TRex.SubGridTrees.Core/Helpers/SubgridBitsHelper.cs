using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Helpers
{
    /// <summary>
    /// A helper class that contains definitions of Clear and Filled Bits subgrid mitmask bit arrays as well as
    /// the number of elements in the Bits array
    /// </summary>
    public static class SubgridBitsHelper
    {
    /// <summary>
    /// The number of elements in the Bits array. Defined as:
    /// (SubGridTreeConsts.SubGridTreeCellsPerSubgrid * 8) / 2 unsigned ints
    /// </summary>
    public const int BitsArrayLength = SubGridTreeConsts.SubGridTreeDimension;

        /// <summary>
        /// The number of bytes occupied by the elements in a Bits array
        /// </summary>
        public const int BytesInBitsArray = BitsArrayLength * 4;

        /// <summary>
        /// A predefined array of cleared bits for a subgrid bitmask
        /// </summary>
        public static uint[] SubGridTreeLeafBitmapSubGridBits_Clear = 
            {
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0
            };

        /// <summary>
        /// A predefined array of filled bits for a subgrid bitmask
        /// </summary>
        public static uint[] SubGridTreeLeafBitmapSubGridBits_Fill = 
            {
            0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
            0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
            0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
            0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF
            };
    }
}
