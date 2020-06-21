using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Machines;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Requests;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SiteModels.Executors
{
  /// <summary>
  /// Generates a patch of sub grids from a wider query
  /// </summary>
  public class DeleteSiteModelComputeFuncExecutor
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<DeleteSiteModelComputeFuncExecutor>();

    /// <summary>
    /// The response object available for inspection once the Executor has completed processing
    /// </summary>
    public DeleteSiteModelRequestResponse Response { get; } = new DeleteSiteModelRequestResponse { Result = DeleteSiteModelResult.OK};

    private readonly DeleteSiteModelRequestArgument _deleteSiteModelRequestArgument;
    private readonly ISiteModel _siteModel;

    /// <summary>
    /// Constructor for the renderer accepting all parameters necessary for its operation
    /// </summary>
    public DeleteSiteModelComputeFuncExecutor(DeleteSiteModelRequestArgument arg)
    {
      _deleteSiteModelRequestArgument = arg;
      if (arg != null)
        _siteModel = DIContext.Obtain<ISiteModels>().GetSiteModelRaw(arg.ProjectID);

      if (_siteModel == null)
      {
        Response.Result = DeleteSiteModelResult.UnableToLocateSiteModel;
        return;
      }

      _siteModel.SetStorageRepresentationToSupply(StorageMutability.Mutable);

      // Deletion is only supported in the context of the mutable grid for coordination
      if (_siteModel.PrimaryStorageProxy.Mutability != StorageMutability.Mutable
      || _siteModel.PrimaryStorageProxy.ImmutableProxy == null)
      {
        throw new TRexException($"Deletion is only supported in the context of the mutable grid for coordination");
      }
    }

    /// <summary>
    /// Removes all sub grid directory and segment information in the site model
    /// </summary>
    /// <param name="storageProxy"></param>
    /// <returns></returns>
    private bool RemoveAllSpatialCellPassData(IStorageProxy storageProxy)
    {
      var subGridRemovalSuccessful = true;

      _siteModel.ExistenceMap.ScanAllSetBitsAsSubGridAddresses(address =>
      {
        var filename = ServerSubGridTree.GetLeafSubGridFullFileName(address);
        var leaf = new ServerSubGridTreeLeaf(_siteModel.Grid, null, SubGridTreeConsts.SubGridTreeLevels, StorageMutability.Mutable)
        {
          OriginX = address.X,
          OriginY = address.Y
        };
        leaf.LoadDirectoryFromFile(storageProxy, filename);

        leaf.Directory.SegmentDirectory.ForEach(segmentInfo =>
        {
          var segmentFilename = ServerSubGridTree.GetLeafSubGridSegmentFullFileName(leaf.OriginAsCellAddress(), segmentInfo);
          if (!leaf.RemoveSegmentFromStorage(storageProxy, segmentFilename, segmentInfo))
          {
            _log.LogError($"Failed to remove segment {segmentFilename} sub grid: {leaf.Moniker()}");
            subGridRemovalSuccessful = false;
          }
        });

        if (!leaf.RemoveDirectoryFromStorage(storageProxy, filename))
        {
          _log.LogError($"Failed to remove directory for server sub grid leaf: {leaf.Moniker()}");
          subGridRemovalSuccessful = false;
        }
      });

      return subGridRemovalSuccessful;
    }

    /// <summary>
    /// Removes all the event elements for all machines within the site model
    /// </summary>
    /// <param name="storageProxy"></param>
    private void RemovalAllMachineEvents(IStorageProxy storageProxy, bool exceptOverrideEvents)
    {
      foreach (var machine in _siteModel.Machines)
      {
        _siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex]?.RemoveMachineEventsFromPersistentStore(storageProxy, exceptOverrideEvents);
      }
    }

    /// <summary>
    /// Executor that implements deletion of the project
    /// </summary>
    public async Task<bool> ExecuteAsync()
    {
      _log.LogInformation($"Performing Execute for DataModel:{_deleteSiteModelRequestArgument?.ProjectID}, with selectivity {_deleteSiteModelRequestArgument.Selectivity}");

      if (Response.Result != DeleteSiteModelResult.OK)
      {
        _log.LogInformation($"Deleting site model {_siteModel.ID}: Initial execution response state not OK ({Response.Result}) - aborting request");
        return Response.Result == DeleteSiteModelResult.UnableToLocateSiteModel;
      }

      _log.LogInformation($"Deleting site model {_siteModel.ID}: Initiating");

      if (!_siteModel.IsMarkedForDeletion)
      {
        // Instruct the site model to mark itself for deletion. Once completed, the site model will no longer be returned 
        // by requests to SiteModels.GetSiteModel()
        _siteModel.MarkForDeletion();
      }

      // Obtain a private storage proxy to perform deletion operations in
      var storageProxy = DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage();

      // Begin executing the deletion process. The aspects to be deleted are, in order:
      // All sub grids containing processed cell data
      // Event lists for all machines in the site model
      // All machines for the site model
      // All proofing runs for the site model
      // Site model machine designs index for the site model
      // Site model machine designs list for the site model
      // The designs store for the site model
      // The surveyed surfaces store for the site model
      // The alignments store for the site model
      // The coordinate system for the site model
      // The existence map for the spatial sub grids [this is done late to aid fault recovery]
      // The base metadata for the site model
      // The persistent store for the site model itself

      //***************************************************
      // Begin removal of data elements from the site model
      //***************************************************

      if (_deleteSiteModelRequestArgument.Selectivity.HasFlag(DeleteSiteModelSelectivity.TagFileDerivedData))
      {
        // Remove all spatial data
        if (!RemoveAllSpatialCellPassData(storageProxy))
        {
          Response.Result = DeleteSiteModelResult.FailedToRemoveSubGrids;
          return false;
        }

        // Remove all event data
        RemovalAllMachineEvents(storageProxy, _deleteSiteModelRequestArgument.Selectivity != DeleteSiteModelSelectivity.All);

        // Remove the machines list for the site model. If the override events are being maintained, then 
        // recreate machines with jsut basic information
        if (_deleteSiteModelRequestArgument.Selectivity == DeleteSiteModelSelectivity.All)
        {
          MachinesList.RemoveFromPersistentStore(_siteModel.ID, storageProxy);
        }
        else
        {
          _siteModel.Machines.ClearTAGFileStateData(storageProxy);
        }

        // Remove the site model machine designs list for the site model
        SiteModelMachineDesignList.RemoveFromPersistentStore(_siteModel.ID, storageProxy);

        // Remove the proofing run list for the site model
        SiteProofingRunList.RemoveFromPersistentStore(_siteModel.ID, storageProxy);

        // Remove the site model production designs list from the site model
        SiteModelDesignList.RemoveFromPersistentStoreStatic(_siteModel.ID, storageProxy);
      }

      if (_deleteSiteModelRequestArgument.Selectivity.HasFlag(DeleteSiteModelSelectivity.Designs))
      {
        // **********************************************
        // Remove the list of designs from the site model
        // **********************************************
        if (!DIContext.Obtain<IDesignManager>().Remove(_siteModel.ID, storageProxy))
        {
          _log.LogInformation($"Deleting site model {_siteModel.ID}: Failed to remove site designs");
          Response.Result = DeleteSiteModelResult.FailedToRemoveSiteDesigns;
          return false;
        }
      }

      if (_deleteSiteModelRequestArgument.Selectivity.HasFlag(DeleteSiteModelSelectivity.SurveyedSurfaces))
      {
        // ********************************************************
        // Remove the list of surveyed surfaces from the site model
        // ********************************************************

        if (!DIContext.Obtain<ISurveyedSurfaceManager>().Remove(_siteModel.ID, storageProxy))
        {
          _log.LogInformation($"Deleting site model {_siteModel.ID}: Failed to remove surveyed surfaces");
          Response.Result = DeleteSiteModelResult.FailedToRemoveSurveyedSurfaces;
          return false;
        }
      }

      if (_deleteSiteModelRequestArgument.Selectivity.HasFlag(DeleteSiteModelSelectivity.Alignments))
      {
        // *************************************************
        // Remove the list of alignments from the site model
        // *************************************************

        if (!DIContext.Obtain<IAlignmentManager>().Remove(_siteModel.ID, storageProxy))
        {
          _log.LogInformation($"Deleting site model {_siteModel.ID}: Failed to remove alignments");
          Response.Result = DeleteSiteModelResult.FailedToRemoveAlignments;
          return false;
        }
      }

      if (_deleteSiteModelRequestArgument.Selectivity.HasFlag(DeleteSiteModelSelectivity.CoordinateSystem))
      {
        // ************************************************
        // Remove the coordinate system from the site model
        // ************************************************

        // TODO: CSIB add/remove/read support could be wrapped into a more central location as a manager (currently controlled here, Add CSIB activity and SiteModel class)
        if (storageProxy.RemoveStreamFromPersistentStore(_siteModel.ID, FileSystemStreamType.CoordinateSystemCSIB, CoordinateSystemConsts.CoordinateSystemCSIBStorageKeyName) != FileSystemErrorStatus.OK)
        {
          _log.LogInformation($"Deleting site model {_siteModel.ID}: Failed to remove CSIB");
          Response.Result = DeleteSiteModelResult.FailedToRemoveCSIB;
          return false;
        }
      }

      // ****************************************************************************************************************
      // Commit all assembled deletion stages before removing existence map and metadata. This helps recovery from errors
      // in the commit process resulting in partial deletion.
      // ****************************************************************************************************************

      if (!storageProxy.Commit(out var numDeleted, out _, out _))
      {
        Response.Result = DeleteSiteModelResult.FailedToCommitPrimaryElementRemoval;
        _log.LogInformation($"Deleting site model {_siteModel.ID}: Failed to commit primary element removal");
        return false;
      }

      Response.NumRemovedElements = numDeleted;

      _log.LogInformation($"Deleting site model {_siteModel.ID}: Primary commit removed {numDeleted} elements");

      if (_deleteSiteModelRequestArgument.Selectivity.HasFlag(DeleteSiteModelSelectivity.SiteModelMetadata))
      {
        // Remove the site model meta data entry for the site model
        // TODO: This is a non transacted operation and could be facaded with the storage proxy cache pattern as an internal implementation concern (this is only a mutable grid activity)
        DIContext.Obtain<ISiteModelMetadataManager>().Remove(_siteModel.ID);
        Response.NumRemovedElements++;
      }

      if (_deleteSiteModelRequestArgument.Selectivity.HasFlag(DeleteSiteModelSelectivity.TagFileDerivedData))
      {
        //*************************
        // Remove the existence map
        //*************************
        _siteModel.RemoveProductionDataExistenceMapFromStorage(storageProxy);

        if (!storageProxy.Commit())
        {
          Response.Result = DeleteSiteModelResult.FailedToCommitExistenceMapRemoval;
          _log.LogInformation($"Deleting site model {_siteModel.ID}: Failed to commit existence map removal");
          return false;
        }

        Response.NumRemovedElements++;
      }

      if (_deleteSiteModelRequestArgument.Selectivity.HasFlag(DeleteSiteModelSelectivity.SiteModel))
      {
        //************************************************************
        // Remove the site model persistent storage for the site model
        //************************************************************
        var productionDataXmlResult = _siteModel.RemoveMetadataFromPersistentStore(storageProxy);
        if (!productionDataXmlResult)
        {
          Response.Result = DeleteSiteModelResult.FailedToRemoveProjectMetadata;
          _log.LogError($"Deleting site model {_siteModel.ID}: Unable to remove site model persistent store");
          return false;
        }

        if (!storageProxy.Commit(out _, out _, out _))
        {
          _log.LogInformation($"Deleting site model {_siteModel.ID}: Failed to commit site model metadata removal");
          return false;
        }
      }

      // If the delete operation had partial selectivity enabled, then remove the delete flag from the site model
      if (_deleteSiteModelRequestArgument.Selectivity != DeleteSiteModelSelectivity.All)
      {
        // Instruct the site model to remove the deletion mark against it to allow operations to continue as normal
        _siteModel.RemovedMarkForDeletion();
      }

      Response.NumRemovedElements++;

      _log.LogInformation($"Deleting site model {_siteModel.ID} with selectivity = {_deleteSiteModelRequestArgument.Selectivity}: Complete");

      Response.Result = DeleteSiteModelResult.OK;
      return true;
    }
  }
}
