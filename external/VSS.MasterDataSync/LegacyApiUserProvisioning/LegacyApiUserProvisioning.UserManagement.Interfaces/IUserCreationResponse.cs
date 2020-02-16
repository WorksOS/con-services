using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LegacyApiUserProvisioning.UserManagement.Interfaces
{
    public interface IUserCreationResponse : IBaseResponse
    {
        [JsonProperty("username")]
        string UserName { get;  }

        [JsonProperty("email")]
        string Email{ get; }

        [JsonProperty("firstname")]
        string FirstName{ get; }

        [JsonProperty("lastName")]
        string LastName { get; }

        [JsonProperty("features")]
        IEnumerable<IApiFeature> Features { get; }
    }
}