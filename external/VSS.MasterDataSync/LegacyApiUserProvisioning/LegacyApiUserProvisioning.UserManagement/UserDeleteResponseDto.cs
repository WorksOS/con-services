using LegacyApiUserProvisioning.UserManagement.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyApiUserProvisioning.UserManagement
{
    public class UserDeleteResponseDto : IUserDeleteResponse
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }
        public bool Success { get; set; }
    }
}