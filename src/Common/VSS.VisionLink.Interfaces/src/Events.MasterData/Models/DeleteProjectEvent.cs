using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;


namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
  public class DeleteProjectEvent : IProjectEvent 
  {
    public Guid ProjectUID { get; set; }
    public bool DeletePermanently { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}