using System;

namespace VSS.VisionLink.Interfaces.Events.ApplicationsManagement.Models
{
    public class CreatePermissionEvent
    {
        public string action { get; set; }
        public string permissionUID { get; set; }
        public string resource { get; set; }
        public string description { get; set; }
        public long permissionID { get; set; }
        public string tpaasAppName { get; set; }
        public DateTime eventUtc { get; set; }
    }
}
