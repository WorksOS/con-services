using System;

namespace LandfillService.AcceptanceTests.Models
{
    public class AssociateCustomerUserEvent
    {
        public Guid CustomerUID { get; set; }
        public Guid UserUID { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}