using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LegacyApiUserProvisioning.UserManagement.Interfaces;

namespace LegacyApiUserProvisioning.WebApi.Models
{
    public class MigrateUsersRequest : IMigrateUsersRequest
    {
        public string CustomerUid { get; set; }

        public string[] UserIds { get; set; }
    }
}