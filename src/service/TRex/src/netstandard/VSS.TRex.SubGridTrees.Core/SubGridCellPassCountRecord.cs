namespace VSS.TRex.SubGridTrees
{
    /// <summary>
    /// Represents the number of passes and the location of the first cell pass for a cell within the set of cell passes
    /// stored in this sub grid segment
    /// </summary>
    public struct SubGridCellPassCountRecord
    {
        /// <summary>
        /// The number of passes in this cell in this segment
        /// </summary>
        public uint PassCount;

        /// <summary>
        /// The index of the first cell pass in the cell in this segment within the overall list of cell passes
        /// </summary>
        public uint FirstCellPass;
    }
}
