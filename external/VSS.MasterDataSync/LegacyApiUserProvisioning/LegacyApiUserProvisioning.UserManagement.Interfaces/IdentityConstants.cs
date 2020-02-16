using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyApiUserProvisioning.UserManagement.Interfaces
{
    public static class IdentityConstants
    {
        public static string VISIONLINK_CUSTOMERUID => "X-VisionLink-CustomerUid";
        public static string VISIONLINK_USERUID => "X-VisionLink-UserUid";
        public static string USERUID_API => "UserUID_IdentityAPI";
    }
}
