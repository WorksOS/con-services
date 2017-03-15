using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
  public class CreateCustomerTccOrgEvent : ICustomerEvent
  {
    public CreateCustomerTccOrgEvent() {; }

    public Guid CustomerUID { get; set; }
    public string TCCOrgID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}