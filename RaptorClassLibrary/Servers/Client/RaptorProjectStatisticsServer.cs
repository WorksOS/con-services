using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Servers.Client
{
    /// <summary>
    /// A server type supporting requests relating to project statistics
    /// </summary>
    public class RaptorProjectStatisticsServer : RaptorClientServer
    {
        public RaptorProjectStatisticsServer() : base(ServerRoles.ASNODE)
        {

        }
    }
}
