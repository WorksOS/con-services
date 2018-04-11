using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators;

namespace VSS.VisionLink.Raptor.SubGridTrees.Interfaces
{
    public interface IServerLeafSubGrid : ILeafSubGrid
    {
        SubGridCellPassesDataWrapper Cells { get; set; }

        SubGridDirectory Directory { get; set; }

        void CreateDefaultSegment();

        void AllocateSegment(SubGridCellPassesDataSegmentInfo segmentInfo);

        void AllocateFullPassStacks(SubGridCellPassesDataSegmentInfo SegmentInfo);
        void AllocateLatestPassGrid(SubGridCellPassesDataSegmentInfo SegmentInfo);
        void AllocateLeafFullPassStacks();
        void AllocateLeafLatestPassGrid();

        bool LoadSegmentFromStorage(IStorageProxy storageProxy, string FileName, SubGridCellPassesDataSegment Segment, bool loadLatestData, bool loadAllPasses /*, SiteModel SiteModelReference*/);

        void Integrate(ServerSubGridTreeLeaf Source, SubGridSegmentIterator Iterator, bool IntegratingIntoIntermediaryGrid);

        void AddPass(uint cellX, uint cellY, CellPass Pass);
    }
}
