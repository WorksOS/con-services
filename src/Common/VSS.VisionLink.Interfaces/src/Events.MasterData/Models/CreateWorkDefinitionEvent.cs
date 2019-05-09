using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
  public class CreateWorkDefinitionEvent:IWorkDefinitionEvent
  {
    public Guid AssetUID { get; set; }
    public string WorkDefinitionType { get; set; } //Required Field
    public int? SensorNumber { get; set; }
    public bool? StartIsOn { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}
