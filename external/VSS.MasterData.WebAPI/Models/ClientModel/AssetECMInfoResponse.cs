using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.MasterData.WebAPI.ClientModel
{
    public class AssetECMInfoResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid AssetUID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<AssetECMInfo> AssetECMInfo { get; set; }
    }
}
