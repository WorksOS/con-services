using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees
{
    /// <summary>
    /// Base class for any sub grid derivative that contains data at the bottom layer of a sub grid tree
    /// </summary>
    public class LeafSubGridBase : SubGrid
    {
        public LeafSubGridBase(ISubGridTree owner,
                               ISubGrid parent,
                               byte level,
                               double cellSize,
                               int indexOriginOffset) : this(owner, parent, level)
        {
        }

        public LeafSubGridBase(ISubGridTree owner,
                               ISubGrid parent,
                               byte level) : base(owner, parent, level)
        {
        }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public LeafSubGridBase() : base(null, null, SubGridTreeConsts.SubGridTreeLevels)
        {

        }
    }
}
