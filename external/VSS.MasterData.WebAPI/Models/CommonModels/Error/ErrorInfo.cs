using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace CommonModel.Error
{
	[ExcludeFromCodeCoverage]
    public class ErrorInfo : IErrorInfo
    {
        [JsonProperty(PropertyName = "errorCode", NullValueHandling = NullValueHandling.Ignore)]
        public int ErrorCode { get; set; }

        [JsonProperty(PropertyName = "message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonIgnore]
        public bool IsInvalid { get; set; }
    }
}
