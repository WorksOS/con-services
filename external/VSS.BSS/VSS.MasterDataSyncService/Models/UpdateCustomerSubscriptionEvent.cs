using Newtonsoft.Json;
using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
    public class UpdateCustomerSubscriptionEvent : ISubscriptionEvent
    {
        public Guid SubscriptionUID { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime ActionUTC { get; set; }

        public DateTime ReceivedUTC { get; set; }
    }
}
