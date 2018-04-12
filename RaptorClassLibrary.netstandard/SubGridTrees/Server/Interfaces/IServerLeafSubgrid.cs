using System;
using System.IO;
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

        /// <summary>
        /// The date time of the first observed cell pass within this subgrid
        /// </summary>
        DateTime LeafStartTime { get; }

        /// <summary>
        /// The date time of the last observed cell pass within this subgrid
        /// </summary>
        DateTime LeafEndTime { get; }

        void CreateDefaultSegment();

        void AllocateSegment(SubGridCellPassesDataSegmentInfo segmentInfo);

        void AllocateFullPassStacks(SubGridCellPassesDataSegmentInfo SegmentInfo);
        void AllocateLatestPassGrid(SubGridCellPassesDataSegmentInfo SegmentInfo);
        void AllocateLeafFullPassStacks();
        void AllocateLeafLatestPassGrid();

        void DeAllocateLeafFullPassStacks();
        void DeAllocateLeafLatestPassGrid();

        bool LoadSegmentFromStorage(IStorageProxy storageProxy, string FileName, SubGridCellPassesDataSegment Segment, bool loadLatestData, bool loadAllPasses /*, SiteModel SiteModelReference*/);

        void Integrate(IServerLeafSubGrid Source, SubGridSegmentIterator Iterator, bool IntegratingIntoIntermediaryGrid);

        bool HasAllCellPasses();
        bool HasLatestData();
        bool LatestCellPassesOutOfDate { get; }
        bool HaveSubgridDirectoryDetails { get; }

        void AddPass(uint cellX, uint cellY, CellPass Pass);

        void ComputeLatestPassInformation(bool fullRecompute);

        bool LoadDirectoryFromStream(Stream stream);
        bool SaveDirectoryToStream(Stream stream);
        bool LoadDirectoryFromFile(IStorageProxy storage, string fileName);

        bool SaveDirectoryToFile(IStorageProxy storage, string FileName
            /* const AInvalidatedSpatialStreams : TInvalidatedSpatialStreamArray*/);
    }
}
