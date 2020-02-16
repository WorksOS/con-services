using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyApiUserProvisioning.UserManagement.Interfaces;

namespace LegacyApiUserProvisioning.UserManagement
{
    public class UserEditRequestDto: IUserEditRequest
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
