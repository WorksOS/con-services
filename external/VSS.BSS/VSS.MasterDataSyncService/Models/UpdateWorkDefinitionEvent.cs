using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
  public class UpdateWorkDefinitionEvent : IWorkDefinitionEvent
  {
    public Guid AssetUID { get; set; }

    public string WorkDefinitionType { get; set; }

    public int? SensorNumber { get; set; }

    public bool? StartIsOn { get; set; }

    public DateTime ActionUTC { get; set; }
  }
}