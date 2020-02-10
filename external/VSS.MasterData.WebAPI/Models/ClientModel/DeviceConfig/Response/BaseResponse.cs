using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ClientModel.DeviceConfig.Response
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseResponse<TLists, TErrorInfo>
    {
        [JsonProperty("errors", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IList<TErrorInfo> Errors { get; set; }

        public abstract IEnumerable<TLists> Lists { get; set; }

        //Default constructor added for the deserialization
        public BaseResponse() { }

        public BaseResponse(IList<TErrorInfo> errors)
        {
            this.Errors = errors;
        }

        public BaseResponse(TErrorInfo error)
        {
            this.Errors = new List<TErrorInfo> { error };
        }
        public BaseResponse(IEnumerable<TLists> lists, IList<TErrorInfo> errors = null)
        {
            this.Lists = lists;
            if (errors != null && errors.Count > 0)
            {
                this.Errors = errors;
            }
        }
    }
}
