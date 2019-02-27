namespace VSS.TRex.SubGridTrees.Interfaces
{
    /// <summary>
    /// Factory interface used to implement concrete sub grid factory for use by a sub grid tree
    /// </summary>
    public interface ISubGridFactory
    {
        /// <summary>
        /// Construct a concrete instance of a sub grid implementing the ISubGrid interface based
        /// on the role it should play according to the sub grid tree level requested.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="treeLevel"></param>
        /// <returns></returns>
        ISubGrid GetSubGrid(ISubGridTree tree, byte treeLevel);
    }
}
