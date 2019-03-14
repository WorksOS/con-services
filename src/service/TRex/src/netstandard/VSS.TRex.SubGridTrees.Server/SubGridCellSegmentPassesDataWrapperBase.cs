namespace VSS.TRex.SubGridTrees.Server
{
    /// <summary>
    /// Base class to hold common aspects of the segment implementation data wrapper between NonStatic, Static and 
    /// StaticCompressed implementations
    /// </summary>
    public abstract class SubGridCellSegmentPassesDataWrapperBase
    {
        /// <summary>
        /// The count of cell passes residing in this sub grid segment
        /// </summary>
        public uint SegmentPassCount { get; set; }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SubGridCellSegmentPassesDataWrapperBase()
        {
        }
    }
}
