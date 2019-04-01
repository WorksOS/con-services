using System;
using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
    public interface IServerLeafSubGrid : ILeafSubGrid
    {
        /// <summary>
        /// Controls whether segment and cell pass information held within this sub grid is represented
        /// in the mutable or immutable forms supported by TRex
        /// </summary>
        bool IsMutable { get; }

        void SetIsMutable(bool isMutable);

        ISubGridCellPassesDataWrapper Cells { get; set; }

        ISubGridDirectory Directory { get; set; }

        /// <summary>
        /// The date time of the first observed cell pass within this sub grid
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
        bool HasSubGridDirectoryDetails { get; }

        void AddPass(uint cellX, uint cellY, CellPass Pass);

        void ComputeLatestPassInformation(bool fullRecompute, IStorageProxy storageProxy);

        bool LoadDirectoryFromStream(Stream stream);
        bool SaveDirectoryToStream(Stream stream);
        bool LoadDirectoryFromFile(IStorageProxy storage, string fileName);

        bool SaveDirectoryToFile(IStorageProxy storage, string FileName);

        //bool SaveLeafSubGrid(IServerLeafSubGrid subGrid, IStorageProxy storageProxy);

        ISubGridSpatialAffinityKey AffinityKey();
    }
}
