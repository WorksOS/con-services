using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.MasterData.WebAPI.DbModel
{
    public class AssetOwnerInfo
    {
        // i.e. "Universal Customer"
        public string CustomerName { get; set; }
        // i.e. "Account Name"
        public string AccountName { get; set; }
        // i.e. "DCN"
        public string DealerAccountCode { get; set; }
        // i.e. "Registered DealerUID"
        public string DealerUID { get; set; }
        // i.e. "Registered Dealer"
        public string DealerName { get; set; }
        // i.e. "Dealer Code"
        public string NetworkDealerCode { get; set; }
        // i.e. "UCID"
        public string NetworkCustomerCode { get; set; }
        // i.e. CustomerUID"
        public string CustomerUID { get; set; }
        // i.e. AccountUID"
        public string AccountUID { get; set; }
    }
}

