using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LegacyApiUserProvisioning.UserManagement.Interfaces
{
    public interface IUserDeleteRequest
    {
        [JsonProperty("userList")]
        List<string> UserList { get; }
    }
}
