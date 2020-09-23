using System;
using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.SiteModels.Interfaces.Listeners;

namespace VSS.TRex.SiteModels.GridFabric.ComputeFuncs
{
  public class RebuildSiteModelTAGNotifierListenerComputeFunc : IComputeFunc<IRebuildSiteModelTAGNotifierEvent, IRebuildSiteModelTAGNotifierEventSenderResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<RebuildSiteModelTAGNotifierListenerComputeFunc>();

    public bool PerformRebuilderNotificationAction(IRebuildSiteModelTAGNotifierEvent message)
    {
      var responseCount = 0;
      try
      {
        responseCount = message.ResponseItems?.Length ?? 0;
        _log.LogInformation($"Received notification of TAG file processing for {message.ProjectUid}, #TAG files = {responseCount}");

        if (responseCount > 0)
        {
          // Tell the rebuilder manager instance about the notification
          var rebuilderManager = DIContext.Obtain<ISiteModelRebuilderManager>();
          if (rebuilderManager != null)
          {
            Task.Run(() =>
            {
              try
              {
                rebuilderManager.TAGFileProcessed(message.ProjectUid, message.ResponseItems);
              }
              catch (Exception e)
              {
                _log.LogError(e, "Exception handling TAG file processed notification event");
              }
            });
          }
          else
          {
            _log.LogError("No ISiteModelRebuilderManager instance available from DIContext to send TAG file processing notification to");
            return true; // Stay subscribed
          }
        }
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception handling TAG file processed notification event");
        return true;  // Stay subscribed
      }
      finally
      {
        _log.LogInformation($"Completed handling of notification of TAG file processing for {message?.ProjectUid}, #TAG files = {responseCount}");
      }

      return true;
    }

    public IRebuildSiteModelTAGNotifierEventSenderResponse Invoke(IRebuildSiteModelTAGNotifierEvent arg)
    {
      var localNodeId = Guid.Empty;

      try
      {
        var siteModels = DIContext.Obtain<ISiteModels>();
        if (siteModels == null)
        {
          _log.LogError("No ISiteModels instance available from DIContext to send attributes change message to");

          return new RebuildSiteModelTAGNotifierEventSenderResponse { Success = false, NodeUid = localNodeId };
        }

        localNodeId = DIContext.ObtainRequired<ITRexGridFactory>().Grid(siteModels.PrimaryMutability).GetCluster().GetLocalNode().Id;

        PerformRebuilderNotificationAction(arg);

        return new RebuildSiteModelTAGNotifierEventSenderResponse
        {
          Success = true,
          NodeUid = localNodeId
        };
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception occurred processing site model attributes changed event");

        return new RebuildSiteModelTAGNotifierEventSenderResponse
        {
          Success = false,
          NodeUid = localNodeId
        };
      }
    }
  }
}
