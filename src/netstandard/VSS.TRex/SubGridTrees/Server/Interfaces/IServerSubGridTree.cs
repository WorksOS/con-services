using VSS.TRex.Storage.Interfaces;

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
