using System;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Events.MasterData.Models
{
  public class DeleteProjectEvent : IProjectEvent 
  {
    public Guid ProjectUID { get; set; }
    public bool DeletePermanently { get; set; }
    public DateTime ActionUTC { get; set; }
  }
}
