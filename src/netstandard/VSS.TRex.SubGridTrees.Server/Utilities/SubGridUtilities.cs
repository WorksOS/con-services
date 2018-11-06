using Microsoft.Extensions.Logging;
using System.Diagnostics;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Utilities
{
    public static partial class SubGridUtilities
    {
        private static ILogger Log = Logging.Logger.CreateLogger("SubGridUtilities");

        /// <summary>
        /// GetOTGLeafSubGridCellIndex determines the local in-subgrid X/Y location of a
        /// cell given its absolute cell index in an on-the-ground leaf subgrid where the level of the subgrid is implicitly known
        /// to be the same as FOwner.NumLevels. Do not call this method for a subgrid that is not a leaf subgrid
        /// WARNING: This call assumes the cell index does lie within this subgrid
        /// and (currently) no range checking is performed to ensure this}
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <param name="subGridX"></param>
        /// <param name="subGridY"></param>
        public static void GetOTGLeafSubGridCellIndex(int cellX, int cellY, out byte subGridX, out byte subGridY)
        {
            subGridX = (byte)(cellX & SubGridTreeConsts.SubGridLocalKeyMask);
            subGridY = (byte)(cellY & SubGridTreeConsts.SubGridLocalKeyMask);

            //  Debug.Assert((SubGridX >=0) && (SubGridX < SubGridTreeConsts.SubGridTreeDimension) &
            //         (SubGridY >=0) && (SubGridY < SubGridTreeConsts.SubGridTreeDimension),
            //         "GetOTGLeafSubGridCellIndex given cell address out of bounds for this subgrid");
        }

      /// <summary>
      /// Locates the subgrid in the subgrid tree that contains the cell identified by CellX and CellY in the global 
      /// sub grid tree cell address space. The tree level for the subgrid returned is specified in Level.
      /// </summary>
      /// <param name="storageProxy"></param>
      /// <param name="forSubGridTree"></param>
      /// <param name="cellX"></param>
      /// <param name="cellY"></param>
      /// <param name="level"></param>
      /// <param name="lookInCacheOnly"></param>
      /// <param name="acceptSpeculativeReadFailure"></param>
      /// <returns></returns>
      public static ISubGrid LocateSubGridContaining(IStorageProxy storageProxy,
                                 IServerSubGridTree forSubGridTree,
                                 //const GridDataCache : TICDataStoreCache;
                                 uint cellX,
                                 uint cellY,
                                 byte level,
                                 bool lookInCacheOnly,
                                 bool acceptSpeculativeReadFailure)
        {
            IServerLeafSubGrid leafSubGrid = null;
            bool createdANewSubgrid = false;

            ISubGrid result = null;

            try
            {
                Debug.Assert(forSubGridTree != null, "Subgrid tree null in LocateSubGridContaining");

                // Note: Subgrid tree specific interlocks are no longer used. The tree now internally
                // manages fine grained locks across structurally mutating activities such as node/leaf
                // subgrid addition and reading content from the persistent store.

              // First check to see if the requested cell is present in a leaf subgrid
                ISubGrid subGrid = forSubGridTree.LocateClosestSubGridContaining(cellX, cellY, level);

                if (subGrid == null) // Something bad happened
                {
                    Log.LogWarning($"Failed to locate subgrid at {cellX}:{cellY}, level {level}, data model ID:{forSubGridTree.ID}");
                    return null;
                }

                if (!subGrid.IsLeafSubGrid() && !lookInCacheOnly && level == forSubGridTree.NumLevels)
                {
                    // Create the leaf subgrid that will be used to read in the subgrid from the disk.
                    // In the case where the subgrid isn't present on the disk this reference will
                    // be destroyed
                    subGrid = forSubGridTree.ConstructPathToCell(cellX, cellY, Types.SubGridPathConstructionType.CreateLeaf);

                    if (subGrid != null)
                    {
                        createdANewSubgrid = true;
                    }
                    else
                    {
                        Log.LogError($"Failed to create leaf subgrid in LocateSubGridContaining for subgrid at {cellX}x{cellY}");
                        return null;
                    }
                }

                if (subGrid.IsLeafSubGrid())
                {
                    leafSubGrid = subGrid as IServerLeafSubGrid;
                }

                if (leafSubGrid == null)  // Something bad happened
                {
                    Log.LogError($"Subgrid request result for {cellX}:{cellY} is not a leaf subgrid, it is a {subGrid.GetType().Name}.");
                    return null;
                }

                if (!createdANewSubgrid)
                {
                    if (lookInCacheOnly)
                    {
                        if (subGrid.Level == level)
                        {
                            return subGrid;
                        }

                        // If the returned subgrid is a leaf subgrid then it was already present in the
                        // cache. If the level of the returned subgrid matches the request level parameter
                        // then there is nothing more to do here.
                        if (subGrid.IsLeafSubGrid() &&
                            ((leafSubGrid.HaveSubgridDirectoryDetails || leafSubGrid.Dirty) &&
                             leafSubGrid.HasAllCellPasses() && leafSubGrid.HasLatestData()) ||
                           (!subGrid.IsLeafSubGrid() && subGrid.Level == level))
                        {
                            return subGrid;
                        }
                    }
                }

                if ((!leafSubGrid.HaveSubgridDirectoryDetails && !leafSubGrid.Dirty) ||
                    !(leafSubGrid.HasAllCellPasses() && leafSubGrid.HasLatestData()))
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

                    if (forSubGridTree.LoadLeafSubGrid(storageProxy,
                                           new SubGridCellAddress(cellX, cellY),
                                           true, true,
                                           leafSubGrid))
                    {
                        // We've loaded it - get the reference to the new subgrid and return it
                        result = leafSubGrid;
                    }
                    else
                    {
                        // The subgrid could not be loaded. This is likely due to it not ever existing
                        // in the model, or it may have been deleted. Failure here does not necessarily
                        // constitute evidence of corruption in the data model. Examination of the
                        // spatial existence map in conjunction with the requested subgrid index is
                        // required to determine that. Advise the caller nothing was read by sending back
                        // a null subgrid reference.
                        // The failed subgrid is not proactively deleted and will remain so the normal cache
                        // expiry mechanism can remove it in its normal operations

                        if (acceptSpeculativeReadFailure)
                        {
                            // Return the otherwise empty subgrid back to the caller and integrate it into the cache 
                            Log.LogDebug($"Speculative read failure accepted for subgrid {leafSubGrid.Moniker()}. Blank subgrid returned to caller.");
                            result = leafSubGrid;
                        }
                        else
                        {
                            Log.LogWarning($"Failed to read leaf subgrid {leafSubGrid.Moniker()} in model {forSubGridTree.ID}. Failed subgrid is NOT removed from the tree");

                            // Empty the subgrid leaf based data to encourage it to be read on a secondary attempt
                            leafSubGrid.DeAllocateLeafFullPassStacks();
                            leafSubGrid.DeAllocateLeafLatestPassGrid();
                        }
                    }
                }

                // TODO Ignite special case - allow Dirty leaf subgrids to be returned
                if (result == null)
                {
                    if (leafSubGrid.HaveSubgridDirectoryDetails && leafSubGrid.Dirty && leafSubGrid.HasAllCellPasses() && leafSubGrid.HasLatestData())
                    {
                        result = leafSubGrid;
                    }
                }

                // IGNITE: Last gasp - if the subgrid is in memory and has directory details then just return it
                if (result == null && leafSubGrid.HaveSubgridDirectoryDetails)
                {
                    result = leafSubGrid;
                }
            }
            finally
            {
                if (result != null && result.IsLeafSubGrid())
                {
                  /* TODO ... caching semantics
                      // Add the subgrid we just read to the cache manager, even if the read failed
                      if (!Result.PresentInCache)
                      {
                          if (!GridDataCache.AddSubGridToCache(Result as TSubGridTreeSubGridBase))
                              SIGLogMessage.PublishNoODS(Nil, Format('Failed to add subgrid %s to the cache', [Result.Moniker]), ...);
                      }
        
                      if (VLPDSvcLocations.VLPDPSNode_TouchSubgridAndSegmentsInCacheDuringAccessOperations)
                          GridDataCache.SubGridTouched(Result as TSubGridTreeSubGridBase);
                      */
                  }
            }

        return result;
        }
    }
}
