using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetDetail
{
    public class AssetDetailResponseModel
    {
        public AssetInfo AssetInfo { get; set; }
        public DeviceInfo DeviceInfo { get; set; }
        public List<AccountInfo> AccountInfo { get; set; }
        public Subscription Subscription { get; set; }
    }

    public class AssetInfo
    {
        public string AssetName { get; set; }
        public string SerialNumber { get; set; }
        public string MakeCode { get; set; }
        public string Model { get; set; }
        public string AssetType { get; set; }
        public int ModelYear { get; set; }
        public Guid AssetUID { get; set; }
    }

    public class DeviceInfo
    {
        public string DeviceSerialNumber { get; set; }
        public string DeviceType { get; set; }
        public string DeviceState { get; set; }
        public Guid DeviceUID { get; set; }
    }

    public class AccountInfo
    {
        public Guid CustomerUID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public Guid? ParentCustomerUID { get; set; }
        public string ParentName { get; set; }
        public string ParentCustomerType { get; set; }
    }

    public class Subscription
    {
        public Guid AssetUID { get; set; }
        public string SubscriptionStatus { get; set; }
        public List<OwnersVisibility> OwnersVisibility { get; set; }
    }

    public class OwnersVisibility
    {
        public Guid CustomerUID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public Guid SubscriptionUID { get; set; }
        public string SubscriptionName { get; set; }
        public string SubscriptionStatus { get; set; }
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime SubscriptionEndDate { get; set; }
    }
}
