using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees
{
    /// <summary>
    /// The factory responsible for creating subgrids within a subgrid tree.
    /// </summary>
    /// <typeparam name="Node"></typeparam>
    /// <typeparam name="Leaf"></typeparam>
    public class SubGridFactory<Node, Leaf> : ISubGridFactory where Node : INodeSubGrid where Leaf : ILeafSubGrid
    {
        /// <summary>
        /// Construct either a node or a leaf subgrid for the given sub grid tree at the given level using the generic
        /// types Node and Leaf.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="treeLevel"></param>
        /// <returns>An ISubGrid interface representing the newly created node or leaf subrid</returns>
        public ISubGrid GetSubGrid(ISubGridTree tree, byte treeLevel)
        {
            // Ensure the requested tree level is valid for the given tree
            if (treeLevel < 1 || treeLevel > tree.NumLevels)
            {
                throw new ArgumentException(String.Format("Invalid treeLevel in subgrid factory: {0}, range is 1-{1}", treeLevel, tree.NumLevels), "treeLevel");
            }

            Type NodeType = (treeLevel < tree.NumLevels) ? typeof(Node) : typeof(Leaf);

            return (ISubGrid)Activator.CreateInstance(NodeType, tree, null, treeLevel /*, tree.CellSize, tree.IndexOriginOffset*/);
        }
    }
}
