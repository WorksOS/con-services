using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Eviction;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public RaptorClientServer()
        {
            if (raptorGrid == null)
            {
                // Attempt to attach to an already existing Ignite instance
                raptorGrid = Ignition.TryGetIgnite("Raptor");

                // If there was no connection obtained, attempt to create a new instance
                if (raptorGrid == null)
                {
                    IgniteConfiguration cfg = new IgniteConfiguration()
                    {
                        GridName = "Raptor",
                        ClientMode = true
                    };

                    raptorGrid = Ignition.Start(cfg);
                }
            }

            /*
            if (spatialGrid == null)
            {
                // Attempt to attach to an already existing Ignite instance
                spatialGrid = Ignition.TryGetIgnite("Spatial");

                // If there was no connection obtained, attempt to create a new instance
                if (spatialGrid == null)
                {
                    IgniteConfiguration cfg = new IgniteConfiguration()
                    {
                        GridName = "Spatial",
                        ClientMode = true
                    };

                    spatialGrid = Ignition.Start(cfg);
                }
            }
            */
        }
    }    
}
