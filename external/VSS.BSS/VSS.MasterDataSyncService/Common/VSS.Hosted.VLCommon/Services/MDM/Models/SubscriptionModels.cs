using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Hosted.VLCommon.Services.MDM.Models
{
    public class CreateAssetSubscriptionEvent
    {
        public Guid SubscriptionUID { get; set; }

        public Guid CustomerUID { get; set; }

        public Guid AssetUID { get; set; }

        public Guid DeviceUID { get; set; }

        public string SubscriptionType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime ActionUTC { get; set; }

        public DateTime ReceivedUTC { get; set; }
    }

    public class UpdateAssetSubscriptionEvent
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
