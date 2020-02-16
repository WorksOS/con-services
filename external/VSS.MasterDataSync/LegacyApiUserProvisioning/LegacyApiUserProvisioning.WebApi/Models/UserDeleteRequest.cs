using System.Collections.Generic;
using LegacyApiUserProvisioning.UserManagement.Interfaces;
using Newtonsoft.Json;

namespace LegacyApiUserProvisioning.WebApi.Models
{
    public class UserDeleteRequest : IUserDeleteRequest
    {
        [JsonProperty("userList")]
        public List<string> UserList { get; set; }
    }
}