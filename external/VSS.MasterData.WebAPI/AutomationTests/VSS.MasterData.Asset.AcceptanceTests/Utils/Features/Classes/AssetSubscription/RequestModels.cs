using Newtonsoft.Json;
using System;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetSubscription
{
    #region Valid AssetSubscriptionServiceRequest

    public class AssetSubscriptionModel
    {
        public Guid SubscriptionUID { get; set; }
        public Guid CustomerUID { get; set; }
        public Guid AssetUID { get; set; }
        public Guid DeviceUID { get; set; }
        public string SubscriptionType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ActionUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ReceivedUTC { get; set; }
    }

    #endregion
}
