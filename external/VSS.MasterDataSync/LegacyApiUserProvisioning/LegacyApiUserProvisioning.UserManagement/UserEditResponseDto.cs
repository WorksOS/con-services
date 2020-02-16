using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyApiUserProvisioning.UserManagement.Interfaces;

namespace LegacyApiUserProvisioning.UserManagement
{
    public class UserEditResponseDto : IUserEditResponse
    {
        public string Error { get; set; }
        public IEnumerable<IApiFeature> Features { get; set; }
    }
}