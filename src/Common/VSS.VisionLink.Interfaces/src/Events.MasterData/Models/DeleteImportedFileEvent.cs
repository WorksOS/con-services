using System;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Models
{
  public class DeleteImportedFileEvent : IProjectEvent
  {
    public string ImportedFileUID { get; set; }
    public string ProjectUID { get; set; }
    public bool DeletePermanently { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}
