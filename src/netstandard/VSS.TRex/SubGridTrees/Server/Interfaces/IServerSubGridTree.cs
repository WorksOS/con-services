using VSS.TRex.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Server;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
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
