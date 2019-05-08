using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
    public class AssociateProjectSubscriptionEvent : ISubscriptionEvent
    {
        public Guid SubscriptionUID { get; set; }

        public Guid ProjectUID { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime ActionUTC { get; set; }

        public DateTime ReceivedUTC { get; set; }
    }
}
