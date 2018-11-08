using System;

namespace VSS.MasterData.Models.Models
{
    public class AssociateProjectSubscriptionData
    {
        public Guid SubscriptionUID { get; set; }
        public Guid ProjectUID { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ActionUTC => DateTime.UtcNow;
    }
}