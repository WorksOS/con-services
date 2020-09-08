using System;
using Apache.Ignite.Core;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.SiteModels.GridFabric.Listeners;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.SiteModels.Executors
{
  public class RebuildSiteModelTAGNotifier : IRebuildSiteModelTAGNotifier
  {
    private IIgnite _ignite;
    private IIgnite Ignite => _ignite ??= DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Mutable);

    private string _roleAttribute;

    public void TAGFileProcessed(Guid projectUid, IProcessTAGFileResponseItem[] processedItems)
    {
      _roleAttribute ??= $"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{ServerRoles.PROJECT_REBUILDER_ROLE}";

      var notification = new RebuildSiteModelTAGNotifierEvent
      {
        ProjectUid = projectUid,
        ResponseItems = processedItems
      };

      Ignite.GetCluster().ForAttribute(_roleAttribute, "True").GetMessaging().Send(notification, RebuildSiteModelTAGNotifierListener.SITE_MODEL_REBUILDER_TAG_FILE_PROCESSED_EVENT_TOPIC_NAME);
    }
  }
}
