using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace LandfillService.AcceptanceTests.Models.KafkaTopics
{
  public class AssociateCustomerUserEvent : ICustomerUserEvent
  {
    public Guid CustomerUID { get; set; }
    public Guid UserUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}