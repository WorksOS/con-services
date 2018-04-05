namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators
{
    /// <summary>
    /// The time iteration direction for iterating thorugh the segments in the subgrid
    /// </summary>
    public enum IterationDirection
    {
        /// <summary>
        /// Iteration proceeds from oldest segment to newest segment
        /// </summary>
        Forwards,

        /// <summary>
        /// Iteration proceeds from newest segment to oldest segment
        /// </summary>
        Backwards
    }
}
