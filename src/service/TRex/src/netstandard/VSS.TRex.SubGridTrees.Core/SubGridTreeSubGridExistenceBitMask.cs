using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees
{
    /// <summary>
    /// A specialized descendent of SubGridTreeBitMask tailored to tracking sub grid existence within a larger sub grid tree.
    /// Existence tracking requires a single bit to be maintained per on-the-ground leaf in the sub grid (ie: leaves existing
    /// at the bottom level of the sub grid tree.
    /// 
    /// This means the sub grid tree tracking this information may be one level 
    /// shallower than the sub grid tree containing the sub grid whose existence are being tracked.
    /// 
    /// It also means that the effective cell size of the entries in the shallower sub grid tree need to be multiplied by
    /// the sub grid cell dimension to correctly represented the spatial coverage of each bit (representing a full sub grid).
    /// </summary>
    public class SubGridTreeSubGridExistenceBitMask : SubGridTreeBitMask
    {
        /// <summary>
        /// Default no-arg constructor that creates a sub grid tree one level shallower than the default, and with a correspondingly larger cell size
        /// </summary>
        public SubGridTreeSubGridExistenceBitMask() : base(SubGridTreeConsts.SubGridTreeLevels - 1, SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.SubGridTreeDimension)
        {
        }
    }
}
