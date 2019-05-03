using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Interfaces.Events.Identity.User
{
    /// <summary>
    /// DissociateUserRoleEvent Model
    /// </summary>
    public class DissociateUserRoleEvent
    {
        public string UserUID { get; set; }
        public Int64 RoleID { get; set; }
        public string CustomerUID { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}
