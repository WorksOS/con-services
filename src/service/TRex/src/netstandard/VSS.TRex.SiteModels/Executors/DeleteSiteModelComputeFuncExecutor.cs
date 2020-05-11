using System.Threading.Tasks;
using Amazon.S3.Model.Internal.MarshallTransformations;
using Microsoft.Extensions.Logging;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Machines;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SiteModels.Executors
{
  /// <summary>
  /// Generates a patch of sub grids from a wider query
  /// </summary>
  public class DeleteSiteModelComputeFuncExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<DeleteSiteModelComputeFuncExecutor>();

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
        _siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(arg.ProjectID);

      // Deletion is only supported in the context of the mutable grid for coordination
      if (_siteModel.PrimaryStorageProxy.Mutability != StorageMutability.Mutable)
      {
        Response.Result = DeleteSiteModelResult.RequestNotMadeToMutableGrid;
      }
    }

    /// <summary>
    /// Executor that implements requesting and rendering grid information to create the grid rows
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ExecuteAsync()
    {
      Log.LogInformation($"Performing Execute for DataModel:{_deleteSiteModelRequestArgument.ProjectID}");

      if (Response.Result != DeleteSiteModelResult.OK)
      {
        Log.LogInformation($"Deleting site model {_siteModel.ID}: Initial execution response state not OK ({Response.Result}) - aborting request");
        return false;
      }

      Log.LogInformation($"Deleting site model {_siteModel.ID}: Initiating");

      // Instruct the site model to mark itself for deletion. Once completed, the site model will no longer be returned 
      // by requests to SiteModels.GetSiteModel()
      _siteModel.MarkForDeletion();

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

      //*********************************************************
      // Remove all sub grids (directory and segment information)
      //*********************************************************

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
            Log.LogError($"Failed to remove segment {segmentFilename} sub grid: {leaf.Moniker()}");
            subGridRemovalSuccessful = false;
          }
        });

        if (!leaf.RemoveDirectoryFromStorage(storageProxy, filename))
        {
          Log.LogError($"Failed to remove directory for server sub grid leaf: {leaf.Moniker()}");
          subGridRemovalSuccessful = false;
        }
      });

      if (!subGridRemovalSuccessful)
      {
        Response.Result = DeleteSiteModelResult.FailedToRemoveSubGrids;
        return false;
      }

      // *******************************************************
      // Remove all events lists for machine from the site model
      // *******************************************************
      foreach (var machine in _siteModel.Machines)
      {
        _siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex]?.RemoveMachineEventsFromPersistentStore(storageProxy);
      }

      // Remove the machines list for the site model
      MachinesList.RemoveFromPersistentStore(_siteModel.ID, storageProxy);

      // Remove the site model machine designs list for the site model
      SiteModelMachineDesignList.RemoveFromPersistentStore(_siteModel.ID, storageProxy);

      // Remove the proofing run list for the site model
      SiteProofingRunList.RemoveFromPersistentStore(_siteModel.ID, storageProxy);

      // Remove the site model production designs list from the site model
      SiteModelDesignList.RemoveFromPersistentStoreStatic(_siteModel.ID, storageProxy);

      // Remove the list of designs from the site model
      DIContext.Obtain<IDesignManager>().Remove(_siteModel.ID, storageProxy);

      // Remove the list of surveyed surfaces from the site model
      DIContext.Obtain<ISurveyedSurfaceManager>().Remove(_siteModel.ID, storageProxy);

      // Remove the list of alignments from the site model
      DIContext.Obtain<IAlignmentManager>().Remove(_siteModel.ID, storageProxy);

      // Remove the coordinate system from the site model
      // TODO: CSIB add/remove/read support could be wrapped into a more central location (currently here, Add CSIB activity and SiteModel class)
      storageProxy.RemoveStreamFromPersistentStore(_siteModel.ID, FileSystemStreamType.CoordinateSystemCSIB, CoordinateSystemConsts.CoordinateSystemCSIBStorageKeyName);

      // Commit all assembled deletion stages before removing existence map and metadata. This helps recovery from errors
      // in the commit process resulting in partial deletion.

      if (!storageProxy.Commit(out var numDeleted, out _, out _))
      {
        Response.Result = DeleteSiteModelResult.FailedToCommitExistenceMapRemoval;
        Log.LogInformation($"Deleting site model {_siteModel.ID}: Failed to commit primary element removal");
        return false;
      }

      Response.NumRemovedElements = numDeleted;

      Log.LogInformation($"Deleting site model {_siteModel.ID}: Primary commit removed {numDeleted} elements");

      // Remove the site model meta data entry for the site model
      // TODO: This is a non transacted operation and should be facaded with the storage proxy cache pattern as an internal implementation concern
      DIContext.Obtain<ISiteModelMetadataManager>().Remove(_siteModel.ID);
      Response.NumRemovedElements++;

      //*************************
      // Remove the existence map
      //*************************
      _siteModel.RemoveProductionDataExistenceMapFromStorage(storageProxy);

      if (!storageProxy.Commit(out _, out _, out _))
      {
        Response.Result = DeleteSiteModelResult.FailedToCommitExistenceMapRemoval;
        Log.LogInformation($"Deleting site model {_siteModel.ID}: Failed to commit existence map removal");
        return false;
      }

      Response.NumRemovedElements++;

      //************************************************************
      // Remove the site model persistent storage for the site model
      //************************************************************
      var productionDataXmlResult = _siteModel.RemoveMetadataFromPersistentStore(storageProxy);
      if (!productionDataXmlResult)
      {
        Response.Result = DeleteSiteModelResult.FailedToRemoveProjectMetadata;
        Log.LogError($"Deleting site model {_siteModel.ID}: Unable to remove site model persistent store");
        return false;
      }

      if (!storageProxy.Commit(out _, out _, out _))
      {
        Log.LogInformation($"Deleting site model {_siteModel.ID}: Failed to commit site model metadata removal");
        return false;
      }

      Response.NumRemovedElements++;

      Log.LogInformation($"Deleting site model {_siteModel.ID}: Complete");

      Response.Result = DeleteSiteModelResult.OK;
      return true;
    }
  }
}
