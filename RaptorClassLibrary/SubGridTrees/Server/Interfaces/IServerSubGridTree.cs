using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.SubGridTrees.Server;

namespace VSS.VisionLink.Raptor.SubGridTrees.Interfaces
{
    public interface IServerSubGridTree
    {
        bool LoadLeafSubGridSegment(SubGridCellAddress cellAddress,
                                    bool loadLatestData,
                                    bool loadAllPasses,
                                    IServerLeafSubGrid SubGrid,
                                    SubGridCellPassesDataSegment Segment,
                                    SiteModel SiteModelReference);
    }
}
