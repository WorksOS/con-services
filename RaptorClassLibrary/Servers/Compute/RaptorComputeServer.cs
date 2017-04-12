using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Eviction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Storage;

namespace VSS.VisionLink.Raptor.Servers.Compute
{
    /// <summary>
    /// Defines a representation of a server responsible for performing Raptor related compute operations using
    /// the Ignite In Memory Data Grid
    /// </summary>
    public class RaptorComputeServer : RaptorIgniteServer
    {
        public RaptorComputeServer()
        {
            if (ignite == null)
            {
                IgniteConfiguration cfg = new IgniteConfiguration()
                {
                    GridName = "Raptor",
                    JvmMaxMemoryMb = 6000
                };

                ignite = Ignition.Start(cfg);
                ignite = Ignition.TryGetIgnite("Raptor");

                // Add a cache to Ignite
                cache = ignite.GetOrCreateCache<String, MemoryStream>
                    (new CacheConfiguration()
                    {
                        Name = "DataModels",
                        CopyOnRead = false,
                        KeepBinaryInStore = false,
                        MemoryMode = CacheMemoryMode.OnheapTiered,
                        CacheStoreFactory = new RaptorCacheStoreFactory(),
                        ReadThrough = true,
                        WriteThrough = true,
                        WriteBehindFlushFrequency = new TimeSpan(0, 0, 30), // 30 seconds 
                        EvictionPolicy = new LruEvictionPolicy()
                        {
                            MaxMemorySize = 2000000000,
                        },
                    });
            }
        }
    }
}
