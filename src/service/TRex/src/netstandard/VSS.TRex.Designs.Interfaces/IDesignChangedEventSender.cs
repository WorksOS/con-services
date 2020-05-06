using System;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesignChangedEventSender
  {
    /// <summary>
    /// Notify all interested nodes in the immutable grid of a design change
    /// </summary>
    void DesignStateChanged(DesignNotificationGridMutability targetGrid, Guid siteModelUid, Guid designUid, ImportedFileType fileType, bool designRemoved = false );
  }
}
