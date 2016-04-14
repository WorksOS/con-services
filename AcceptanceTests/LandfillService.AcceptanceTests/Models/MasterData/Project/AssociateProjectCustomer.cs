using System;

namespace LandfillService.AcceptanceTests.Models
{
    public class AssociateProjectCustomer
    {
        public Guid ProjectUID { get; set; }
        public Guid CustomerUID { get; set; }
        public RelationType RelationType { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}