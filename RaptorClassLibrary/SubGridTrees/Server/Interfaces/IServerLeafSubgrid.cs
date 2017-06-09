using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators;

namespace VSS.VisionLink.Raptor.SubGridTrees.Interfaces
{
    public interface IServerLeafSubGrid : ILeafSubGrid, ISubGrid
    {
        SubGridCellPassesDataWrapper Cells { get; set; }

        SubGridDirectory Directory { get; set; }

        void CreateDefaultSegment();

        void AllocateSegment(SubGridCellPassesDataSegmentInfo segmentInfo);

        void AllocateFullPassStacks(SubGridCellPassesDataSegmentInfo SegmentInfo);
        void AllocateLatestPassGrid(SubGridCellPassesDataSegmentInfo SegmentInfo);
        void AllocateLeafFullPassStacks();
        void AllocateLeafLatestPassGrid();

        bool LoadSegmentFromStorage(IStorageProxy storageProxy, string FileName, SubGridCellPassesDataSegment Segment, bool loadLatestData, bool loadAllPasses, SiteModel SiteModelReference);

        void Integrate(ServerSubGridTreeLeaf Source, SubGridSegmentIterator Iterator, bool IntegratingIntoIntermediaryGrid);
    }
}
