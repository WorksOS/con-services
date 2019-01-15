using System;
using System.IO;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.Tools.KeyScanner
{
  public class KeyScanner
  {
    private void writeKeys(string title, StreamWriter writer, ICache<INonSpatialAffinityKey, byte[]> cache)
    {
      int count = 0;

      writer.WriteLine(title);
      writer.WriteLine("###############");
      writer.WriteLine();

      if (cache == null)
      {
        return;
      }

      var scanQuery = new ScanQuery<INonSpatialAffinityKey, byte[]>();
      IQueryCursor<ICacheEntry<INonSpatialAffinityKey, byte[]>> queryCursor = cache.Query(scanQuery);
      scanQuery.PageSize = 1; // Restrict the number of keys requested in each page to reduce memory consumption

      foreach (ICacheEntry<INonSpatialAffinityKey, byte[]> cacheEntry in queryCursor)
      {
        writer.WriteLine($"{count++}:{cacheEntry.Key}, size = {cacheEntry.Value.Length}");
      }

      writer.WriteLine();
    }

    private void WriteKeysSpatial(string title, StreamWriter writer, ICache<ISubGridSpatialAffinityKey, byte[]> cache)
    {
      int count = 0;

      writer.WriteLine(title);
      writer.WriteLine("###############");
      writer.WriteLine();

      if (cache == null)
      {
        return;
      }

      var scanQuery = new ScanQuery<ISubGridSpatialAffinityKey, byte[]>
      {
        PageSize = 1 // Restrict the number of keys requested in each page to reduce memory consumption
      };

      IQueryCursor<ICacheEntry<ISubGridSpatialAffinityKey, byte[]>> queryCursor = cache.Query(scanQuery);

      foreach (ICacheEntry<ISubGridSpatialAffinityKey, byte[]> cacheEntry in queryCursor)
      {
        writer.WriteLine($"{count++}:{cacheEntry.Key}, size = {cacheEntry.Value.Length}");
      }

      writer.WriteLine();
    }

    private void writeTAGFileBufferQueueKeys(string title, StreamWriter writer, ICache<ITAGFileBufferQueueKey, TAGFileBufferQueueItem> cache)
    {
      int count = 0;

      writer.WriteLine(title);
      writer.WriteLine("#####################");
      writer.WriteLine();

      if (cache == null)
      {
        return;
      }

      var scanQuery = new ScanQuery<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>();
      IQueryCursor<ICacheEntry<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>> queryCursor = cache.Query(scanQuery);
      scanQuery.PageSize = 1; // Restrict the number of keys requested in each page to reduce memory consumption

      foreach (ICacheEntry<ITAGFileBufferQueueKey, TAGFileBufferQueueItem> cacheEntry in queryCursor)
      {
        writer.WriteLine($"{count++}:{cacheEntry.Key}, size = {cacheEntry.Value.Content.Length}");
      }

      writer.WriteLine();
    }

    private void writeSegmentRetireeQueueKeys(string title, StreamWriter writer, ICache<ISegmentRetirementQueueKey, SegmentRetirementQueueItem> cache)
    {
      int count = 0;

      writer.WriteLine(title);
      writer.WriteLine("#####################");
      writer.WriteLine();

      if (cache == null)
      {
        return;
      }

      var scanQuery = new ScanQuery<ISegmentRetirementQueueKey, SegmentRetirementQueueItem>();
      IQueryCursor<ICacheEntry<ISegmentRetirementQueueKey, SegmentRetirementQueueItem>> queryCursor = cache.Query(scanQuery);
      scanQuery.PageSize = 1; // Restrict the number of keys requested in each page to reduce memory consumption

      foreach (ICacheEntry<ISegmentRetirementQueueKey, SegmentRetirementQueueItem> cacheEntry in queryCursor)
      {
        writer.WriteLine($"{count++}:{cacheEntry.Key}: retiree count = {cacheEntry.Value.SegmentKeys.Length}");

        foreach (var key in cacheEntry.Value.SegmentKeys)
          writer.WriteLine($"  [{key.SubGridX}x{key.SubGridY}]: {key.SegmentIdentifier}");
      }

      writer.WriteLine();
    }

    public void dumpKeysToFile(StorageMutability mutability, string fileName)
    {
      try
      {
        IIgnite ignite = DIContext.Obtain<ITRexGridFactory>().Grid(mutability);

        if (ignite == null)
        {
          Console.WriteLine($@"----> No ignite reference for {TRexGrids.GridName(mutability)} grid");
          return;
        }

        using (var outFile = new FileStream(fileName, FileMode.Create))
        {
          using (var writer = new StreamWriter(outFile))
          {
            if (mutability == StorageMutability.Immutable)
            {
              Console.WriteLine($"----> Writing keys for {TRexCaches.ImmutableNonSpatialCacheName()}");
              try
              {
                writeKeys(TRexCaches.ImmutableNonSpatialCacheName(), writer, ignite.GetCache<INonSpatialAffinityKey, byte[]>(TRexCaches.ImmutableNonSpatialCacheName()));
              }
              catch (Exception E)
              {
                writer.WriteLine($"Exception occurred: {E.Message}");
              }

              Console.WriteLine($"----> Writing keys for {TRexCaches.DesignTopologyExistenceMapsCacheName()}");
              try
              {
                writeKeys(TRexCaches.DesignTopologyExistenceMapsCacheName(), writer, ignite.GetCache<INonSpatialAffinityKey, byte[]>(TRexCaches.DesignTopologyExistenceMapsCacheName()));
              }
              catch (Exception E)
              {
                writer.WriteLine($"Exception occurred: {E.Message}");
              }

              Console.WriteLine($"----> Writing keys for {TRexCaches.ImmutableSpatialCacheName()}");
              try
              {
                WriteKeysSpatial(TRexCaches.ImmutableSpatialCacheName(), writer, ignite.GetCache<ISubGridSpatialAffinityKey, byte[]>(TRexCaches.ImmutableSpatialCacheName()));
              }
              catch (Exception E)
              {
                writer.WriteLine($"Exception occurred: {E.Message}");
              }
            }

            if (mutability == StorageMutability.Mutable)
            {
              Console.WriteLine($"----> Writing keys for {TRexCaches.MutableNonSpatialCacheName()}");
              try
              {
                writeKeys(TRexCaches.MutableNonSpatialCacheName(), writer, ignite.GetCache<INonSpatialAffinityKey, byte[]>(TRexCaches.MutableNonSpatialCacheName()));
              }
              catch (Exception E)
              {
                writer.WriteLine($"Exception occurred: {E.Message}");
              }

              Console.WriteLine($"----> Writing keys for {TRexCaches.MutableSpatialCacheName()}");
              try
              {
                WriteKeysSpatial(TRexCaches.MutableSpatialCacheName(), writer, ignite.GetCache<ISubGridSpatialAffinityKey, byte[]>(TRexCaches.MutableSpatialCacheName()));
              }
              catch (Exception E)
              {
                writer.WriteLine($"Exception occurred: {E.Message}");
              }

              Console.WriteLine($"----> Writing keys for {TRexCaches.TAGFileBufferQueueCacheName()}");
              try
              {
                writeTAGFileBufferQueueKeys(TRexCaches.TAGFileBufferQueueCacheName(), writer, ignite.GetCache<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>(TRexCaches.TAGFileBufferQueueCacheName()));
              }
              catch (Exception E)
              {
                writer.WriteLine($"Exception occurred: {E.Message}");
              }

              Console.WriteLine($"----> Writing keys for {TRexCaches.SegmentRetirementQueueCacheName()}");
              try
              {
                writeSegmentRetireeQueueKeys(TRexCaches.SegmentRetirementQueueCacheName(), writer, ignite.GetCache<ISegmentRetirementQueueKey, SegmentRetirementQueueItem>(TRexCaches.SegmentRetirementQueueCacheName()));
              }
              catch (Exception E)
              {
                writer.WriteLine($"Exception occurred: {E.Message}");
              }
            }
          }
        }
      }
      catch (Exception ee)
      {
        Console.WriteLine(ee.ToString());
      }
    }
  }

}
