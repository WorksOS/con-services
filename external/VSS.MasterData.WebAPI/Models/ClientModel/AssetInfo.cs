using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.MasterData.WebAPI.ClientModel
{
    public class AssetInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string AssetName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MakeCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AssetType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? ModelYear { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid? AssetUID { get; set; }
    }
}