using System;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesignChangedEventSender
  {
    /// <summary>
    /// Notify all interested nodes in the immutable grid a site model has changed attributes
    /// </summary>
    void DesignStateChanged(DesignNotificationGridMutability targetGrid, Guid siteModelUid, Guid designUid, ImportedFileType fileType, bool designRemoved = false );
  }
}
