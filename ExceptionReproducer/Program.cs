using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Eviction;
using Apache.Ignite.Core.PersistentStore;
using System;
using System.Collections.Generic;
using System.IO;

namespace ExceptionReproducer
{
    class Program
    {
        private const string PersistentCacheStoreLocation = "C:\\Temp\\RaptorIgniteData\\Persistence";

        public static uint getDescriptor()
        {
            return 0;
        }

        private static void setupGridConfiguration(IgniteConfiguration cfg)
        {
            cfg.GridName = "Raptor";
            cfg.IgniteInstanceName = "Raptor";
            cfg.JvmInitialMemoryMb = 512; // Set to minimum advised memory for Ignite grid JVM of 512Mb
            cfg.JvmMaxMemoryMb = 4 * 1024; // Set max to 4Gb

            cfg.UserAttributes = new Dictionary<String, object>();
            cfg.UserAttributes.Add("Owner", "Raptor");
            cfg.UserAttributes.Add("Role", "PSNode");
            cfg.UserAttributes.Add("Division", getDescriptor());

            cfg.PersistentStoreConfiguration = new PersistentStoreConfiguration()
            {
                PersistentStorePath = "C:\\Temp\\RaptorIgniteData\\Persistence" // PersistentCacheStoreLocation
            };
        }

        private static void setupCacheConfiguration(CacheConfiguration cfg)
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

            setupGridConfiguration(cfg);

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
            setupCacheConfiguration(cacheCfg);

            ICache<String, MemoryStream> cache = null;
            cache = Grid.GetOrCreateCache<String, MemoryStream>(cacheCfg);
        }

        static void Main(string[] args)
        {
            Reproduce();
        }
    }
}
