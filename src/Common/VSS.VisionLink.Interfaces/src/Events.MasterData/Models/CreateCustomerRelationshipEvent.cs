using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
  public class CreateCustomerRelationshipEvent : ICustomerRelationshipEvent
  {
    public Guid ParentCustomerUID { get; set; }
    
    public Guid ChildCustomerUID { get; set; }  
    
    public DateTime ActionUTC { get; set; }    
    public DateTime ReceivedUTC { get; set; }
  }
}
