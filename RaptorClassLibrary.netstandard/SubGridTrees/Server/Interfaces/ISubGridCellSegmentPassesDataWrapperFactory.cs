using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces
{
    /// <summary>
    /// Interface for the subgrid cell segment cell pass collection wrapper factory
    /// </summary>
    public interface ISubGridCellSegmentPassesDataWrapperFactory
    {
        ISubGridCellSegmentPassesDataWrapper NewWrapper();
    }
}
