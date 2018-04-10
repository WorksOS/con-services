using System;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators;
using VSS.VisionLink.Raptor.SubGridTrees.Types;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.Integrator
{
    public class SubGridIntegrator
    {
        private ServerSubGridTree Source;
        private SiteModel SiteModel;
        private ServerSubGridTree Target;

        private ISubGrid SourceSubGrid;
        private IServerLeafSubGrid TargetSubGrid;

        //const Persistor : TSVOICDeferredPersistor;
        private Action<uint, uint> SubGridChangeNotifier;

        private IStorageProxy StorageProxy;  //IStorageProxy[] SpatialStorageProxy = null;

        public SubGridIntegrator()
        {
        }

        public SubGridIntegrator(ServerSubGridTree source,
        SiteModel siteModel,
        ServerSubGridTree target,
        IStorageProxy storageProxy /*IStorageProxy[] spatialStorageProxy*/) : this()
        {
            Source = source;
            SiteModel = siteModel;
            Target = target;
            StorageProxy = storageProxy;  //SpatialStorageProxy = spatialStorageProxy;
        }

        private void IntegrateIntoIntermediaryGrid(SubGridSegmentIterator SegmentIterator)
        {
            // Note: There is no need to lock this subgrid as we are working
            // in a private in-memory subgrid
            TargetSubGrid = Target.ConstructPathToCell(SourceSubGrid.OriginX,
                                                       SourceSubGrid.OriginY,
                                                       SubGridPathConstructionType.CreateLeaf) as IServerLeafSubGrid;

            TargetSubGrid.AllocateLeafFullPassStacks();

            // If the node is brand new (ie: it does not have any cell passes committed to it yet)
            // then create and select the default segment
            if (TargetSubGrid.Directory.SegmentDirectory.Count == 0)
            {
                TargetSubGrid.Cells.SelectSegment(DateTime.MinValue);
                TargetSubGrid.Cells.PassesData[0].AllocateFullPassStacks();
            }

            if (TargetSubGrid.Cells.PassesData[0].PassesData == null)
            {
                // TODO readd when logging available
                // SIGLogMessage.PublishNoODS(Self, 'No segment passes data in new segment', slmcAssert);
                return;
            }

            // As the integration is into the intermediary grid, these segments do not
            // need to be involved with the cache, so instruct the iterator to not
            // 'touch' them
            SegmentIterator.MarkReturnedSegmentsAsTouched = false;

            TargetSubGrid.Integrate(SourceSubGrid as ServerSubGridTreeLeaf, SegmentIterator, true);
        }

        private bool IntegrateIntoLiveDatabase(SubGridSegmentIterator SegmentIterator)
        {
            // Note the fact that this subgrid will be changed and become dirty as a result
            // of the cell pass integration
            TargetSubGrid.Dirty = true; // MarkAsDirty;

            //    TargetSubGrid.CachedMemoryUpdateTrackingActive = true;
            //    try
            // As the integration is into the live database these segments do
            // need to be involved with the cache, so instruct the iterator to
            // 'touch' them
            SegmentIterator.MarkReturnedSegmentsAsTouched = true;

            TargetSubGrid.Integrate(SourceSubGrid as ServerSubGridTreeLeaf, SegmentIterator, true);
            //    finally
            //      TargetSubGrid.CachedMemoryUpdateTrackingActive = false;
            //    end;

            /*
            // TODO: Resolve when IFOPT C+ equivalent understood
            {$IFOPT C+}
            if (!TargetSubGrid.Locked)
            {
              // TODO readd when logging available
              // SIGLogMessage.PublishNoODS(self, 'Target subgrid not locked after integration operation', slmcAssert);
            return false;
        }
    {$ENDIF}
    */

            SubGridChangeNotifier?.Invoke(TargetSubGrid.OriginX, TargetSubGrid.OriginY);

            /* TODO...
            if (Persistor != null)
            {
                if (TargetSubGrid.Cells.PassesData.Count == 0)
                {
                    // TODO add whenlogging available
                    //SIGLogMessage.PublishNoODS(Self, 'Committing a subgrid with no subgrid segments to the persistent store in TSVOICAggregatedDataIntegrator.IntegrateSubGridTree', slmcException); { SKIP}
                    return false;
                }

                Target.SaveLeafSubGrid(TargetSubGrid, Persistor);
            }
            */

            // Save the integrated state of the subgrid segments to allow Ignite to store & socialise the update
            // within the cluster. No cleaving is performed yet... First ensure the latest pass information is calculated

            SubGridCellAddress SubGridOriginAddress = new SubGridCellAddress(TargetSubGrid.OriginX, TargetSubGrid.OriginY);

            // Replaced by argument in method signature
            //IStorageProxy storageProxy = SpatialStorageProxy[SubGridOriginAddress.ToSpatialDivisionDescriptor(RaptorConfig.numSpatialProcessingDivisions)];

            (TargetSubGrid as ServerSubGridTreeLeaf).ComputeLatestPassInformation(true);

            foreach (var s in (TargetSubGrid as ServerSubGridTreeLeaf).Directory.SegmentDirectory)
            {
                s.Segment?.SaveToFile(StorageProxy, ServerSubGridTree.GetLeafSubGridSegmentFullFileName(SubGridOriginAddress, s), out FileSystemErrorStatus FSError);
            }

            // Save the changed subgrid directory to allow Ignite to store & socialise the update
            // within the cluster
            if ((TargetSubGrid as ServerSubGridTreeLeaf).SaveDirectoryToFile(StorageProxy, ServerSubGridTree.GetLeafSubGridFullFileName(SubGridOriginAddress)))
            {
                // Successfully saving the subgrid directory information is the point at which this subgrid may be recognised to exist
                // in the sitemodel. Note this by including it within the SiteModel existance map

                SiteModel.ExistanceMap.SetCell(TargetSubGrid.OriginX >> SubGridTree.SubGridIndexBitsPerLevel,
                                               TargetSubGrid.OriginY >> SubGridTree.SubGridIndexBitsPerLevel,
                                               true);
            }
            else
            {
                // Failure to save a piece of data aborts the entire integration
                return false;
            }

            // Finally, mark the source subgrid as not being dirty. We need to do this to allow
            // the subgrid to permit its destruction as all changes have been merged into the target.
            SourceSubGrid.AllChangesMigrated();

            return true;
        }

        private void IntegrateIntoLiveGrid(SubGridSegmentIterator SegmentIterator)
        {
            TargetSubGrid = LocateOrCreateAndLockSubgrid(Target,
                SourceSubGrid.OriginX, SourceSubGrid.OriginY,
                0 /* TODO:... FAggregatedDataIntegratorLockToken */);
            if (TargetSubGrid == null)
            {
                //TODO add when logging available
                //SIGLogMessage.PublishNoODS(Self, 'Failed to locate subgrid in TSVOICAggregatedDataIntegrator.IntegrateIntoLiveGrid', slmcAssert); { SKIP}
                return;
            }

            try
            {
                if (!IntegrateIntoLiveDatabase(SegmentIterator))
                {
                    return;
                }
            }
            finally
            {
                TargetSubGrid.ReleaseLock(0 /* TODO ... FAggregatedDataIntegratorLockToken */);
            }
        }

        public bool IntegrateSubGridTree(//IServerSubGridTree source,
                                         //SiteModel siteModel,
                                         //IServerSubGridTree Target,
                                         SubGridTreeIntegrationMode integrationMode,
                                         //const Persistor : TSVOICDeferredPersistor;
                                         Action<uint, uint> subGridChangeNotifier // : TICSubGridTreeGridCellChangedNotificationEvent
                                        )
        {
            SubGridTreeIterator Iterator;
            SubGridSegmentIterator SegmentIterator;
            bool IntegratingIntoIntermediaryGrid;
            //int WaitCnt;
            //PrevInMemorySize: Integer;
            //PostFullRecalcInMemorySize: Integer;

            // If the cache is stressed (saturated and has overage > 1Mb) then suspend processing
            // of the integrated nodes until the cache is no longer stressed
            /*  WaitCnt := 0;
              with DataStoreInstance.GridDataCache do
                While (IsFull or IsSaturated) and not CacheIsIrreduciblySmall and not FShuttingDown do
                  begin
                    inc(WaitCnt);

                    if IsSaturated then
                      SIGLogMessage.PublishNoODS(Self, 'Aggregated Data Integrator: Cache is saturated, suspending processing for %1 msec, Wait count = %2', {SKIP}
                                                 [IntToStr(VLPDSvcLocations.WaitForSaturatedCachePeriodMS), Inttostr(WaitCnt)], slmcMessage)
                    else
                      SIGLogMessage.PublishNoODS(Self, Format('Aggregation Task Process --> Integrate aggregated cell passes into model --> Cache full with overage, sleeping  %d Msec. WaitCount:%d',
                                                              [VLPDSvcLocations.WaitForSaturatedCachePeriodMS, WaitCnt]), slmcWarning);

                    Sleep(VLPDSvcLocations.WaitForSaturatedCachePeriodMS);
            end;

              if FShuttingDown then
                Exit;
            */

            // Iterate over the subgrids in source and merge the cell passes from source
            // into the subgrids in this sub grid tree;

            Iterator = new SubGridTreeIterator(StorageProxy, false);

            SegmentIterator = new SubGridSegmentIterator(null)
            {
                IterationDirection = IterationDirection.Forwards
            };
            IntegratingIntoIntermediaryGrid = integrationMode == SubGridTreeIntegrationMode.UsingInMemoryTarget;

            Iterator.Grid = Source;

            SubGridChangeNotifier = subGridChangeNotifier;

            // The acquiring and releasing of the server lock in the code below may initially seem strange
            // At this point we already have the database modification lock, and really just need
            // to ensure that the subgrid may be selected by the iterator without risk of
            // it being victimised in the grid cache management layer. So, we ensure the lock is active
            // until we are sure of having an specific lock on the subgrid itself, at which point
            // the subgrid may be freely modified. The lock is reinstated prior to getting the
            // next subgrid from the iterator and is finally released for the last time once all the
            // subgrids have been returned by the iterator

            while (Iterator.MoveToNextSubGrid())
            {
                SourceSubGrid = Iterator.CurrentSubGrid;
                SubGridCellAddress SubGridOriginAddress = new SubGridCellAddress(SourceSubGrid.OriginX, SourceSubGrid.OriginY);

                /*
                 * // TODO...
                if (Terminated)
                {
                    // Service has been shutdown. Abort integration of changes and flag the
                    // operation as failed. The TAG file will be reprocessed when the service
                    // restarts
                    return false;
                }
                */

                // Locate a matching subgrid in this tree. If there is none, then create it
                // and assign the subgrid from the iterator to it. If there is one, process
                // the cell pass stacks merging the two together
                if (IntegratingIntoIntermediaryGrid)
                {
                    IntegrateIntoIntermediaryGrid(SegmentIterator);
                }
                else
                {
                    IntegrateIntoLiveGrid(SegmentIterator);
                }

            }

            return true;
        }

        public ServerSubGridTreeLeaf LocateOrCreateAndLockSubgrid(ServerSubGridTree Grid,
                                                                 uint CellX, uint CellY,
                                                                 int LockToken)
        {
            ServerSubGridTreeLeaf Result = SubGridUtilities.LocateSubGridContaining(
                                    Grid,
                                    // DataStoreInstance.GridDataCache,
                                    CellX, CellY,
                                    Grid.NumLevels,
                                    LockToken, false, true) as ServerSubGridTreeLeaf;

            // Ensure the cells and segment directory are initialised if this is a new subgrid
            if (Result != null)
            {
                /* TODO ... Locking semantics not defined for Ignite
                if (LockToken != -1 && !Result.Locked)
                {
                    SIGLogMessage.PublishNoODS(Self, Format('LocateSubGridContaining returned a subgrid %s that was not locked as requested', [Result.Moniker]), slmcAssert);
                    return Result;
                }
                */

                // By definition, any new subgrid we create here is dirty, even if we
                // ultimately do not add any cell passes to it. This is necessary to
                // enourage even otherwise empty subgrids to be persisted to disk if
                // they have been created, but never populated with cell passes.
                // The subgrid persistency layer may implement a rule that no empty
                // subgrids are saved to disk if this becomes an issue...
                Result.Dirty = true;

                Result.AllocateLeafFullPassStacks();
                if (Result.Directory.SegmentDirectory.Count == 0)
                {
                    Result.Cells.SelectSegment(DateTime.MinValue);
                }

                if (Result.Cells == null)
                {
                    // TODO readd when logging available
                    // SIGLogMessage.PublishNoODS(Self, Format('LocateSubGridContaining returned a subgrid %s with no allocated cells', [Result.Moniker]), slmcAssert);
                    return Result;
                }

                if (Result.Directory.SegmentDirectory.Count == 0)
                {
                    // TODO add when logging available
                    //SIGLogMessage.PublishNoODS(Self, Format('LocateSubGridContaining returned a subgrid %s with no segments in its directory', [Result.Moniker]), slmcAssert);
                    return Result;
                }

                // Removed the cache memory size out of date check in favour of the cache manager thread performing its recalculations
                //      if Result.CachedMemorySizeOutOfDate then
                //        DataStoreInstance.GridDataCache.ApplyCacheSizeDelta(Result, -(Result.CachedInMemorySize - Result.InMemorySize));
            }
            else
            {
                // TODO add when logging available
                //SIGLogMessage.PublishNoODS(Self, Format('LocateSubGridContaining failed to return a subgrid (CellX/Y=%d/%d)', [CellX, CellY]), slmcAssert);
            }

            return Result;
        }
    }
}
