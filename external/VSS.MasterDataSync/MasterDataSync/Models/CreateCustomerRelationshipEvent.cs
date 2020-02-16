using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
    public class CreateCustomerRelationshipEvent : ICustomerRelationshipEvent
    {
        public Guid? ParentCustomerUID { get; set; }
        public Guid ChildCustomerUID { get; set; }
        public Guid? AccountCustomerUID { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}