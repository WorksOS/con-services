using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces
{
    public interface ISubGridCellLatestPassesDataWrapperFactory
    {
        ISubGridCellLatestPassesDataWrapperFactory NewWrapper();
    }
}
