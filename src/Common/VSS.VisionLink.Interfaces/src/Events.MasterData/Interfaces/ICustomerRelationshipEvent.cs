using System;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Interfaces
{
  public interface ICustomerRelationshipEvent
  {
    Guid ParentCustomerUID { get; set; }

    Guid ChildCustomerUID { get; set; }

    DateTime ActionUTC { get; set; }
    DateTime ReceivedUTC { get; set; }
  }
}
