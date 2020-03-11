using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.MasterData.WebAPI.ClientModel
{
    public class AssetDeviceDetail
    {
        /// <summary>
        /// 
        /// </summary>
        public AssetInfo AssetInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DeviceModel DeviceInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<AssetCustomer> AccountInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public AssetSubscriptionModel Subscription { get; set; }
    }
}