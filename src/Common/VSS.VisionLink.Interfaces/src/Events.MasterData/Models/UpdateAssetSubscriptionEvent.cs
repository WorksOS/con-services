using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
    public class UpdateAssetSubscriptionEvent : ISubscriptionEvent
    {
        public Guid SubscriptionUID { get; set; }
        public Guid? CustomerUID { get; set; }
        public Guid? AssetUID { get; set; }
        public Guid? DeviceUID { get; set; }
        public string SubscriptionType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}