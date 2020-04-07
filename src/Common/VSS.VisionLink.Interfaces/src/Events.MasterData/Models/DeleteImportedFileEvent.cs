using System;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Models
{
  public class DeleteImportedFileEvent : IProjectEvent
  {
    public Guid ImportedFileUID { get; set; }
    public Guid ProjectUID { get; set; }
    public bool DeletePermanently { get; set; }
    public DateTime ActionUTC { get; set; }
  }
}
