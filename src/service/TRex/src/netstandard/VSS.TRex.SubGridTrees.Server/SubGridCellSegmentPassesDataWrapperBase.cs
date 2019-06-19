namespace VSS.TRex.SubGridTrees.Server
{
    /// <summary>
    /// Base class to hold common aspects of the segment implementation data wrapper between NonStatic, Static and 
    /// StaticCompressed implementations
    /// </summary>
    public abstract class SubGridCellSegmentPassesDataWrapperBase
    {
        protected int segmentPassCount;

        /// <summary>
        /// The count of cell passes residing in this sub grid segment
        /// </summary>
        public int SegmentPassCount
        {
          get => segmentPassCount;
          set => segmentPassCount = value;
        }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SubGridCellSegmentPassesDataWrapperBase()
        {
        }
    }
}
