using System;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Models
{
  public class UpdateImportedFileEvent : IProjectEvent
  {
    public string ImportedFileUID { get; set; }
    public string FileDescriptor { get; set; }
    public DateTime FileCreatedUtc { get; set; }
    public DateTime FileUpdatedUtc { get; set; }
    public string ImportedBy { get; set; }
    public DateTime? SurveyedUtc { get; set; }
    public string ProjectUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
    public int MinZoomLevel { get; set; }
    public int MaxZoomLevel { get; set; }
    public double Offset { get; set; }
  }
}
