using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
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
      // The surveyed surfaces store for the site model
      // The alignments store for the site model
      // All proofing runs for the site model
      // Site model machine designs index for the site model
      // Site model machine designs list for the site model
      // The designs store for the site model
      // The coordinate system for the site model
      // The base metadata for the site model
      // The persistent store for the site model itself

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
