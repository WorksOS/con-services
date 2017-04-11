using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    public class ServerLeafSubGridBase : LeafSubGrid, ILeafSubGrid
    {
        public ServerLeafSubGridBase(ISubGridTree owner,
                                     ISubGrid parent,
                                     byte level) : base(owner, parent, level)
        {
        }
    }
}
