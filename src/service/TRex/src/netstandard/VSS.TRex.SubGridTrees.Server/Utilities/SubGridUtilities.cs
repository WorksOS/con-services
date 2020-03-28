using Microsoft.Extensions.Logging;
using VSS.Serilog.Extensions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Utilities
{
    public static class SubGridUtilities
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger("SubGridUtilities");

        /// <summary>
        /// GetOTGLeafSubGridCellIndex determines the local in-sub grid X/Y location of a
        /// cell given its absolute cell index in an on-the-ground leaf sub grid where the level of the sub grid is implicitly known
        /// to be the same as FOwner.NumLevels. Do not call this method for a sub grid that is not a leaf sub grid
        /// WARNING: This call assumes the cell index does lie within this sub grid
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
        }

      /// <summary>
      /// Locates the sub grid in the sub grid tree that contains the cell identified by CellX and CellY in the global 
      /// sub grid tree cell address space. The tree level for the sub grid returned is specified in Level.
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
                                 int cellX,
                                 int cellY,
                                 byte level,
                                 bool lookInCacheOnly,
                                 bool acceptSpeculativeReadFailure)
        {
            IServerLeafSubGrid leafSubGrid = null;
            var createdANewSubGrid = false;

            ISubGrid result = null;

            try
            {
                if (forSubGridTree == null)
                {
                  throw new TRexSubGridProcessingException($"Sub grid tree null in {nameof(LocateSubGridContaining)}");
                }
              
                // Note: Sub grid tree specific interlocks are no longer used. The tree now internally
                // manages fine grained locks across structurally mutating activities such as node/leaf
                // sub grid addition and reading content from the persistent store.

                // First check to see if the requested cell is present in a leaf sub grid
                var subGrid = forSubGridTree.LocateClosestSubGridContaining(cellX, cellY, level);

                if (subGrid == null) // Something bad happened
                {
                    Log.LogWarning($"Failed to locate sub grid at {cellX}:{cellY}, level {level}, data model ID:{forSubGridTree.ID}");
                    return null;
                }

                if (!subGrid.IsLeafSubGrid() && !lookInCacheOnly && level == forSubGridTree.NumLevels)
                {
                    if (forSubGridTree.CachingStrategy == ServerSubGridTreeCachingStrategy.CacheSubGridsInTree)
                    { 
                        // Create the leaf sub grid that will be used to read in the sub grid from the disk.
                        // In the case where the sub grid isn't present on the disk this reference will be destroyed
                        subGrid = forSubGridTree.ConstructPathToCell(cellX, cellY, Types.SubGridPathConstructionType.CreateLeaf);
                    }
                    else if (forSubGridTree.CachingStrategy == ServerSubGridTreeCachingStrategy.CacheSubGridsInIgniteGridCache)
                    {
                        // Create the leaf sub grid without constructing elements in the grid to represent it other than the
                        // path in the tree to the parent of the sub grid.
                        // Note: Setting owner and parent relationship from the sub grid to the tree in this fashion permits
                        // business logic in th sub grid that require knowledge of it parent and owner relationships to function
                        // correctly while not including a reference to the sub grid from the tree.
                        subGrid = forSubGridTree.CreateNewSubGrid(forSubGridTree.NumLevels);
                        subGrid.SetAbsoluteOriginPosition(cellX & ~SubGridTreeConsts.SubGridLocalKeyMask, cellY & ~SubGridTreeConsts.SubGridLocalKeyMask);
                        subGrid.Owner = forSubGridTree;
                        subGrid.Parent = forSubGridTree.ConstructPathToCell(cellX, cellY, Types.SubGridPathConstructionType.CreatePathToLeaf);
                    }

                    if (subGrid != null)
                    {
                        createdANewSubGrid = true;
                    }
                    else
                    {
                        Log.LogError($"Failed to create leaf sub grid in LocateSubGridContaining for sub grid at {cellX}x{cellY}");
                        return null;
                    }
                }

                if (subGrid.IsLeafSubGrid())
                    leafSubGrid = subGrid as IServerLeafSubGrid;

                if (leafSubGrid == null)  // Something bad happened
                {
                    Log.LogError($"Sub grid request result for {cellX}:{cellY} is not a leaf sub grid, it is a {subGrid.GetType().Name}.");
                    return null;
                }

                if (!createdANewSubGrid)
                {
                    if (lookInCacheOnly)
                    {
                        if (subGrid.Level == level)
                            return subGrid;

                        // If the returned sub grid is a leaf sub grid then it was already present in the
                        // cache. If the level of the returned sub grid matches the request level parameter
                        // then there is nothing more to do here.
                        if (subGrid.IsLeafSubGrid() &&
                            ((leafSubGrid.HasSubGridDirectoryDetails || leafSubGrid.Dirty) &&
                             leafSubGrid.HasAllCellPasses() && leafSubGrid.HasLatestData()) ||
                           (!subGrid.IsLeafSubGrid() && subGrid.Level == level))
                        {
                            return subGrid;
                        }
                    }
                }

                if ((!leafSubGrid.HasSubGridDirectoryDetails && !leafSubGrid.Dirty) ||
                    !(leafSubGrid.HasAllCellPasses() && leafSubGrid.HasLatestData()))
                {
                    // The requested cell is either not present in the sub grid tree (cache),
                    // or it is residing on disk, and a newly created sub grid has been constructed
                    // to contain the data read from disk.

                    // The underlying assumption is that this method is only called if the caller knows
                    // that the sub grid exists in the sub grid tree (this is known via the sub grid existence
                    // map available to the caller). In cases where eventual consistency
                    // may mean that a sub grid was removed from the sub grid tree since the caller retrieved
                    // its copy of the sub grid existence map this function will fail gracefully with a null sub grid.
                    // The exception to this rule is the tag file processor service which may speculatively
                    // attempt to read a sub grid that doesn't exist.
                    // This is a different approach to desktop systems where the individual node sub grids
                    // contain mini existence maps for the sub grids below them.

                    if (forSubGridTree.LoadLeafSubGrid(storageProxy,
                                           new SubGridCellAddress(cellX, cellY),
                                           true, true,
                                           leafSubGrid))
                    {
                        // We've loaded it - get the reference to the new sub grid and return it
                        result = leafSubGrid;
                    }
                    else
                    {
                        // The sub grid could not be loaded. This is likely due to it not ever existing
                        // in the model, or it may have been deleted. Failure here does not necessarily
                        // constitute evidence of corruption in the data model. Examination of the
                        // spatial existence map in conjunction with the requested sub grid index is
                        // required to determine that. Advise the caller nothing was read by sending back
                        // a null sub grid reference.
                        // The failed sub grid is not proactively deleted and will remain so the normal cache
                        // expiry mechanism can remove it in its normal operations

                        if (acceptSpeculativeReadFailure)
                        {
                            // Return the otherwise empty sub grid back to the caller and integrate it into the cache 
                            if (Log.IsTraceEnabled())
                            {
                              Log.LogTrace($"Speculative read failure accepted for sub grid {leafSubGrid.Moniker()}. Blank sub grid returned to caller.");
                            }

                            result = leafSubGrid;
                        }
                        else
                        {
                            Log.LogWarning($"Failed to read leaf sub grid {leafSubGrid.Moniker()} in model {forSubGridTree.ID}. Failed sub grid is NOT removed from the tree");

                            // Empty the sub grid leaf based data to encourage it to be read on a secondary attempt
                            leafSubGrid.DeAllocateLeafFullPassStacks();
                            leafSubGrid.DeAllocateLeafLatestPassGrid();
                        }
                    }
                }

                // Ignite special case - allow Dirty leaf sub grids to be returned
                if (result == null)
                {
                    if (leafSubGrid.HasSubGridDirectoryDetails && leafSubGrid.Dirty && leafSubGrid.HasAllCellPasses() && leafSubGrid.HasLatestData())
                        result = leafSubGrid;
                }

                // IGNITE: Last gasp - if the sub grid is in memory and has directory details then just return it
                if (result == null && leafSubGrid.HasSubGridDirectoryDetails)
                    result = leafSubGrid;
            }
            finally
            {
                if (result != null && result.IsLeafSubGrid())
                {
                  /* Raptor implementation. In TRex this is stored in the Ignite cache

                      // Add the sub grid we just read to the cache manager, even if the read failed
                      if (!Result.PresentInCache)
                      {
                          if (!GridDataCache.AddSubGridToCache(Result as TSubGridTreeSubGridBase))
                              SIGLogMessage.PublishNoODS(Nil, Format('Failed to add sub grid %s to the cache', [Result.Moniker]), ...);
                      }
        
                      if (TouchSubGridAndSegmentsInCacheDuringAccessOperations)
                          GridDataCache.SubGridTouched(Result as TSubGridTreeSubGridBase);
                      */
                }
            }

            return result;
        }
    }
}
