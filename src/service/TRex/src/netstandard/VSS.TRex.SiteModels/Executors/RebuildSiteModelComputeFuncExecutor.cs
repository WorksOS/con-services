using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.SiteModels.Executors
{
  public class RebuildSiteModelComputeFuncExecutor
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<RebuildSiteModelComputeFuncExecutor>();

    /// <summary>
    /// The response object available for inspection once the Executor has completed processing
    /// </summary>
    public RebuildSiteModelRequestResponse Response { get; } = new RebuildSiteModelRequestResponse { RebuildResult = RebuildSiteModelResult.OK };

    private readonly RebuildSiteModelRequestArgument _rebuildSiteModelRequestArgument;
    private readonly ISiteModel _siteModel;

    /// <summary>
    /// Constructor for the renderer accepting all parameters necessary for its operation
    /// </summary>
    public RebuildSiteModelComputeFuncExecutor(RebuildSiteModelRequestArgument arg)
    {
      _rebuildSiteModelRequestArgument = arg;
      if (arg != null)
        _siteModel = DIContext.Obtain<ISiteModels>().GetSiteModelRaw(arg.ProjectID);

      if (_siteModel == null)
      {
        Response.RebuildResult = RebuildSiteModelResult.UnableToLocateSiteModel;
        return;
      }

      _siteModel.SetStorageRepresentationToSupply(StorageMutability.Mutable);

      // Rebuilding is only supported in the context of the mutable grid for coordination
      if (_siteModel.PrimaryStorageProxy.Mutability != StorageMutability.Mutable
      || _siteModel.PrimaryStorageProxy.ImmutableProxy == null)
      {
        throw new TRexException($"Rebulding is only supported in the context of the mutable grid for coordination");
      }
    }

    /// <summary>
    /// Executor that implements rebuilding of the project
    /// <returns></returns>
    public async Task<bool> ExecuteAsync()
    {
      _log.LogInformation($"Performing Rebuild for DataModel:{_rebuildSiteModelRequestArgument?.ProjectID}, with deletion selectivity {_rebuildSiteModelRequestArgument.DeletionSelectivity}");

      if (Response.RebuildResult != RebuildSiteModelResult.OK)
      {
        _log.LogInformation($"Rebuilding site model {_siteModel.ID}: Initial execution response state not OK ({Response.RebuildResult}) - aborting request");
        return false;
      }

      //***********************************
      // TODO: LOTS OF STUFF HAPPENING HERE
      /*
       * 1. Perform project deletion step and record result from that. Abort if there is an issue.
       * 2. The request saves progress state and metadata into a special cache to monitor progress
       * 
       * <- At this point the request may return to the caller all steps after this point execute asynchronously outside the context of this executor ->
       * 
       * 2. Scan S3 Tag file archive bucket looking for entries in the form "[/Projects]/{projectUid}[/Machines]/{machineUid}/*.tag
       * 3. Sort all Tag file based on date, according to the same logic present in the TagFileSubmitter utility in TRex
       * 4. Submit all discovered, sorted, tag files using the SubmitTagFile request. Each request defines the project Uid (from the 
       *    request) and the {machineUid} from the key for the Tag file. As each batch of TAG files are submitted this progress is updated in the
       *    tracking metadata to aid in restarting the process if it is interrupted
       * 5. Once all tag files are submitted then the tracking state is update to reflect the submission phase is complete
       * 6. Determination of rebuild completion or progress estimation is not yet defined, but this could be managed by tagging TAG files 
       *    in the processing queue with state that causes the TAG file processor to emit message based on on the tracking indication state on that file.
       * 7. An adidtional progress tracking request will be implemented to return the progress/tracking state metadata to allow a client to 
       *    track and display this to a user if required.
       */
      //***********************************

      _log.LogInformation($"Deleting site model {_siteModel.ID} with selectivity = {_rebuildSiteModelRequestArgument.DeletionSelectivity}: Complete");

      Response.RebuildResult = RebuildSiteModelResult.OK;
      return true;
    }
  }
}

