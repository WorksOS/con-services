using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyApiUserProvisioning.UserManagement.Interfaces;
using Newtonsoft.Json;

namespace LegacyApiUserProvisioning.UserManagement
{
    public class UserCreationResponse : IUserCreationResponse
    {
        public string Error { get; set; }

        [JsonIgnore]
        public VSS.Hosted.VLCommon.User User { get; set; }

        public string UserName => this.User.Name;
        public string Email => this.User.EmailContact;
        public string FirstName => this.User.FirstName;
        public string LastName => this.User.LastName;

        public IEnumerable<IApiFeature> Features { get; set; }
    }
}