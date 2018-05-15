using VSS.TRex.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Storage;

namespace VSS.TRex.SubGridTrees.Server
{
    public class ServerSubGridTree : SubGridTree, IServerSubGridTree
    {
        /// <summary>
        /// The SiteModel that this subgrid tree is holding information for
        /// </summary>
        private SiteModel SiteModelReference { get; set; }

        public ServerSubGridTree(SiteModel siteModel) :
            base(SubGridTreeLevels, DefaultCellSize,
                new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>())
        {
            // FSerialisedStream := Nil;
            // SerialisedCompressorStream := Nil;
            // FIsNewlyCreated := False;

            ID = siteModel.ID; // Ensure the ID of the subgrid tree matches the datamodel ID
            SiteModelReference = siteModel;

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
                                                               SubGridCellPassesDataSegmentInfo SegmentInfo)
        {
            // Work out the cell address of the origin cell in the appropriate leaf
            // subgrid. We use this cell position to derive the name of the file
            // containing the leaf subgrid data
            return SegmentInfo.FileName(new SubGridCellAddress((uint)(CellAddress.X & ~SubGridLocalKeyMask), (uint)(CellAddress.Y & ~SubGridLocalKeyMask)));
        }

        public bool LoadLeafSubGridSegment(IStorageProxy storageProxy,
                                           SubGridCellAddress cellAddress,
                                           bool loadLatestData,
                                           bool loadAllPasses,
                                           IServerLeafSubGrid SubGrid,
                                           SubGridCellPassesDataSegment Segment /*,
                                           SiteModel SiteModelReference*/)
        {
            string FullFileName;
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
            FullFileName = GetLeafSubGridSegmentFullFileName(cellAddress, Segment.SegmentInfo);

            // Debug.Assert(false, "SubGrid.LoadFromFile not implemented (should usee direct serialisation from Ignite, or serialisation of dumb dinary data from same");
            // Load the cells into it from its file
            FileLoaded = SubGrid.LoadSegmentFromStorage(storageProxy, FullFileName, Segment, needToLoadLatestData, needToLoadAllPasses /*, SiteModelReference*/);

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
            string FullFileName;

            bool Result;

            /* TODO ... Locking semantics not defined for Inite
            if (!SubGrid.Locked)
            {
             SIGLogMessage.PublishNoODS(Self, 'TICServerSubGridTree.LoadLeafSubGrid (Subgrid not locked on request for reading)', slmcAssert);
            return false;
            }
            */

            try
            {
                // Load the cells into it from its file
                if (SubGrid.Dirty)
                {
                    // TODO readd when logging available
                    //SIGLogMessage.PublishNoODS(Self, 'Leaf subgrid directory loads may not be performed while the subgrid is dirty. The information should be taken from the cache instead.', slmcError);
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
                /* TODO ...
                if (Result)
                {
                if (RecordSubgridFileReadingToLog)
                {
                    //SIGLogMessage.PublishNoODS(Self, 'Subgrid file %1 read from persistant store containing %2 segments (Moniker: %3)',      
                                                 [FullFileName, IntToStr(Subgrid.Directory.SegmentDirectory.Count), SubGrid.Moniker], slmcDebug);
                }
                }
                */
            }

            return Result;
        }

        public static string GetLeafSubGridFullFileName(SubGridCellAddress CellAddress)
        {
            // Work out the cell address of the origin cell in the appropriate leaf
            // subgrid. We use this cell position to derive the name of the file
            // containing the leaf subgrid data
            return ServerSubGridTreeLeaf.FileNameFromOriginPosition(new SubGridCellAddress((uint)(CellAddress.X & ~SubGridLocalKeyMask), (uint)(CellAddress.Y & ~SubGridLocalKeyMask)));
        }
    }
}
