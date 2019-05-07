using System;

namespace VSS.VisionLink.Interfaces.Events.ApplicationsManagement.Models
{
    public class AssociateRolePermissionEvent
    {
        public int roleID { get; set; }
        public int permissionID { get; set; }
        public DateTime eventUtc { get; set; }
    }
}
