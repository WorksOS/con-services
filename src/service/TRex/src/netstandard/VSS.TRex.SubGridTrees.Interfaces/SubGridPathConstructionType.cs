namespace VSS.TRex.SubGridTrees.Types
{
    /// <summary>
    /// When constructing a path through a sub grid tree to a sub grid, use the following mode to control
    /// how the path is followed and/or constructed as required
    /// </summary>
    public enum SubGridPathConstructionType : byte
  {
        /// <summary>
        /// Creates an appropriate leaf sub grid, if none exists
        /// </summary>
        CreateLeaf,

        /// <summary>
        /// Creates internal node sub grids to level above leaf sub grid, but does not create the leaf
        /// </summary>
        CreatePathToLeaf,

        /// <summary>
        /// Will only return a leaf if it already exists, null otherwise
        /// </summary>
        ReturnExistingLeafOnly
    }
}
