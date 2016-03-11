using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using VSS.Subscription.Data.Interfaces;

namespace VSS.Subscription.Data.Models
{
    public class AssociateProjectSubscriptionEvent : ISubscriptionEvent
    {
        [Required]
        public Guid SubscriptionUID { get; set; }

        [Required]
        public Guid ProjectUID { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        [Required]
        public DateTime ActionUTC { get; set; }

        public DateTime ReceivedUTC { get; set; }
    }
}
