using System;
using System.Collections.Generic;
using System.IO;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Configuration;

namespace OutOfMemoryReproducer
{
  static class Program
  {
    public static ICache<string, byte[]> cacheServer;

    static void Main()
    {
      // Make the server
      var cfgServer = new IgniteConfiguration
      {
        IgniteInstanceName = "Server",
        JvmMaxMemoryMb = 1024,
        JvmInitialMemoryMb = 512,
        DataStorageConfiguration = new DataStorageConfiguration
        {
          WalMode = WalMode.Fsync,
          PageSize = 16 * 1024, // Failure does not occur when using default page size of 4096
          StoragePath = Path.Combine(@"c:\temp", "ErrorReproducer", "Persistence"),
          WalArchivePath = Path.Combine(@"c:\temp", "ErrorReproducer", "WalArchive"),
          WalPath = Path.Combine(@"c:\temp", "ErrorReproducer", "WalStore"),
          DefaultDataRegionConfiguration = new DataRegionConfiguration
          {
            Name = "Default",
            InitialSize = 64 * 1024 * 1024, // 128 MB
            MaxSize = 2L * 1024 * 1024 * 1024, // 2 GB

            PersistenceEnabled = true
          },

          // Establish a separate data region for the TAG file buffer queue
          DataRegionConfigurations = new List<DataRegionConfiguration>
          {
            new DataRegionConfiguration
            {
              Name = "BufferQueue", 
              InitialSize = 64 * 1024 * 1024,
              MaxSize = 64 * 1024 * 1024,
              PersistenceEnabled = true,
              // EmptyPagesPoolSize = 200 // No change in behaviour between 100 vs 200
            }
          }
        },
        JvmOptions = new List<string>() {"-DIGNITE_QUIET=false", "-Djava.net.preferIPv4Stack=true", "-XX:+UseG1GC"},
        WorkDirectory = Path.Combine(@"c:\temp", "ErrorReproducer")
      };

      var igniteServer = Ignition.Start(cfgServer);

      // Activate the grid
      igniteServer.GetCluster().SetActive(true);

      // Set up the cache
      var cacheCfgServer = new CacheConfiguration {Name = "BufferQueueCache", KeepBinaryInStore = true, CacheMode = CacheMode.Partitioned, DataRegionName = "BufferQueue"};
      cacheServer = igniteServer.GetOrCreateCache<string, byte[]>(cacheCfgServer);

      // Write a series of elements in increasing size. Reliably fails at 4150
      for (var i = 1; i < 20000; i++)
      {
        cacheServer.Put($"Item {i}a", new byte[i]);
        Console.WriteLine($"Put item {i} with {i} bytes");
      }

      Console.WriteLine("Completed");
    }
  }
}
