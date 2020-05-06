using System;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Storage.Models;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Designs.GridFabric.Events
{

  /// <summary>
  /// Responsible for sending a notification that the state of a design has changed
  /// By definition, all server and client nodes should react to this message
  /// </summary>
  public class DesignChangedEventSender : IDesignChangedEventSender
  {

    private const string MessageTopicName = "DesignStateChangedEvents";

    /// <summary>
    /// Notify all interested nodes in the immutable grid a site model has changed attributes
    /// </summary>
    public void DesignStateChanged(DesignNotificationGridMutability targetGrids,  Guid siteModelUid, Guid designUid, ImportedFileType fileType, bool designRemoved = false)
    {
      var gridFactory = DIContext.Obtain<ITRexGridFactory>();
      var evt = new DesignChangedEvent
      {
        SiteModelUid = siteModelUid,
        DesignUid = designUid,
        FileType = fileType,
        DesignRemoved = designRemoved
      };

      if ((targetGrids & DesignNotificationGridMutability.NotifyImmutable) != 0)
        gridFactory.Grid(StorageMutability.Immutable).GetMessaging().SendOrdered(evt, MessageTopicName);

      if ((targetGrids & DesignNotificationGridMutability.NotifyMutable) != 0)
        gridFactory.Grid(StorageMutability.Mutable).GetMessaging().SendOrdered(evt, MessageTopicName);
    }
  }

}
