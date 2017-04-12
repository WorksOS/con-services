using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Eviction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Servers
{
    /// <summary>
    /// Defines a representation of a client able to request Raptor related compute operations using
    /// the Ignite In Memory Data Grid. All client type server classes should descend from this class.
    /// </summary>
    public class RaptorClientServer : RaptorIgniteServer
    {
        public RaptorClientServer()
        {
            if (ignite == null)
            {
               IgniteConfiguration cfg = new IgniteConfiguration()
                {
                    GridName = "Raptor",
                    ClientMode = true
                };

                // Attempt to attach to an already existing Ignite instance
                ignite = Ignition.TryGetIgnite("Raptor");

                // If there was no connection obtained, attempt to create a new instance
                if (ignite == null)
                {
                    ignite = Ignition.Start(cfg);
                }

                /*                if (ignite == null)
                                {
                                    ignite = Ignition.GetIgnite();
                                }

                                // Add a cache to Ignite
                                cache = ignite.GetOrCreateCache<String, System.IO.MemoryStream>
                                    (new CacheConfiguration()
                                    {
                                        Name = "DataModels",
                                        CopyOnRead = false,
                                        KeepBinaryInStore = false,
                                        MemoryMode = CacheMemoryMode.OnheapTiered,
                                        ReadThrough = true,
                                        WriteThrough = true,
                                        WriteBehindFlushFrequency = new TimeSpan(0, 0, 30), // 30 seconds 
                                        EvictionPolicy = new LruEvictionPolicy()
                                        {
                                            MaxMemorySize = 2000000000,
                                        },
                                    });
                */
            }
        }
    }    
}
