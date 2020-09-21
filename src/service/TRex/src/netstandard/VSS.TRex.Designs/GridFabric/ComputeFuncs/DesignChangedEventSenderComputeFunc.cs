using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Designs.GridFabric.ComputeFuncs
{
  public class DesignChangedEventSenderComputeFunc : IComputeFunc<IDesignChangedEvent, IDesignChangedEventSenderResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<DesignChangedEventSenderComputeFunc>();

    public IDesignChangedEventSenderResponse Invoke(IDesignChangedEvent arg)
    {
      var localNodeId = Guid.Empty;

      try
      {
        var siteModels = DIContext.Obtain<ISiteModels>();
        if (siteModels == null)
        {
          _log.LogError("No ISiteModels instance available from DIContext to send design change message to");

          return new DesignChangedEventSenderResponse {Success = false, NodeUid = localNodeId};
        }

        localNodeId = DIContext.ObtainRequired<ITRexGridFactory>().Grid(siteModels.PrimaryMutability).GetCluster().GetLocalNode().Id;

        var designFiles = DIContext.ObtainOptional<IDesignFiles>();

        if (designFiles == null)
        {
          // No cache, leave early...
          return new DesignChangedEventSenderResponse {Success = true, NodeUid = localNodeId};
        }

        var siteModel = siteModels.GetSiteModel(arg.SiteModelUid);
        if (siteModel == null)
        {
          _log.LogWarning($"No site model found for ID {arg.SiteModelUid}");
          return new DesignChangedEventSenderResponse {Success = true, NodeUid = localNodeId};
        }

        designFiles.DesignChangedEventHandler(arg.DesignUid, siteModel, arg.FileType);

        return new DesignChangedEventSenderResponse {Success = true, NodeUid = localNodeId};
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception occurred processing site model attributes changed event");

        return new DesignChangedEventSenderResponse {Success = false, NodeUid = localNodeId};
      }
      finally
      {
        _log.LogInformation(
          $"Completed handling notification of design changed for Site:{arg.SiteModelUid}, Design:{arg.DesignUid}, DesignRemoved:{arg.DesignRemoved}, ImportedFileType:{arg.FileType}");
      }
    }
  }
}
