using System.Collections.Generic;
using LegacyApiUserProvisioning.UserManagement.Interfaces;

namespace LegacyApiUserProvisioning.WebApi.Models
{
    public class UserEditRequest: IUserEditRequest
    {
        public string Email { get; set; }

        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public IEnumerable<int> Features { get; set; }
        public string RequestedBy { get; set; }
    }
}