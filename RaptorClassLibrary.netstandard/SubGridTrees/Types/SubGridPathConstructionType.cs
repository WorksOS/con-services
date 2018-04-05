namespace VSS.VisionLink.Raptor.SubGridTrees.Types
{
    /// <summary>
    /// When construcing a path through a subgrid tree to a subgrid, use the following mode to control
    /// how the path is followed and/or constructed as required
    /// </summary>
    public enum SubGridPathConstructionType
    {
        /// <summary>
        /// Creates an appropriate leaf subgrid, if none exists
        /// </summary>
        CreateLeaf,

        /// <summary>
        /// Creates internal node subgrids to level above leaf subgrid, but does not create the leaf
        /// </summary>
        CreatePathToLeaf,

        /// <summary>
        /// Will only return a leaf if it already exists, null otherwise
        /// </summary>
        ReturnExistingLeafOnly
    }
}
