namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    /// <summary>
    /// Represents the number of passes and the location of the first cell pass for a cell within the set of cell passes
    /// stored in this subgrid segment
    /// </summary>
    public struct SubGridCellPassCountRecord
    {
        /// <summary>
        /// The number of passes in this cell in this segment
        /// </summary>
        public int PassCount;

        /// <summary>
        /// The index of the first cell pass inteh cell in this segment within the overall list of cell passes
        /// </summary>
        public int FirstCellPass;
    }
}
