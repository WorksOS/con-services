using VSS.VisionLink.Raptor.SubGridTrees.Server;

namespace VSS.VisionLink.Raptor.SubGridTrees.Interfaces
{
    public interface IServerSubGridTree
    {
        bool LoadLeafSubGridSegment(SubGridCellAddress cellAddress,
                                    bool loadLatestData,
                                    bool loadAllPasses,
                                    IServerLeafSubGrid SubGrid,
                                    SubGridCellPassesDataSegment Segment /*,
                                    SiteModel SiteModelReference*/);
    }
}
