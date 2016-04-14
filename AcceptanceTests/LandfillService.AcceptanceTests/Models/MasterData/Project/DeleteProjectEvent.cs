using System;

namespace LandfillService.AcceptanceTests.Models
{
    public class DeleteProjectEvent
    {
        public Guid ProjectUID { get; set; }
        public bool DeletePermanently { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}
