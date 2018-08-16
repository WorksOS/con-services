using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Utilities
{
    public static partial class SubGridUtilities
    {
        private static ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// GetOTGLeafSubGridCellIndex determines the local in-subgrid X/Y location of a
        /// cell given its absolute cell index in an on-the-ground leaf subgrid where the level of the subgrid is implicitly known
        /// to be the same as FOwner.Numlevels. Do not call this method for a subgrid that is not a leaf subgrid
        /// WARNING: This call assumes the cell index does lie within this subgrid
        /// and (currently) no range checking is performed to ensure this}
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <param name="SubGridX"></param>
        /// <param name="SubGridY"></param>
        public static void GetOTGLeafSubGridCellIndex(int CellX, int CellY, out byte SubGridX, out byte SubGridY)
        {
            SubGridX = (byte)(CellX & SubGridTree.SubGridLocalKeyMask);
            SubGridY = (byte)(CellY & SubGridTree.SubGridLocalKeyMask);

            //  Debug.Assert((SubGridX >=0) && (SubGridX < SubGridTree.SubGridTreeDimension) &
            //         (SubGridY >=0) && (SubGridY < SubGridTree.SubGridTreeDimension),
            //         "GetOTGLeafSubGridCellIndex given cell address out of bounds for this subgrid");
        }

        /// <summary>
        /// Locates the subgrid in the subgrid tree that contains the cell identified by CellX and CellY in the global 
        /// sub grid tree cell address space. The tree level for the subgrid returned is specified in Level.
        /// </summary>
        /// <param name="ForSubGridTree"></param>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <param name="Level"></param>
        /// <param name="LockToken"></param>
        /// <param name="LookInCacheOnly"></param>
        /// <param name="AcceptSpeculativeReadFailure"></param>
        /// <returns></returns>
        public static ISubGrid LocateSubGridContaining(IStorageProxy storageProxy,
                                 ServerSubGridTree ForSubGridTree,
                                 //const GridDataCache : TICDataStoreCache;
                                 uint CellX,
                                 uint CellY,
                                 byte Level,
                                 int LockToken,
                                 bool LookInCacheOnly,
                                 bool AcceptSpeculativeReadFailure)
        {
            IServerLeafSubGrid LeafSubGrid = null;
            // bool SubGridLockAcquired = false;
            // bool IntentionLockCounterWound = false;
            bool CreatedANewSubgrid = false;

            ISubGrid Result = null;

            /* TODO Locking semntics not defined for Ignite
            if (LockToken == -1)
            {
                SIGLogMessage.Publish(Nil, 'LocateSubGridContaining not given valid lock token. By definition, the requestor of a subgrid must lock that subgrid', slmcError);
                return null;
            }
            */

            try
            {
                Debug.Assert(ForSubGridTree != null, "Subgridtree null in LocateSubGridContaining");

                // Use a subgrid tree specific interlock to permit multiple threads accessing
                // different subgrid trees to operate concurrently
                ISubGrid SubGrid;
                lock (ForSubGridTree) //  ForSubGridTree.AcquireExternalAccessInterlock;
                {
                    try
                    {
                        // First check to see if the requested cell is present in a leaf subgrid
                        SubGrid = ForSubGridTree.LocateClosestSubGridContaining(CellX, CellY, Level);

                        if (SubGrid == null) // Something bad happened
                        {
                            Log.LogWarning($"Failed to locate subgrid at {CellX}:{CellY}, level {Level}, data model ID:{ForSubGridTree.ID}");
                            return null;
                        }

                        if (!SubGrid.IsLeafSubGrid() && !LookInCacheOnly && Level == ForSubGridTree.NumLevels)
                        {
                            // Create the leaf subgrid that will be used to read in the subgrid from the disk.
                            // In the case where the subgrid isn't present on the disk this reference will
                            // be destroyed
                            SubGrid = ForSubGridTree.ConstructPathToCell(CellX, CellY, Types.SubGridPathConstructionType.CreateLeaf);

                            if (SubGrid != null)
                            {
                                CreatedANewSubgrid = true;

                                /* TODO ... Locking semantics not defined for Ignite
                                SubGridLockAcquired = SubGrid.AcquireLock(LockToken);
                                if (!SubGridLockAcquired)
                                {
                                    SIGLogMessage.PublishNoODS(Nil, 'Failed to acquire subgrid lock in LocateSubGridContaining for newly created subgrid (Moniker: %2)', [SubGrid.Moniker], slmcError);
                                    return null;
                                }
                                */
                            }
                            else
                            {
                                //SIGLogMessage.PublishNoODS(Nil, 'Failed to create leaf subgrid in LocateSubGridContaining (Moniker: %2)', [SubGrid.Moniker], slmcError);
                                Log.LogError($"Failed to create leaf subgrid in LocateSubGridContaining (Moniker: {SubGrid.Moniker()})");

                                return null;
                            }
                        }
                    }
                    finally
                    {
                        /* TODO ... Locking semantics not defined for Ingite
                        if (SubGrid != null && !SubGridLockAcquired && SubGrid.IsLeafSubGrid())
                        {
                            // Don't lock the subgrid until the Subgridtree ExternalAccessInterlock is released
                            // as this will block all threads rather than just the thread that wants
                            // to lock the subgrid.
                            SubGrid.WindIntentionLock(); // Ensure that the subgrid will not be removed in the meantime before locking it

                            IntentionLockCounterWound = true;
                        }
                        */
                    }
                }

                if (SubGrid == null)  // Something bad happened
                {
                    Log.LogError($"Subgrid request result for {CellX}:{CellY} is null for undetermined reason.");
                    return null;
                }

                if (SubGrid.IsLeafSubGrid())
                {
                    LeafSubGrid = SubGrid as IServerLeafSubGrid;
                }

                if (LeafSubGrid == null)  // Something bad happened
                {
                    Log.LogError($"Subgrid request result for {CellX}:{CellY} is not a leaf subgrid, it is a {SubGrid.GetType().Name}.");
                    return null;
                }

                /* TODO... Locking semantics not defined yet when using Ignite
                if (LeafSubGrid != null)
                {
                    try
                    {
                        if (!SubGridLockAcquired)
                        {
                            SubGridLockAcquired = LeafSubGrid.AcquireLock(LockToken);
                        }

                        if (!SubGridLockAcquired)
                        {
                            SIGLogMessage.Publish(Nil, 'Failed to acquire subgrid lock in TICServer.LocateSubGridContaining', slmcError);
                            return null;
                        }
                    }
                    finally
                    {
                        if (IntentionLockCounterWound)
                        {
                            LeafSubGrid.UnwindIntentionLock();
                        }
                    }
                }

                if (!LeafSubGrid.Locked)
                {
                    SIGLogMessage.Publish(Nil, 'LocateSubGridContaining: SubGrid not locked after primary index lookup and/or subgrid creation', slmcAssert);
                    return null;
                }
                */

                if (!CreatedANewSubgrid)
                {
                    // If the returned subgrid is a leaf subgrid then it was already present in the
                    // cache. Check to see if the in-cache subgrid has the storage classes requested
                    // by the caller. If not, load the requested storage classes from disk
                    if (LeafSubGrid != null && LeafSubGrid.HasAllCellPasses())
                    {
                        if (LeafSubGrid.Dirty && (LeafSubGrid.LatestCellPassesOutOfDate || !LeafSubGrid.HasLatestData()))
                        {
                            //$IFNDEF STATIC_CELL_PASSES}
                            // Note: This only has relevance to the TAG file processor. PS Nodes
                            // (which use STATIC_CELL_PASSES), will never have this consideration
                            // as the latest pass information will have been written to the
                            // persistent data store before the subgrid was read.

                            // In this case we have a subgrid that is dirty, but does not have any latest data present for it.
                            // It is possible that this subgrid has not been persisted to disk yet (in fact it certainly has
                            // not in terms of the latest updates as it is marked as dirty), and it is also possible that
                            // there is no persistent disk store file for this subgrid yet (i.e.: it is in the process of
                            // being populated for the first time via TAG file processing). While the subgrid is present in
                            // the cache, it may not be present on disk and so attempts to retrieve the latest values from the
                            // disk store may fail dramatically if it is not present (dramatically includes the
                            // presumption that the subgrid should have been there and isn't which triggers the
                            // process of purging the defunct subgrid from the cache etc)
                            // So, in this particular case, the latest values will be calculated for the subgrid and
                            // returned to the caller.

                            LeafSubGrid.ComputeLatestPassInformation(true, storageProxy);

                            //{$ENDIF}
                        }
                    }

                    if (LookInCacheOnly)
                    {
                        if (SubGrid.Level == Level)
                        {
                            return SubGrid;
                        }

                        // If the returned subgrid is a leaf subgrid then it was already present in the
                        // cache. If the level of the returned subgrid matches the request level parameter
                        // then there is nothing more to do here.
                        if (SubGrid.IsLeafSubGrid() &&
                            ((LeafSubGrid.HaveSubgridDirectoryDetails || LeafSubGrid.Dirty) &&
                             LeafSubGrid.HasAllCellPasses() && LeafSubGrid.HasLatestData()) ||
                           (!SubGrid.IsLeafSubGrid() && SubGrid.Level == Level))
                        {
                            return SubGrid;
                        }
                    }
                }

                if (LeafSubGrid != null &&
                    ((!LeafSubGrid.HaveSubgridDirectoryDetails && !LeafSubGrid.Dirty) ||
                     !(LeafSubGrid.HasAllCellPasses() && LeafSubGrid.HasLatestData())))
                {
                    // The requested cell is either not present in the sub grid tree (cache),
                    // or it is residing on disk, and a newly created subgrid has been constructed
                    // to contain the data read from disk.

                    // The underlying assumption is that this method is only called if the caller knows
                    // that the subgrid exists in the subgrid tree (this is known via the subgrid existence
                    // map available to the caller). In cases where eventual consistency
                    // may mean that a subgrid was removed from the subgrid tree since the caller retrieved
                    // its copy of the subgrid existence map this function will fail gracefully with a null subgrid.
                    // The exception to this rule is the tag file processor service which may speculatively
                    // attempt to read a subgrid that doesn't exist.
                    // This is a different approach to desktop systems where the individual node subgrids
                    // contain mini existence maps for the subgrids below them.

                    if (ForSubGridTree.LoadLeafSubGrid(storageProxy,
                                           new SubGridCellAddress(CellX, CellY, false, false),
                                           true, true,
                                           LeafSubGrid))
                    {
                        // We've loaded it - get the reference to the new subgrid and return it
                        Result = LeafSubGrid;

                        /* TODO ... Locking semantics not defined for Ignite
                        if (!Result.Locked)
                        {
                            SIGLogMessage.PublishNoODS(Nil, 'LeafSubGrid not locked after loading by LoadLeafSubGrid', slmcAssert);
                        }
                        */
                    }
                    else
                    {
                        // The subgrid could not be loaded. This is likely due to it not ever existing
                        // in the model, or it may have been deleted. Failure here does not necessarily
                        // constitute evidence of corruption in the data model. Examination of the
                        // spatial existence map in conjunction with the requested subgrid index is
                        // required to determine that. Advise the caller nothing was read by sending back
                        // nil subgrid reference.

                        /*
                          DON'T REMOVE THE SUBGRID! Leave it there to be expired via the normal cache expiry to avoid
                          thread concurrency issues with other threads that may be attempting to acquire interlocks
                          on this subgrid.Any operations that add information to the newly created subgrid are responsible
                          for ensuring its persistency in the DB

                          // Find the location of the cell within the subgrid.
                          LeafSubGrid.Parent.GetSubGridCellIndex(CellX, CellY, ParentSubGridCellX, ParentSubGridCellY);

                          // Mark its entry as nil and free the subgrid.
                          (LeafSubGrid.Parent as TICServerSubGridTreeNode).Cells[ParentSubGridCellX, ParentSubGridCellY] := Nil;

                          FreeAndNil(LeafSubGrid);

                          Result:= Nil;

                          //            SIGLogMessage.Publish(Nil,
                          //                                  Format('Failed to read leaf subgrid %s (eventual consistency window?).', {SKIP}
                          //                                         [ForSubGridTree.GetLeafSubGridFullFileName(CellAddress)]),
                          //                                  slmcWarning);
                         */

                        if (AcceptSpeculativeReadFailure)
                        {
                            // Return the otherwise empty subgrid back to the caller and integrate
                            // it into the cache and lock it ready for use.
                            // SIGLogMessage.PublishNoODS(Nil, Format('Speculative read failure accepted for subgrid %s. Blank subgrid returned to caller.', [LeafSubGrid.Moniker]), slmcDebug);
                            Result = LeafSubGrid;
                        }
                        else
                        {
                            Log.LogWarning($"Failed to read leaf subgrid {LeafSubGrid.Moniker()} in model {ForSubGridTree.ID}. Failed subgrid is NOT removed from the tree");

                            // Empty the subgrid leaf based data to encourage it to be read on a secondary attempt
                            LeafSubGrid.DeAllocateLeafFullPassStacks();
                            LeafSubGrid.DeAllocateLeafLatestPassGrid();
                        }
                    }
                }

                // TODO Ignite special case - allow Dirty leaf subgrids to be returned
                if (Result == null)
                {
                    if (LeafSubGrid != null && LeafSubGrid.HaveSubgridDirectoryDetails && LeafSubGrid.Dirty && LeafSubGrid.HasAllCellPasses() && LeafSubGrid.HasLatestData())
                    {
                        Result = LeafSubGrid;
                    }
                }

                // IGNITE: Last gasp - if the subgrid is in memory and has direcotry details then just return it
                if (Result == null && LeafSubGrid != null && LeafSubGrid.HaveSubgridDirectoryDetails)
                {
                    Result = LeafSubGrid;
                }
            }
            finally
            {
                /* TODO ... locking and caching semantics
                if (Result != null && Result.IsLeafSubgrid)
                {
                    // Add the subgrid we just read to the cache manager, even if the read failed
                    if (!Result.PresentInCache)
                    {
                        if (!GridDataCache.AddSubGridToCache(Result as TSubGridTreeSubGridBase))
                        {
                            SIGLogMessage.PublishNoODS(Nil, Format('Failed to add subgrid %s to the cache', [Result.Moniker]), slmcAssert);
                        }
                    }

                    if (!SubGridLockAcquired)
                    {
                        SIGLogMessage.PublishNoODS(Nil, 'Failed to acquire subgrid lock in TICServer.LocateSubGridContaining', slmcError);
                    }

                    if (VLPDSvcLocations.VLPDPSNode_TouchSubgridAndSegmentsInCacheDuringAccessOperations)
                    {
                        GridDataCache.SubGridTouched(Result as TSubGridTreeSubGridBase);
                    }
                }
                else
                {
                    // Ensure that no ghost locks are created if we failed to read data for this subgrid
                    // and are passing back a null result to indicate the failure
                    if (Result == null && LeafSubGrid != null && LeafSubGrid.Locked)
                    {
                        LeafSubGrid.ReleaseLock(LockToken);
                    }
                }
                */
            }

            return Result;
        }
    }
}
