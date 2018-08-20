using System;
using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
    public interface IServerLeafSubGrid : ILeafSubGrid
    {
        ISubGridCellPassesDataWrapper Cells { get; set; }

        ISubGridDirectory Directory { get; set; }

        /// <summary>
        /// The date time of the first observed cell pass within this subgrid
        /// </summary>
        DateTime LeafStartTime { get; }

        /// <summary>
        /// The date time of the last observed cell pass within this subgrid
        /// </summary>
        DateTime LeafEndTime { get; }

        void CreateDefaultSegment();

        void AllocateSegment(ISubGridCellPassesDataSegmentInfo segmentInfo);

        void AllocateFullPassStacks(ISubGridCellPassesDataSegmentInfo SegmentInfo);
        void AllocateLatestPassGrid(ISubGridCellPassesDataSegmentInfo SegmentInfo);
        void AllocateLeafFullPassStacks();
        void AllocateLeafLatestPassGrid();

        void DeAllocateLeafFullPassStacks();
        void DeAllocateLeafLatestPassGrid();

        bool LoadSegmentFromStorage(IStorageProxy storageProxy, string FileName, ISubGridCellPassesDataSegment Segment, bool loadLatestData, bool loadAllPasses);

        void Integrate(IServerLeafSubGrid Source, ISubGridSegmentIterator Iterator, bool IntegratingIntoIntermediaryGrid);

        bool HasAllCellPasses();
        bool HasLatestData();
        bool LatestCellPassesOutOfDate { get; }
        bool HaveSubgridDirectoryDetails { get; }

        void AddPass(uint cellX, uint cellY, CellPass Pass);

        void ComputeLatestPassInformation(bool fullRecompute, IStorageProxy storageProxy);

        bool LoadDirectoryFromStream(Stream stream);
        bool SaveDirectoryToStream(Stream stream);
        bool LoadDirectoryFromFile(IStorageProxy storage, string fileName);

        bool SaveDirectoryToFile(IStorageProxy storage, string FileName
            /* const AInvalidatedSpatialStreams : TInvalidatedSpatialStreamArray*/);
    }
}
