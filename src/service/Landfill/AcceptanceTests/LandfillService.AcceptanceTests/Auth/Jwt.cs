using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LandfillService.AcceptanceTests.Auth
{
    /// <summary>
    ///   Model class for Jwt
    /// </summary>
    public class Jwt
    {
        #region Payload
        /// <summary>
        ///   Iss
        /// </summary>
        [JsonProperty(PropertyName = "iss")]
        public string Iss { get; set; }

        /// <summary>
        ///   Exp
        /// </summary>
        [JsonProperty(PropertyName = "exp")]
        public string Exp { get; set; }

        /// <summary>
        ///   Subscriber
        /// </summary>
        [JsonProperty(PropertyName = "http://wso2.org/claims/subscriber")]
        public string Subscriber { get; set; }

        /// <summary>
        ///   ApplicationId
        /// </summary>
        [JsonProperty(PropertyName = "http://wso2.org/claims/applicationid")]
        public int ApplicationId { get; set; }

        /// <summary>
        ///   Application Name
        /// </summary>
        [JsonProperty(PropertyName = "http://wso2.org/claims/applicationname")]
        public string ApplicationName { get; set; }

        /// <summary>
        ///   Application Tier
        /// </summary>
        [JsonProperty(PropertyName = "http://wso2.org/claims/applicationtier")]
        public string ApplicationTier { get; set; }

        /// <summary>
        ///   Api Context
        /// </summary>
        [JsonProperty(PropertyName = "http://wso2.org/claims/apicontext")]
        public string ApiContext { get; set; }

        /// <summary>
        ///   Version
        /// </summary>
        [JsonProperty(PropertyName = "http://wso2.org/claims/version")]
        public string Version { get; set; }

        /// <summary>
        ///   Tier
        /// </summary>
        [JsonProperty(PropertyName = "http://wso2.org/claims/tier")]
        public string Tier { get; set; }

        /// <summary>
        ///   Key Type
        /// </summary>
        [JsonProperty(PropertyName = "http://wso2.org/claims/keytype")]
        public string KeyType { get; set; }

        /// <summary>
        ///   User Type
        /// </summary>
        [JsonProperty(PropertyName = "http://wso2.org/claims/usertype")]
        public string UserType { get; set; }

        /// <summary>
        ///   EndUser
        /// </summary>
        [JsonProperty(PropertyName = "http://wso2.org/claims/enduser")]
        public string EndUser { get; set; }

        /// <summary>
        ///   Tenant Id of the End User
        /// </summary>
        [JsonProperty(PropertyName = "http://wso2.org/claims/enduserTenantId")]
        public string EndUserTenantId { get; set; }

        /// <summary>
        ///   Email
        /// </summary>
        [JsonProperty(PropertyName = "http://wso2.org/claims/emailaddress")]
        public string Email { get; set; }

        /// <summary>
        ///   FirstName
        /// </summary>
        [JsonProperty(PropertyName = "http://wso2.org/claims/givenname")]
        public string GivenName { get; set; }

        /// <summary>
        ///   Last Name
        /// </summary>
        [JsonProperty(PropertyName = "http://wso2.org/claims/lastname")]
        public string LastName { get; set; }

        /// <summary>
        ///   OneTimePassword
        /// </summary>
        [JsonProperty(PropertyName = "http://wso2.org/claims/oneTimePassword")]
        public string Otp { get; set; }

        /// <summary>
        ///   Roles of the User
        /// </summary>
        [JsonProperty(PropertyName = "http://wso2.org/claims/role")]
        public string Role { get; set; }

        /// <summary>
        ///   Unique Id of the User
        /// </summary>
        [JsonProperty(PropertyName = "http://wso2.org/claims/uuid")]
        public string Uuid { get; set; } 
        #endregion

        public static Jwt SampleJwtPayloadObject()
        {
            Jwt payloadObj = null;
            string token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJTSEEyNTZ3aXRoUlNBIiwieDV0IjoiWW1FM016UTRNVFk0TkRVMlpEWm1PRGRtTlRSbU4yWmxZVGt3TVdFelltTmpNVGt6TURFelpnPT0ifQ==.eyJpc3MiOiJ3c28yLm9yZy9wcm9kdWN0cy9hbSIsImV4cCI6MTQ1NTU3NzgyMzkzMCwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9zdWJzY3JpYmVyIjoiY2xheV9hbmRlcnNvbkB0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBwbGljYXRpb25pZCI6IjEwNzkiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IlV0aWxpemF0aW9uIERldmVsb3AgQ0kiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9udGllciI6IiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBpY29udGV4dCI6Ii90L3RyaW1ibGUuY29tL3V0aWxpemF0aW9uYWxwaGFlbmRwb2ludCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdmVyc2lvbiI6IjEuMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMva2V5dHlwZSI6IlBST0RVQ1RJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjoiQVBQTElDQVRJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbmR1c2VyVGVuYW50SWQiOiIxIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hdmF0YXIiOiJodHRwczovL2xoNi5nb29nbGV1c2VyY29udGVudC5jb20vLUZkbE5OVWk1QjFnL0FBQUFBQUFBQUFJL0FBQUFBQUFBQUJNL09aaWJVa012VW13L3Bob3RvLmpwZyIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW1haWxhZGRyZXNzIjoiY2xheV9hbmRlcnNvbkB0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZmlyc3RuYW1lIjoiQ2xheSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZnVsbG5hbWUiOiJDbGF5IEFuZGVyc29uIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9naXZlbm5hbWUiOiJDbGF5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS9hY2NvdW50TG9ja2VkIjoiZmFsc2UiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2lkZW50aXR5L2ZhaWxlZExvZ2luQXR0ZW1wdHMiOiIwIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS91bmxvY2tUaW1lIjoiMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdG5hbWUiOiJBbmRlcnNvbiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcm9sZSI6IlN1YnNjcmliZXIscHVibGlzaGVyIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9zb2NpYWxmZWRlcmF0ZWRpZDEiOiIxMTM1Mjg5NDk3MTg0ODgxNzEzODciLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3NvY2lhbGZlZGVyYXRlZHNvdXJjZTEiOiJ3d3cuZ29vZ2xlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdXVpZCI6ImI4MGI1NDJiLTU1NmYtNGRmNy05MzgzLWE2MzBhNzYxNTUzNiJ9.qpjbU7tgjXFqwyPkYixhUendBjyEOkWCXNFAcEJQjgcOysLWQeNrDGPALpnUZsXfeXnGuyxPmbM/oHJ9rdvTC2wq2Vz+O7ycu2/zs+T206GbCfhmgPwZ8CI+VxfFyOcoLpbIPdIlBIVhjq4qsEl2aMLWl+reqE+3Ne4lUYwmgRA=";

            string payloadBase64 = token.Split('.')[1];
            byte[] payloadBytes = Convert.FromBase64String(payloadBase64);
            string payloadJson = Encoding.UTF8.GetString(payloadBytes);
            payloadObj = JsonConvert.DeserializeObject<Jwt>(payloadJson);

            return payloadObj;
        }
        public static string EncodeJwtToken(Jwt payloadObj)
        {
            string header = "eyJ0eXAiOiJKV1QiLCJhbGciOiJTSEEyNTZ3aXRoUlNBIiwieDV0IjoiWW1FM016UTRNVFk0TkRVMlpEWm1PRGRtTlRSbU4yWmxZVGt3TVdFelltTmpNVGt6TURFelpnPT0ifQ==";
            string signature = "kTaMf1IY83fPHqUHTtVHn6m6aQ9wFch6c0FsNDQ7x1k=";


            string payloadJson = JsonConvert.SerializeObject(payloadObj);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payloadJson);
            string payload = Convert.ToBase64String(payloadBytes);

            // 4-word alignment padding
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

            return header + "." + payload + "." + signature;
        }
        public static string GetJwtToken(Guid userUid)
        {
            Jwt payload = SampleJwtPayloadObject();
            payload.Uuid = userUid.ToString();

            return EncodeJwtToken(payload);
        }
    }
}
