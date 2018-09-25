using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees
{
    /// <summary>
    /// A specialised descendent of SubGridTreeBitMask tailored to tracking subgrid existence within a larger subgrid tree.
    /// Existence tracking requires a single bit to be maintained per on-the-ground leaf in the subgrid (ie: leaves existing
    /// at the bottom level of the subgrid tree.
    /// 
    /// This means the subgrid tree tracking this information may be one level 
    /// shallower than the subgrid tree containing the subgrid whose existence are being tracked.
    /// 
    /// It also means that the effective cell size of the entries in the shallower subgrid tree need to be multiplied by
    /// the subgrid cell dimension to correctly represented the spatial coverage of each bit (representing a full subgrid).
    /// </summary>
    public class SubGridTreeSubGridExistenceBitMask : SubGridTreeBitMask
    {
        /// <summary>
        /// Default no-arg constructor that creates a subgrid tree one level shallower than the default, and with a correspondingly larger cell size
        /// </summary>
        public SubGridTreeSubGridExistenceBitMask() : base(SubGridTreeConsts.SubGridTreeLevels - 1, SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.SubGridTreeDimension)
        {
        }
    }
}
