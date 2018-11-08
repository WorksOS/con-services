namespace ProductionDataSvc.AcceptanceTests.Models
{
    /// <summary>
    /// Specifies CCV summarize mode. Initially was a set, but here is limited with only one selection.
    /// </summary>
    public enum CCVSummaryType
    {
        /// <summary>
        /// Summarize by compaction
        /// </summary>
        Compaction,
        /// <summary>
        /// Summarize by thickness
        /// </summary>
        Thickness,
        /// <summary>
        /// Summarize by workinprogress
        /// </summary>
        WorkInProgress
    }
}