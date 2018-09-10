using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Iterators;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.Integrator
{
    /// <summary>
    /// Responsible for orchestrating integration of mini-sitemodels processed from one or
    /// more TAG files into another sitemodel, either a temporary/transient artifact of the ingest
    /// pipeline, or the persistent data store.
    /// </summary>
    public class SubGridIntegrator
    {
      private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridIntegrator>();

      /// <summary>
      /// The subgrid tree from which information is being integarted
      /// </summary>
      private IServerSubGridTree Source;

        /// <summary>
        /// Sitemodel representing the target sub grid tree
        /// </summary>
        private ISiteModel SiteModel;

        /// <summary>
        /// The subgrid tree the receives the subgrid information from the source subgrid tree
        /// </summary>
        private IServerSubGridTree Target;

        private IServerLeafSubGrid SourceSubGrid;
        private IServerLeafSubGrid TargetSubGrid;

        private Action<uint, uint> SubGridChangeNotifier;

        private IStorageProxy StorageProxy;

        /// <summary>
        ///  Default no-args constructor
        /// </summary>
        public SubGridIntegrator()
        {
        }

        /// <summary>
        /// Constructor the initialises state ready for integration
        /// </summary>
        /// <param name="source">The subgrid tree from which information is being integarted</param>
        /// <param name="siteModel">The sitemodel representing the target subgrid tree</param>
        /// <param name="target">The subgrid tree into which the data from the source subgrid tree is integrated</param>
        /// <param name="storageProxy">The storage proxy providing storage semantics for persisting integration results</param>
        public SubGridIntegrator(IServerSubGridTree source, ISiteModel siteModel, IServerSubGridTree target, IStorageProxy storageProxy) : this()
        {
            Source = source;
            SiteModel = siteModel;
            Target = target;
            StorageProxy = storageProxy;
        }

        private void IntegrateIntoIntermediaryGrid(ISubGridSegmentIterator SegmentIterator)
        {
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
                Log.LogCritical("No segment passes data in new segment");
                return;
            }

            // As the integration is into the intermediary grid, these segments do not
            // need to be involved with the cache, so instruct the iterator to not 'touch' them
            SegmentIterator.MarkReturnedSegmentsAsTouched = false;

            TargetSubGrid.Integrate(SourceSubGrid, SegmentIterator, true);
        }

        private bool IntegrateIntoLiveDatabase(SubGridSegmentIterator SegmentIterator)
        {
            // Note the fact that this subgrid will be changed and become dirty as a result
            // of the cell pass integration
            TargetSubGrid.Dirty = true;

            // As the integration is into the live database these segments do
            // need to be involved with the cache, so instruct the iterator to
            // 'touch' them
            SegmentIterator.MarkReturnedSegmentsAsTouched = true;

            TargetSubGrid.Integrate(SourceSubGrid, SegmentIterator, false);

            SubGridChangeNotifier?.Invoke(TargetSubGrid.OriginX, TargetSubGrid.OriginY);

            // Save the integrated state of the subgrid segments to allow Ignite to store & socialise the update
            // within the cluster. First ensure the latest pass information is calculated

            SubGridSegmentCleaver.PerformSegmentCleaving(SegmentIterator.StorageProxy, TargetSubGrid);

            TargetSubGrid.ComputeLatestPassInformation(true, StorageProxy);

            SubGridCellAddress SubGridOriginAddress = new SubGridCellAddress(TargetSubGrid.OriginX, TargetSubGrid.OriginY);
            foreach (var s in TargetSubGrid.Directory.SegmentDirectory)
            {
                s.Segment?.SaveToFile(StorageProxy, ServerSubGridTree.GetLeafSubGridSegmentFullFileName(SubGridOriginAddress, s), out FileSystemErrorStatus FSError);
            }

            // Save the changed subgrid directory to allow Ignite to store & socialise the update within the cluster
            if (TargetSubGrid.SaveDirectoryToFile(StorageProxy, ServerSubGridTree.GetLeafSubGridFullFileName(SubGridOriginAddress)))
            {
                // Successfully saving the subgrid directory information is the point at which this subgrid may be recognised to exist
                // in the sitemodel. Note this by including it within the SiteModel existance map

                SiteModel.ExistanceMap.SetCell(TargetSubGrid.OriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
                                               TargetSubGrid.OriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
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
            TargetSubGrid = LocateOrCreateSubgrid(Target, SourceSubGrid.OriginX, SourceSubGrid.OriginY);
            if (TargetSubGrid == null)
            {
                Log.LogError("Failed to locate or create subgrid in IntegrateIntoLiveGrid");
                return;
            }

            if (!IntegrateIntoLiveDatabase(SegmentIterator))
            {
                Log.LogError("Integration into live database failed");
            }
        }

        public bool IntegrateSubGridTree(SubGridTreeIntegrationMode integrationMode,
                                         Action<uint, uint> subGridChangeNotifier)
        {
            // Iterate over the subgrids in source and merge the cell passes from source
            // into the subgrids in this sub grid tree;

            SubGridTreeIterator Iterator = new SubGridTreeIterator(StorageProxy, false)
            {
                Grid = Source
            };

            SubGridSegmentIterator SegmentIterator = new SubGridSegmentIterator(null, StorageProxy)
            {
                IterationDirection = IterationDirection.Forwards
            };

            bool IntegratingIntoIntermediaryGrid = integrationMode == SubGridTreeIntegrationMode.UsingInMemoryTarget;
            SubGridChangeNotifier = subGridChangeNotifier;

            while (Iterator.MoveToNextSubGrid())
            {
                SourceSubGrid = Iterator.CurrentSubGrid as IServerLeafSubGrid;

                /*
                 // TODO: Terminated check for integration processing
                if (Terminated)
                {
                    // Service has been shutdown. Abort integration of changes and flag the
                    // operation as failed. The TAG file will be reprocessed when the service restarts
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

        /// <summary>
        /// Locates a subgrid in within this site model. If the subgrid cannot be found it will be created.
        /// If requested from an immutable grid context, the result of this call should be considered as an immutable
        /// copy of the requested data that is valid for the duration the request holds a reference to it. Updates
        /// to subgrids in this data model from ingest processing and other operations performed in mutable contexts
        /// can occur while this request is in process, but will not affected the immutable copy initially requested.
        /// If requested from a mutable grid context the calling context is responsible for ensuring serialised write access
        /// to the data elements being requested. 
        /// </summary>
        /// <param name="Grid"></param>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>
        public IServerLeafSubGrid LocateOrCreateSubgrid(IServerSubGridTree Grid, uint CellX, uint CellY)
        {
            IServerLeafSubGrid Result = SubGridUtilities.LocateSubGridContaining(
                                    StorageProxy,
                                    Grid,
                                    // DataStoreInstance.GridDataCache,
                                    CellX, CellY,
                                    Grid.NumLevels,
                                    false, true) as IServerLeafSubGrid;

            // Ensure the cells and segment directory are initialised if this is a new subgrid
            if (Result != null)
            {
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
                    Log.LogCritical($"LocateSubGridContaining returned a subgrid {Result.Moniker()} with no allocated cells");
                }
                else if (Result.Directory.SegmentDirectory.Count == 0)
                {
                    Log.LogCritical($"LocateSubGridContaining returned a subgrid {Result.Moniker()} with no segments in its directory");
                }
            }
            else
            {
                Log.LogCritical($"LocateSubGridContaining failed to return a subgrid (CellX/Y={CellX}/{CellY})");
            }

            return Result;
        }
    }
}
