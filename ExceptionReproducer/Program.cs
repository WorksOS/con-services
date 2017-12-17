using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Eviction;
using Apache.Ignite.Core.Configuration;
using Apache.Ignite.Core.PersistentStore;
using System;
using System.Collections.Generic;
using System.IO;
using VSS.VisionLink.Raptor.Servers;

namespace ExceptionReproducer
{
    class Program
    {
        private const string PersistentCacheStoreLocation = "C:\\Temp\\RaptorIgniteData\\Persistence";

        public static uint GetDescriptor()
        {
            return 0;
        }

        private static void SetupGridConfiguration(IgniteConfiguration cfg)
        {
            cfg.IgniteInstanceName = "Raptor";
            cfg.JvmInitialMemoryMb = 512; // Set to minimum advised memory for Ignite grid JVM of 512Mb
            cfg.JvmMaxMemoryMb = 1 * 1024; // Set max to 2Gb

            cfg.UserAttributes = new Dictionary<String, object>
            {
                { "Owner", "Raptor" },
                { $"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{ServerRoles.PSNODE}", "Yes" },
                { "Division", GetDescriptor() }
            };
            cfg.DataStorageConfiguration = new DataStorageConfiguration()
            {
                StoragePath = "C:\\Temp\\RaptorIgniteData\\Persistence" // PersistentCacheStoreLocation
            };
        }

        private static void SetupCacheConfiguration(CacheConfiguration cfg)
        {
            cfg.Name = "Cache";
            cfg.CopyOnRead = false;
            cfg.KeepBinaryInStore = false;

//            cfg.CacheStoreFactory = new RaptorCacheStoreFactory(false, true);
//            cfg.ReadThrough = true;
//            cfg.WriteThrough = true;

            cfg.WriteBehindFlushFrequency = new TimeSpan(0, 0, 30); // 30 seconds 
            cfg.EvictionPolicy = new LruEvictionPolicy()
            {
                MaxMemorySize = 100000000   // 100Mb
            };
            cfg.CacheMode = CacheMode.Replicated;
            cfg.Backups = 0;
        }

        public static void Reproduce()
        {
            IgniteConfiguration cfg = new IgniteConfiguration();

            SetupGridConfiguration(cfg);

            IIgnite Grid = Ignition.Start(cfg);

            try
            {
                Grid.SetActive(true);
                Console.WriteLine("OK!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Oops!: {0}", e);
            }

            CacheConfiguration cacheCfg = new CacheConfiguration();
            SetupCacheConfiguration(cacheCfg);

            ICache<String, MemoryStream> cache = null;
            cache = Grid.GetOrCreateCache<String, MemoryStream>(cacheCfg);
        }

        static void Main(string[] args)
        {
            Reproduce();
        }
    }
}
