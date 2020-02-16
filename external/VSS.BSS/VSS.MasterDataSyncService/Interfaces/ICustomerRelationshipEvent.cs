using System;

namespace VSS.Nighthawk.MasterDataSync.Interfaces
{
  public interface ICustomerRelationshipEvent
  {
    Guid? ParentCustomerUID { get; set; }
    Guid ChildCustomerUID { get; set; }
    DateTime ActionUTC { get; set; }

  }
}
