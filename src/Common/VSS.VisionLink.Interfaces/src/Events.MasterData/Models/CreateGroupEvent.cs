using System;
using System.Collections.Generic;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
  public class CreateGroupEvent : IGroupEvent
  {
    public string GroupName { get; set; } //Required Field
    public Guid CustomerUID { get; set; }
    public Guid UserUID { get; set; }
    public List<Guid> AssetUID { get; set; }
    public Guid GroupUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
    public string Description { get; set; }
  }
}