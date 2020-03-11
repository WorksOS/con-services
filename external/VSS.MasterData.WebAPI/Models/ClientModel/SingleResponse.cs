using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.MasterData.WebAPI.ClientModel
{
    public class SingleResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public class Asset
        {
            /// <summary>
            /// 
            /// </summary>
            public string VisionLinkIdentifier { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string MakeCode { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string MakeName { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string SerialNumber { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string AssetID { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string Model { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string ProductFamily { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string ManufactureYear { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string DeviceType { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string DeviceSerialNumber { get; set; }

        }
    }
}
