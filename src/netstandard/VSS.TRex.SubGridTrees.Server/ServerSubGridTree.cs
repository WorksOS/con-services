using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Factories;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{
    public class ServerSubGridTree : SubGridTree, IServerSubGridTree
    {
        private static ILogger Log = Logging.Logger.CreateLogger<ServerSubGridTree>();

        /// <summary>
        /// Controls emission of subgrid reading activities into the log.
        /// </summary>
        public bool RecordSubgridFileReadingToLog { get; set; } = false;

        public ServerSubGridTree(Guid siteModelID) :
            base(SubGridTreeConsts.SubGridTreeLevels, SubGridTreeConsts.DefaultCellSize,
                new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>())
        {
            ID = siteModelID; // Ensure the ID of the subgrid tree matches the datamodel ID
        }

        public ServerSubGridTree(byte numLevels,
                                 double cellSize,
                                 ISubGridFactory subGridFactory) : base(numLevels, cellSize, subGridFactory)
        { }

        /// <summary>
        /// Computes a unique file name for a segment within a particular subgrid
        /// </summary>
        /// <param name="CellAddress"></param>
        /// <param name="SegmentInfo"></param>
        /// <returns></returns>
        public static string GetLeafSubGridSegmentFullFileName(SubGridCellAddress CellAddress,
                                                               ISubGridCellPassesDataSegmentInfo SegmentInfo)
        {
            // Work out the cell address of the origin cell in the appropriate leaf
            // subgrid. We use this cell position to derive the name of the file
            // containing the leaf subgrid data
            return SegmentInfo.FileName(new SubGridCellAddress((uint)(CellAddress.X & ~SubGridTreeConsts.SubGridLocalKeyMask), 
                                                               (uint)(CellAddress.Y & ~SubGridTreeConsts.SubGridLocalKeyMask)));
        }

        public bool LoadLeafSubGridSegment(IStorageProxy storageProxy,
                                           SubGridCellAddress cellAddress,
                                           bool loadLatestData,
                                           bool loadAllPasses,
                                           IServerLeafSubGrid SubGrid,
                                           ISubGridCellPassesDataSegment Segment)
        {
            bool needToLoadLatestData = loadLatestData && !Segment.HasLatestData;
            bool needToLoadAllPasses = loadAllPasses && !Segment.HasAllPasses;

            if (!needToLoadLatestData && !needToLoadAllPasses)              
                return true; // Nothing more to do here

            // Lock the segment briefly while its contents is being loaded
            lock (Segment)
            {
                if (loadLatestData == !Segment.HasLatestData && needToLoadAllPasses == !Segment.HasAllPasses)                  
                    return true; // The load operation was performed on another thread. Leave quietly

                // Ensure the appropriate storage is allocated
                if (needToLoadLatestData)
                    Segment.AllocateLatestPassGrid();
             
                if (needToLoadAllPasses)
                    Segment.AllocateFullPassStacks();
             
                if (!Segment.SegmentInfo.ExistsInPersistentStore)                  
                  return true; // Nothing more to do here
             
                // Locate the segment file and load the data from it
                string FullFileName = GetLeafSubGridSegmentFullFileName(cellAddress, Segment.SegmentInfo);
             
                // Load the cells into it from its file
                bool FileLoaded = SubGrid.LoadSegmentFromStorage(storageProxy, FullFileName, Segment, needToLoadLatestData, needToLoadAllPasses);
             
                if (!FileLoaded)
                {
                    // Oops, something bad happened. Remove the segment from the list. Return failure to the caller.
                    if (loadAllPasses)
                      Segment.DeAllocateFullPassStacks();
                
                    if (loadLatestData)
                      Segment.DeAllocateLatestPassGrid();
                
                    return false;
                }
            }

          return true;
        }

        public bool LoadLeafSubGrid(IStorageProxy storageProxy,
                                    SubGridCellAddress CellAddress,
                                    bool loadAllPasses, bool loadLatestPasses,
                                    IServerLeafSubGrid SubGrid)
        {
            string FullFileName = string.Empty;
            bool Result = false;

            try
            {
                // Loading contents into a dirty subgrid (which should happen only on the mutable nodes), or
                // when there is already content in the segment directory are strictly forbidden and break immutability
                // rules for subgrids
                Debug.Assert(!SubGrid.Dirty, "Leaf subgrid directory loads may not be performed while the subgrid is dirty. The information should be taken from the cache instead.");
                Debug.Assert(SubGrid.Directory?.SegmentDirectory?.Count == 0, "Loading a leaf subgrid directory when there are already segments present within it.");

                // Load the cells into it from its file
                // Briefly lock this subgrid just for the period required to read its contents
                lock (SubGrid)
                {
                    // Check this thread is the winner of the lock to be able to load the contents
                    if (SubGrid.Directory?.SegmentDirectory?.Count != 0)                       
                        return true; // The load has occurred on another thread, leave quietly...

                    // Ensure the appropriate storage is allocated
                    if (loadAllPasses)
                        SubGrid.AllocateLeafFullPassStacks();
                 
                    if (loadLatestPasses)
                        SubGrid.AllocateLeafLatestPassGrid();
                 
                    FullFileName = GetLeafSubGridFullFileName(CellAddress);
                 
                    // Briefly lock this subgrid just for the period required to read its contents
                    Result = SubGrid.LoadDirectoryFromFile(storageProxy, FullFileName);
                }
            }
            finally
            {
              if (Result && RecordSubgridFileReadingToLog)
                  Log.LogDebug($"Subgrid file {FullFileName} read from persistent store containing {SubGrid.Directory.SegmentDirectory.Count} segments (Moniker: {SubGrid.Moniker()}");
            }

            return Result;
        }

        public static string GetLeafSubGridFullFileName(SubGridCellAddress CellAddress)
        {
            // Work out the cell address of the origin cell in the appropriate leaf
            // subgrid. We use this cell position to derive the name of the file
            // containing the leaf subgrid data
            return ServerSubGridTreeLeaf.FileNameFromOriginPosition(new SubGridCellAddress((uint)(CellAddress.X & ~SubGridTreeConsts.SubGridLocalKeyMask), (uint)(CellAddress.Y & ~SubGridTreeConsts.SubGridLocalKeyMask)));
        }
    }
}
