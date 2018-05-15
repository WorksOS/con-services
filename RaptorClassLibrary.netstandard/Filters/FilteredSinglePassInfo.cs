namespace VSS.TRex.Filters
{
    /// <summary>
    /// Records all the information that a filtering operation selected from a grid
    /// cell containing all the recorded machine passes.
    /// </summary>
    public struct FilteredSinglePassInfo
    {
        /// <summary>
        /// PassCount stores the number of passes present in the cell from which
        /// the single pass was filtered. In cases where the pass count is derived as
        /// a count of the number of passes that represent compactive effort and include
        /// compaction machines for which a 'pass' over the ground is considered to
        /// only represent a 'half pass', then this count will include the whole number
        /// of 'full' passes for those machine where a full pass is defined as two
        /// single 'half' passes from those machines.
        /// </summary>
        public int PassCount { get; set; }

        /// <summary>
        /// Filtered data values, target values and event values related to the filtered cell pass information
        /// </summary>
        public FilteredPassData FilteredPassData;

        /// <summary>
        /// Intialise the filtered singlepass to a null state
        /// </summary>
        public void Clear()
        {
            FilteredPassData.Clear();

            PassCount = 0;
        }
    }
}
