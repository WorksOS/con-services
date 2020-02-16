using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json;

namespace LegacyApiUserProvisioning.WebApi
{
    namespace IdentityManager.Common.Errors
    {
        [ExcludeFromCodeCoverage]
        public class ErrorInfo
        {
            [JsonProperty(PropertyName = "status")]
            public HttpStatusCode StatusCode { get; set; }

            [JsonProperty(PropertyName = "errorCode")]
            public int ErrorCode { get; set; }

            [JsonProperty(PropertyName = "message")]
            public string Message { get; set; }
        }
    }
}