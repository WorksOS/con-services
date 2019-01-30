namespace VSS.TRex.SubGridTrees.Types
{
    /// <summary>
    /// Control state for iterating through sub grids in a sub grid tree
    /// A notification event called when scanning node sub grids in the sub grid tree
    /// (or any other events where the node sub grid needs to passed to a processor). 
    /// A result of DontDescendFurther indicates that no further processing should occur in 
    /// child nodes of this node. A result of TerminateProcessing indicates no further scanning 
    /// should occur in the tree.
    /// </summary>
    public enum SubGridProcessNodeSubGridResult
    {
        /// <summary>
        /// Everything is fine :-)
        /// </summary>
        OK,

        /// <summary>
        /// Scanning should not recurse further into the structure past this node sub grid
        /// </summary>
        DontDescendFurther,

        /// <summary>
        /// Stop scanning the tree
        /// </summary>
        TerminateProcessing
    }
}
