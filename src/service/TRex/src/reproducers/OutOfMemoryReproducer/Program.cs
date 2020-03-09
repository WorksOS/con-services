using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Event;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cache.Query.Continuous;
using Apache.Ignite.Core.Configuration;
using Apache.Ignite.Core.Deployment;

namespace OutOfMemoryReproducer
{
  public class RemoteFilter :
    ICacheEntryFilter<string, byte[]>,
    ICacheEntryEventFilter<string, byte[]>
  {
    public RemoteFilter() 
    {
    }

    public bool Invoke(ICacheEntry<string, byte[]> entry)
    {
      Console.WriteLine($"Invoke, removing item {entry.Key} with {entry.Value.Length} bytes");
      Program.cacheServer.Remove(entry.Key);

      // Pause for a bit to simulate real processing time
      Thread.Sleep(50);
      
      return false;
    }

    public bool Evaluate(ICacheEntryEvent<string, byte[]> evt)
    {
      Console.WriteLine($"Evaluate, removing item {evt.Key} with {evt.Value.Length} bytes");
      Program.cacheServer.Remove(evt.Key);

      // Pause for a bit to simulate real processing time
      Thread.Sleep(50);

      return false;
    }
  }

  public class LocalListener : ICacheEntryEventListener<string, byte[]>
  {
    /// <summary>
    /// Event called whenever there are new items in the TAG file buffer queue discovered by the continuous query
    /// Events include creation, modification and deletion of cache entries
    /// </summary>
    /// <param name="events"></param>
    public void OnEvent(IEnumerable<ICacheEntryEvent<string, byte[]>> events)
    {
      // Add the keys for the given events into the Project/Asset mapping buckets ready for a processing context
      // to acquire them. 

      foreach (var evt in events)
      {
        // Only interested in newly added items to the cache. Updates and deletes are ignored.
        if (evt.EventType != CacheEntryEventType.Created)
          continue;

        try
        {
          // Dummy functionality...
          Console.WriteLine($"#Progress# Added item [{evt.Key}] with {evt.Value.Length} bytes");
        }
        catch (Exception e)
        {
          Console.WriteLine($"Exception occurred adding item {evt.Key}, {e.Message}");
        }
      }
    }
  }

  class Program
  {
    public static ICache<string, byte[]> cacheServer = null;
    public static ICache<string, byte[]> cacheClient = null;

    static void Main(string[] args)
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
          PageSize = 16384,
          StoragePath = Path.Combine(@"c:\temp", "ErrorReproducer", "Persistence"),
          WalArchivePath = Path.Combine(@"c:\temp", "ErrorReproducer", "WalArchive"),
          WalPath = Path.Combine(@"c:\temp", "ErrorReproducer", "WalStore"),
          DefaultDataRegionConfiguration = new DataRegionConfiguration
          {
            Name = "Default",
            InitialSize = 128 * 1024 * 1024, // 128 MB
            MaxSize = 2L * 1024 * 1024 * 1024, // 2 GB

            PersistenceEnabled = true
          },

          // Establish a separate data region for the TAG file buffer queue
          DataRegionConfigurations = new List<DataRegionConfiguration> {new DataRegionConfiguration {Name = "BufferQueue", InitialSize = 64 * 1024 * 1024, MaxSize = 64 * 1024 * 1024, PersistenceEnabled = true}}
        },
        JvmOptions = new List<string>() {"-DIGNITE_QUIET=false", "-Djava.net.preferIPv4Stack=true", "-XX:+UseG1GC"},
        PublicThreadPoolSize = 500,
        WorkDirectory = Path.Combine(@"c:\temp", "ErrorReproducer")
      };

      var igniteServer = Ignition.Start(cfgServer);

      // make the client
      var cfgClient = new IgniteConfiguration()
      {
        IgniteInstanceName = "Client",
        ClientMode = true,
        JvmOptions = new List<string>() {"-DIGNITE_QUIET=false", "-Djava.net.preferIPv4Stack=true", "-XX:+UseG1GC"},
        JvmMaxMemoryMb = 1024,
        JvmInitialMemoryMb = 512,

        // Set an Ignite metrics heartbeat of 10 seconds
        MetricsLogFrequency = new TimeSpan(0, 0, 0, 10),
        PublicThreadPoolSize = 500,
        PeerAssemblyLoadingMode = PeerAssemblyLoadingMode.Disabled,
      };

      var igniteClient = Ignition.Start(cfgClient);

      // Activate the grid
      igniteServer.GetCluster().SetActive(true);

      // Set up the caches
      var cacheCfgServer = new CacheConfiguration {Name = "BufferQueueCache", KeepBinaryInStore = true, CacheMode = CacheMode.Partitioned, DataRegionName = "BufferQueue"};
      cacheServer = igniteServer.GetOrCreateCache<string, byte[]>(cacheCfgServer);

      var cacheCfgClient = new CacheConfiguration {Name = "BufferQueueCache", KeepBinaryInStore = true, CacheMode = CacheMode.Partitioned};
      cacheClient = igniteClient.GetOrCreateCache<string, byte[]>(cacheCfgClient);

      // Create a task to stream a collection of file into the cache
      var t1 = Task.Run(() =>
      {
        var rnd = new Random(0);

        for (var i = 0; i < 20000; i++)
        {
          var bytes = new byte[rnd.Next() % (35 * 1024) + 1024];
          rnd.NextBytes(bytes);

          cacheClient.PutIfAbsent($"Item {i}", bytes);
          Console.WriteLine($"Put item {i} with {bytes.Length} bytes");
        }
      });

      // Create a task to consume the from the cache with a continuous query
      var t2 = Task.Run(() =>
      {
        var queryHandle = cacheServer.QueryContinuous
        (new ContinuousQuery<string, byte[]>(new LocalListener()) {Local = true, Filter = new RemoteFilter()},
         new ScanQuery<string, byte[]> {Local = true, Filter = new RemoteFilter()});

        // Perform the initial query to grab all existing elements and add them to the grouper
        // All processing should happen on the remote node in the implementation of the remote filter
        foreach (var item in queryHandle.GetInitialQueryCursor())
        {
          Console.WriteLine($"A cache entry ({item.Key}) from the buffer queue was passed back to the local scan query rather than intercepted by the remote filter");
        }
      });

      Task.WhenAll(new List<Task> {t1, t2}).Wait();

      Console.WriteLine("Completed");
    }
  }
}
