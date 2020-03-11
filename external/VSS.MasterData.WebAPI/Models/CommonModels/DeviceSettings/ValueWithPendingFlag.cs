using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.DeviceSettings
{
    public class ValueWithPendingFlag<T>
    {
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public T Value { get; set; }
        [JsonProperty("isPending", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsPending { get; set; }
    }
}
