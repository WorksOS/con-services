using System;

namespace VSS.VisionLink.Interfaces.Events.ApplicationsManagement.Models
{
    public class CreateRoleEvent
    {
        public int roleID { get; set; }
        public string roleUID { get; set; }
        public string roleName { get; set; }
        public string tpaasAppName { get; set; }
        public string description { get; set; }
        public DateTime eventUtc { get; set; }
    }
}
