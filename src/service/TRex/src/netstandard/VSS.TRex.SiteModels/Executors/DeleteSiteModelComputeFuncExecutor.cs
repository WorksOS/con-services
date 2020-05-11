using System.Threading.Tasks;
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
        return false;
      }

      // Instruct the site model to mark itself for deletion. Once completed, the site model will no longer be returned 
      // by requests to SiteModels.GetSiteModel()
      _siteModel.MarkForDeletion();

      // Obtain a private storage proxy to perform deletion operations in
      var storageProxy = DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage();

      // Begin executing the deletion process. The aspects to be deleted are, in order:
      // All sub grids containing processed cell data
      // The existence map for the spatial sub grids
      // Event lists for all machines in the site model
      // All machines for the site model
      // All proofing runs for the site model
      // Site model machine designs index for the site model
      // Site model machine designs list for the site model
      // The designs store for the site model
      // The surveyed surfaces store for the site model
      // The alignments store for the site model
      // The coordinate system for the site model
      // The base metadata for the site model
      // The persistent store for the site model itself

      //*************************************************
      // Remove the segments for all sub grid directories
      // Todo: The implementation could be refactored into ServerSubGridTree

      var subGridSegmentRemovalSuccessful = true;
      _siteModel.Grid.ScanAllSubGrids(leaf =>
      {
        if (leaf is IServerLeafSubGrid serverLeaf)
        {
          serverLeaf.Directory.SegmentDirectory.ForEach(segmentInfo =>
          {
            var filename = ServerSubGridTree.GetLeafSubGridSegmentFullFileName(leaf.OriginAsCellAddress(), segmentInfo);
            serverLeaf.RemoveSegmentFromStorage(storageProxy, filename, segmentInfo);
          });
        }
        else
        {
          Log.LogError($"Failed to obtain server sub grid leaf from ISubGrid: {leaf.Moniker()}");
          subGridSegmentRemovalSuccessful = false;
          return false;
        }

        return true;
      });

      if (!subGridSegmentRemovalSuccessful)
      {
        return false;
      }

      // ****************************************
      // Remove the directories for all sub grids
      // Todo: The implementation could be refactored into ServerSubGridTree

      var subGridDirectoryRemovalSuccessful = true;
      _siteModel.Grid.ScanAllSubGrids(leaf =>
      {
        if (leaf is IServerLeafSubGrid serverLeaf)
        {
          var filename = ServerSubGridTree.GetLeafSubGridFullFileName(leaf.OriginAsCellAddress());
          serverLeaf.RemoveDirectoryFromStorage(storageProxy, filename);
        }
        else
        {
          Log.LogError($"Failed to obtain server sub grid leaf from ISubGrid: {leaf.Moniker()}");
          subGridDirectoryRemovalSuccessful = false;
          return false;
        }

        return true;
      });

      if (!subGridDirectoryRemovalSuccessful)
      {
        return false;
      }

      // ************************
      // Remove the existence map
      _siteModel.RemoveProductionDataExistenceMapFromStorage(storageProxy);

      // Remove all events lists for machine from the site model
      foreach (var machine in _siteModel.Machines)
      {
        _siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].RemoveMachineEventsFromPersistentStore(storageProxy);
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
      // TODO: CSIB add/remove/read support could be wrapped into a more central location (currently here, Add CSIB and SiteModel class)
      storageProxy.RemoveStreamFromPersistentStore(_siteModel.ID, FileSystemStreamType.CoordinateSystemCSIB, CoordinateSystemConsts.CoordinateSystemCSIBStorageKeyName);

      // Remove the site model meta data entry for the site model
      // TODO: This is a non transacted operation and should be facaded with the storage proxy cache pattern
      DIContext.Obtain<ISiteModelMetadataManager>().Remove(_siteModel.ID);

      // Remove the site model persistent storage for the site model
      var productionDataXmlResult = _siteModel.RemoveMetadataFromPersistentStore(storageProxy);
      if (!productionDataXmlResult)
      {
        Log.LogError($"Unable to remove site model persistent store");
      }

      Response.Result = DeleteSiteModelResult.OK;
      return true;
    }
  }
}
