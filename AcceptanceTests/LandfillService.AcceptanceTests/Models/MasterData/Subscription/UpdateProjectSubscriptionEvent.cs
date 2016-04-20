using System;

namespace LandfillService.AcceptanceTests.Models
{
    public class UpdateProjectSubscriptionEvent
    {
        public Guid SubscriptionUID { get; set; }

        public Guid? CustomerUID { get; set; }

        public string SubscriptionType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime ActionUTC { get; set; }

        public DateTime ReceivedUTC { get; set; }
    }
}
