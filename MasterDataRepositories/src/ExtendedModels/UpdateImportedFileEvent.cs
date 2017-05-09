using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
  /// <summary>
  ///  See notes on CreateImportedFileEvent
  /// </summary>
  public class UpdateImportedFileEvent : IProjectEvent
  {
    public Guid ProjectUID { get; set; }
    public Guid ImportedFileUID { get; set; }
    public string FileDescriptor { get; set; }
    public DateTime FileCreatedUtc { get; set; }
    public DateTime FileUpdatedUtc { get; set; }
    public string ImportedBy { get; set; }
    public DateTime? SurveyedUtc { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}