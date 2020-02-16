using System;

namespace VSS.Nighthawk.MasterDataSync.Interfaces
{
    public interface IAssetOwnerEvent
    {
        Guid AssetUID { get; set; }
        AssetOwner AssetOwnerRecord { get; set; }
        string Action { get; set; }
        DateTime ActionUTC { get; set; }
        DateTime ReceivedUTC { get; set; }
    }

    public class AssetOwner
    {
        // i.e. "Universal Customer"
        public string CustomerName { get; set; }
        // i.e. "Customer Account Name"
        public string AccountName { get; set; }
        // i.e. "AccountUID"
        public Guid? AccountUID { get; set; }
        // i.e. "DCN"
        public string DealerAccountCode { get; set; }
        // i.e. "Registered DealerUID"
        public Guid DealerUID { get; set; }
        // i.e. "Registered Dealer"
        public string DealerName { get; set; }
        // i.e. "Dealer Code"
        public string NetworkDealerCode { get; set; }
        // i.e. "UCID"
        public string NetworkCustomerCode { get; set; }
        // i.e. "CustomerUID"
        public Guid? CustomerUID { get; set; }
  
    }
}
