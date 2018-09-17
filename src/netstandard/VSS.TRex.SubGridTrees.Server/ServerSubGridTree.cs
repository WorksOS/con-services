﻿using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Factories;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{
    public class ServerSubGridTree : SubGridTree, IServerSubGridTree
    {
        private static ILogger Log = Logging.Logger.CreateLogger(nameof(ServerSubGridTree));

        /// <summary>
        /// Controls emission of subgrid reading activities into the log.
        /// </summary>
        public bool RecordSubgridFileReadingToLog { get; set; } = false;

        public ServerSubGridTree(Guid siteModelID) :
            base(SubGridTreeConsts.SubGridTreeLevels, SubGridTreeConsts.DefaultCellSize,
                new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>())
        {
            // FSerialisedStream := Nil;
            // SerialisedCompressorStream := Nil;
            // FIsNewlyCreated := False;

            ID = siteModelID; // Ensure the ID of the subgrid tree matches the datamodel ID

            // FIsValid:= True;
        }

        public ServerSubGridTree(byte numLevels,
                                 double cellSize,
                                 ISubGridFactory subGridfactory) : base(numLevels, cellSize, subGridfactory)
        {

        }

        /// <summary>
        /// Computes a unique file nmae for a segment within a particular subgrid
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
            return SegmentInfo.FileName(new SubGridCellAddress((uint)(CellAddress.X & ~SubGridTreeConsts.SubGridLocalKeyMask), (uint)(CellAddress.Y & ~SubGridTreeConsts.SubGridLocalKeyMask)));
        }

        public bool LoadLeafSubGridSegment(IStorageProxy storageProxy,
                                           SubGridCellAddress cellAddress,
                                           bool loadLatestData,
                                           bool loadAllPasses,
                                           IServerLeafSubGrid SubGrid,
                                           ISubGridCellPassesDataSegment Segment)
        {
            bool FileLoaded;
            bool needToLoadLatestData, needToLoadAllPasses;

            needToLoadLatestData = loadLatestData && !Segment.HasLatestData;
            needToLoadAllPasses = loadAllPasses && !Segment.HasAllPasses;

            if (!needToLoadLatestData && !needToLoadAllPasses)
            {
                // Nothing more to do here
                return true;
            }

            // Ensure the appropriate storage is allocated
            if (needToLoadLatestData)
            {
                Segment.AllocateLatestPassGrid();
            }

            if (needToLoadAllPasses)
            {
                Segment.AllocateFullPassStacks();
            }

            if (!Segment.SegmentInfo.ExistsInPersistentStore)
            {
                // Nothing more to do here
                return true;
            }

            // Locate the segment file and load the data from it
            string FullFileName = GetLeafSubGridSegmentFullFileName(cellAddress, Segment.SegmentInfo);

            // Debug.Assert(false, "SubGrid.LoadFromFile not implemented (should usee direct serialisation from Ignite, or serialisation of dumb dinary data from same");
            // Load the cells into it from its file
            FileLoaded = SubGrid.LoadSegmentFromStorage(storageProxy, FullFileName, Segment, needToLoadLatestData, needToLoadAllPasses);

            if (!FileLoaded)
            {
                // Oops, something bad happened. Remove the segment from the list. Return failure to the caller.
                if (loadAllPasses)
                {
                    Segment.DeAllocateFullPassStacks();
                }

                if (loadLatestData)
                {
                    Segment.DeAllocateLatestPassGrid();
                }
                return false;
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
                // Load the cells into it from its file
                if (SubGrid.Dirty)
                {
                    Log.LogError("Leaf subgrid directory loads may not be performed while the subgrid is dirty. The information should be taken from the cache instead.");
                    return false;
                }

                // Ensure the appropriate storage is allocated
                if (loadAllPasses)
                {
                    SubGrid.AllocateLeafFullPassStacks();
                }

                if (loadLatestPasses)
                {
                    SubGrid.AllocateLeafLatestPassGrid();
                }

                FullFileName = GetLeafSubGridFullFileName(CellAddress);
                Result = SubGrid.LoadDirectoryFromFile(storageProxy, FullFileName);
            }
            finally
            {
              if (Result)
              {
                if (RecordSubgridFileReadingToLog)
                {
                  Log.LogDebug($"Subgrid file {FullFileName} read from persistant store containing {SubGrid.Directory.SegmentDirectory.Count} segments (Moniker: {SubGrid.Moniker()}");
                }
              }
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
