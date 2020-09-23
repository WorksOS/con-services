using System;
using System.Linq;
using Apache.Ignite.Core;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Extensions;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.SiteModels.GridFabric.ComputeFuncs;
using VSS.TRex.SiteModels.GridFabric.Listeners;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.SiteModels.Executors
{
  public class RebuildSiteModelTAGNotifier : IRebuildSiteModelTAGNotifier
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<RebuildSiteModelTAGNotifierListenerComputeFunc>();

    private IIgnite _ignite;
    private IIgnite Ignite => _ignite ??= DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Mutable);

    private readonly string _roleAttribute = $"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{ServerRoles.PROJECT_REBUILDER_ROLE}";

    private readonly RebuildSiteModelTAGNotifierListenerComputeFunc _func = new RebuildSiteModelTAGNotifierListenerComputeFunc();

    public void TAGFileProcessed(Guid projectUid, IProcessTAGFileResponseItem[] processedItems)
    {
      var notification = new RebuildSiteModelTAGNotifierEvent {ProjectUid = projectUid, ResponseItems = processedItems};

      //Ignite.GetCluster().ForAttribute(_roleAttribute, "True").GetMessaging().Send(notification, RebuildSiteModelTAGNotifierListener.SITE_MODEL_REBUILDER_TAG_FILE_PROCESSED_EVENT_TOPIC_NAME);

      var compute = Ignite.GetCluster().ForAttribute(_roleAttribute, "True").GetCompute();

      if (compute == null)
      {
        _log.LogWarning("No compute projection acquired, exiting");
        return;
      }

      var result = compute.Broadcast(_func, notification);

      if (result.Count != 1)
      {
        _log.LogWarning($"Single response not received, encountered {result.Count}");
        result.ForEach((x, i) => _log.LogWarning($"Response {i}: NodeUid: {x.NodeUid} Success: {x.Success}"));
        return;
      }

      if (!result.First().Success)
      {
        _log.LogWarning($"Rebuild site model TAG file notification event failed, nodeUid = {result.First().NodeUid}");
      }
    }
  }
}
