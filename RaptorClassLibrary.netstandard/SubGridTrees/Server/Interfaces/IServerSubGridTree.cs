using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Server;

namespace VSS.VisionLink.Raptor.SubGridTrees.Interfaces
{
    public interface IServerSubGridTree
    {
        bool LoadLeafSubGridSegment(IStorageProxy StorageProxy,
                                    SubGridCellAddress cellAddress,
                                    bool loadLatestData,
                                    bool loadAllPasses,
                                    IServerLeafSubGrid SubGrid,
                                    SubGridCellPassesDataSegment Segment /*,
                                    SiteModel SiteModelReference*/);
    }
}
