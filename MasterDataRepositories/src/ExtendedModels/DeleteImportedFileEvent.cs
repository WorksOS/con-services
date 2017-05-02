using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
  /// <summary>
  ///  See notes on CreateImportedFileEvent
  /// </summary>
  public class DeleteImportedFileEvent : IProjectEvent
  {
    public Guid ProjectUID { get; set; }
    public Guid ImportedFileUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}