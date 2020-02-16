using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyApiUserProvisioning.UserManagement.Interfaces;
using Newtonsoft.Json;

namespace LegacyApiUserProvisioning.UserManagement
{
   
    public class User:IUser
    {
      
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("userId")]
        public string UserName { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("features")]
        public List<IApiFeature> Features { get; set; }
    }
}
