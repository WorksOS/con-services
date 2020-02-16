using System;   
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LegacyApiUserProvisioning.UserManagement.Interfaces;
using Newtonsoft.Json;

namespace LegacyApiUserProvisioning.WebApi.Models

{
    public class UserCreationRequest : IUserCreationRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("customerId")]
        public long CustomerId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("features")]
        public IEnumerable<int> Features { get; set; }
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }
        [JsonProperty("customerUid")]
        public string CustomerUid { get; set; }
    }
}