using System;

namespace LandfillService.AcceptanceTests.Models
{
    public class DeleteCustomerEvent
    {
        public Guid CustomerUID { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}
