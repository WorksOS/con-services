using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Eviction;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Servers.Client
{
    /// <summary>
    /// Defines a representation of a client able to request Raptor related compute operations using
    /// the Ignite In Memory Data Grid. All client type server classes should descend from this class.
    /// </summary>
    public class RaptorClientServer : RaptorIgniteServer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public RaptorClientServer(string role)
        {
            if (raptorGrid == null)
            {
                // Attempt to attach to an already existing Ignite instance
                raptorGrid = Ignition.TryGetIgnite("Raptor");

                // If there was no connection obtained, attempt to create a new instance
                if (raptorGrid == null)
                {
                    RaptorNodeID = Guid.NewGuid().ToString();

                    Log.InfoFormat("Creating new Ignite node with Role = {0} & RaptorNodeID = {1}", role, RaptorNodeID);

                    IgniteConfiguration cfg = new IgniteConfiguration()
                    {
                        GridName = "Raptor",
                        ClientMode = true,
                        UserAttributes = new Dictionary<string, object>() { { "Role", role }, { "RaptorNodeID", RaptorNodeID } }
                    };

                    raptorGrid = Ignition.Start(cfg);
                }
            }
        }
    }    
}
