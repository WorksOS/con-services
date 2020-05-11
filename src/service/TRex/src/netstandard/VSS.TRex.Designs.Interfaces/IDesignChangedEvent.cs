using System;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesignChangedEvent
  {
    Guid SiteModelUid { get; }
    Guid DesignUid { get; }
    ImportedFileType FileType { get; }
    bool DesignRemoved { get; }
    // This may expand in future for blacklisting and adding designs
  }
}
