using System;
using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
    public interface IServerLeafSubGrid : ILeafSubGrid, IDisposable
    {
        /// <summary>
        /// The version number of this spatial element when it is stored in the persistent layer, defined
        /// as the number of ticks in DateTime.UtcNow at the time it is written.
        /// </summary>
        long Version { get; set; }
       
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
        /// The date time of the last observed cell pass within this sub grid
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

        bool RemoveSegmentFromStorage(IStorageProxy storageProxy, string fileName, ISubGridCellPassesDataSegmentInfo segment);
        bool RemoveDirectoryFromStorage(IStorageProxy storageProxy, string fileName);

        void Integrate(IServerLeafSubGrid source, ISubGridSegmentIterator iterator, bool integratingIntoIntermediaryGrid);

        bool HasAllCellPasses();
        bool HasLatestData();
        bool HasSubGridDirectoryDetails { get; }

        void AddPass(int cellX, int cellY, CellPass Pass);

        void ComputeLatestPassInformation(bool fullRecompute, IStorageProxy storageProxy);

        bool LoadDirectoryFromStream(Stream stream);
        bool SaveDirectoryToStream(Stream stream);
        bool LoadDirectoryFromFile(IStorageProxy storage, string fileName);

        bool SaveDirectoryToFile(IStorageProxy storage, string FileName);

        //bool SaveLeafSubGrid(IServerLeafSubGrid subGrid, IStorageProxy storageProxy);

        ISubGridSpatialAffinityKey AffinityKey();
    }
}
