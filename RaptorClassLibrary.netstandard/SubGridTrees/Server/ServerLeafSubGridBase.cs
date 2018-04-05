using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    public class ServerLeafSubGridBase : LeafSubGrid
    {
        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public ServerLeafSubGridBase()
        {
        }

        public ServerLeafSubGridBase(ISubGridTree owner,
                                     ISubGrid parent,
                                     byte level) : base(owner, parent, level)
        {
        }
    }
}
