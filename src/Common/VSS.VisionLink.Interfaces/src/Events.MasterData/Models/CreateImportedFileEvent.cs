using System;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Models
{
  public class CreateImportedFileEvent : IProjectEvent
  {
    public string ImportedFileUID { get; set; }
    public long ImportedFileID { get; set; }
    public string CustomerUID { get; set; }
    public ImportedFileType ImportedFileType { get; set; }
    public string Name { get; set; }
    public string FileDescriptor { get; set; }
    public DateTime FileCreatedUtc { get; set; }
    public DateTime FileUpdatedUtc { get; set; }
    public string ImportedBy { get; set; }
    public DateTime? SurveyedUTC { get; set; }
    public DxfUnitsType DxfUnitsType { get; set; }
    public string ProjectUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
    public int MinZoomLevel { get; set; }
    public int MaxZoomLevel { get; set; }
    public double Offset { get; set; }
    public string ParentUID { get; set; }

  }
}
