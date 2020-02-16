using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyApiUserProvisioning.UserManagement.Interfaces
{
    public interface IMigrateUsersRequest
    {
        string CustomerUid { get; set; }

        string[] UserIds { get; set; }
    }
}