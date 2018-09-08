using System;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees
{
    /// <summary>
    /// LeafSubgrid is the true base class from which to derive varieties of leaf subgrid that support different data types
    /// and use cases.
    /// </summary>
    public class LeafSubGrid : LeafSubGridBase, ILeafSubGrid
    {
        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public LeafSubGrid()
        {
        }

        public LeafSubGrid(ISubGridTree owner,
                           ISubGrid parent,
                           byte level) : base(owner, parent, level)
        {
            // Assert level = tree.NumLevels (leaves are only at the tips)
            if (owner != null && level != owner.NumLevels)
                throw new ArgumentException("Requested level for leaf subgrid <> number of levels in tree", nameof(level));
        }

        public override bool IsEmpty()
        {
            for (byte I = 0; I < SubGridTreeConsts.SubGridTreeDimension; I++)
            {
                for (byte J = 0; J < SubGridTreeConsts.SubGridTreeDimension; J++)
                {
                    if (CellHasValue(I, J))
                        return false;
                }
            }

            return true;
        }
    }
}
