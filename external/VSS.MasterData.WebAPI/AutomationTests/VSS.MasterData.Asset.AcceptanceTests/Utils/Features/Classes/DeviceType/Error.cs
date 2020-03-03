using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.DeviceType
{
   
    public class Error

    {
        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
