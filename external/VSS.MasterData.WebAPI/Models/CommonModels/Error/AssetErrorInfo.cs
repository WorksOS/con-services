using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.Error
{
    public class AssetErrorInfo : ErrorInfo
    {
        [JsonProperty(PropertyName = "assetUid", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetUID { get; set; }
    }
}
