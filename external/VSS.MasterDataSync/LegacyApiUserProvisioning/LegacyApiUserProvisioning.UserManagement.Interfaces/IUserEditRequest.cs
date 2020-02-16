using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyApiUserProvisioning.UserManagement.Interfaces
{
    public interface IUserEditRequest
    {
        string Email { get; set; }
        string UserName { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Password { get; set; }
        IEnumerable<int> Features { get; set; }
        string RequestedBy { get; set; }
    }
}
