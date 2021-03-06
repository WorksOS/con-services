﻿using System;
using System.IO;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.Tools.KeyScanner
{
  public class KeyScanner
  {
    private void writeKeys(string title, StreamWriter writer, ICache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper> cache)
    {
      int count = 0;

      writer.WriteLine(title);
      writer.WriteLine("###############");
      writer.WriteLine();

      if (cache == null)
      {
        return;
      }

      var scanQuery = new ScanQuery<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>();
      var queryCursor = cache.Query(scanQuery);
      scanQuery.PageSize = 1; // Restrict the number of keys requested in each page to reduce memory consumption

      foreach (var cacheEntry in queryCursor)
      {
        writer.WriteLine($"{count++}:{cacheEntry.Key}, size = {cacheEntry.Value.Bytes.Length}");
      }

      writer.WriteLine();
    }

    private void WriteKeysSpatial(string title, StreamWriter writer, ICache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper> cache)
    {
      int count = 0;

      writer.WriteLine(title);
      writer.WriteLine("###############");
      writer.WriteLine();

      if (cache == null)
      {
        return;
      }

      var scanQuery = new ScanQuery<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>
      {
        PageSize = 1 // Restrict the number of keys requested in each page to reduce memory consumption
      };

      var queryCursor = cache.Query(scanQuery);

      foreach (var cacheEntry in queryCursor)
      {
        writer.WriteLine($"{count++}:{cacheEntry.Key}, size = {cacheEntry.Value.Bytes.Length}");
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
      var queryCursor = cache.Query(scanQuery);
      scanQuery.PageSize = 1; // Restrict the number of keys requested in each page to reduce memory consumption

      foreach (var cacheEntry in queryCursor)
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
      var queryCursor = cache.Query(scanQuery);
      scanQuery.PageSize = 1; // Restrict the number of keys requested in each page to reduce memory consumption

      foreach (var cacheEntry in queryCursor)
      {
        writer.WriteLine($"{count++}:{cacheEntry.Key}: retiree count = {cacheEntry.Value.SegmentKeys.Length}");

        foreach (var key in cacheEntry.Value.SegmentKeys)
          writer.WriteLine($"  [{key.SubGridX}x{key.SubGridY}]: {key.SegmentStartDateTicks}={key.SegmentEndDateTicks}");
      }

      writer.WriteLine();
    }

    private void writeSiteModelChangeMapQueueKeys(string title, StreamWriter writer, ICache<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper> cache)
    {
      var count = 0;

      writer.WriteLine(title);
      writer.WriteLine("#####################");
      writer.WriteLine();

      if (cache == null)
      {
        return;
      }

      var scanQuery = new ScanQuery<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper>();
      var queryCursor = cache.Query(scanQuery);
      scanQuery.PageSize = 1; // Restrict the number of keys requested in each page to reduce memory consumption

      foreach (var cacheEntry in queryCursor)
      {
        writer.WriteLine($"{count++}:{cacheEntry.Key}: size = {cacheEntry.Value.Bytes.Length}");
      }

      writer.WriteLine();
    }

    public void dumpKeysToFile(StorageMutability mutability, string fileName)
    {
      try
      {
        var ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(mutability);

        if (ignite == null)
        {
          Console.WriteLine($@"----> No ignite reference for {TRexGrids.GridName(mutability)} grid");
          return;
        }

        using (var outFile = new FileStream(fileName, FileMode.Create))
        {
          using (var writer = new StreamWriter(outFile) { NewLine = "\r\n" })
          {
            if (mutability == StorageMutability.Immutable)
            {
              Console.WriteLine($"----> Writing keys for {TRexCaches.ImmutableNonSpatialCacheName()}");
              try
              {
                writeKeys(TRexCaches.ImmutableNonSpatialCacheName(), writer, ignite.GetCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.ImmutableNonSpatialCacheName()));
              }
              catch (Exception E)
              {
                writer.WriteLine($"Exception occurred: {E.Message}");
              }

              Console.WriteLine($"----> Writing keys for {TRexCaches.DesignTopologyExistenceMapsCacheName()}");
              try
              {
                writeKeys(TRexCaches.DesignTopologyExistenceMapsCacheName(), writer, ignite.GetCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.DesignTopologyExistenceMapsCacheName()));
              }
              catch (Exception E)
              {
                writer.WriteLine($"Exception occurred: {E.Message}");
              }

              Console.WriteLine($"----> Writing keys for {TRexCaches.SpatialSubGridDirectoryCacheName(StorageMutability.Immutable)}");
              try
              {
                WriteKeysSpatial(TRexCaches.SpatialSubGridDirectoryCacheName(StorageMutability.Immutable), writer, ignite.GetCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.SpatialSubGridDirectoryCacheName(StorageMutability.Immutable)));
              }
              catch (Exception E)
              {
                writer.WriteLine($"Exception occurred: {E.Message}");
              }

              Console.WriteLine($"----> Writing keys for {TRexCaches.SpatialSubGridSegmentCacheName(StorageMutability.Immutable)}");
              try
              {
                WriteKeysSpatial(TRexCaches.SpatialSubGridSegmentCacheName(StorageMutability.Immutable), writer, ignite.GetCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.SpatialSubGridSegmentCacheName(StorageMutability.Immutable)));
              }
              catch (Exception E)
              {
                writer.WriteLine($"Exception occurred: {E.Message}");
              }

              Console.WriteLine($"----> Writing keys for {TRexCaches.SiteModelChangeMapsCacheName()}");
              try
              {
                writeSiteModelChangeMapQueueKeys(TRexCaches.SiteModelChangeMapsCacheName(), writer, ignite.GetCache<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.SiteModelChangeMapsCacheName()));
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
                writeKeys(TRexCaches.MutableNonSpatialCacheName(), writer, ignite.GetCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.MutableNonSpatialCacheName()));
              }
              catch (Exception E)
              {
                writer.WriteLine($"Exception occurred: {E.Message}");
              }

              Console.WriteLine($"----> Writing keys for {TRexCaches.SpatialSubGridDirectoryCacheName(StorageMutability.Mutable)}");
              try
              {
                WriteKeysSpatial(TRexCaches.SpatialSubGridDirectoryCacheName(StorageMutability.Mutable), writer, ignite.GetCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.SpatialSubGridDirectoryCacheName(StorageMutability.Mutable)));
              }
              catch (Exception E)
              {
                writer.WriteLine($"Exception occurred: {E.Message}");
              }

              Console.WriteLine($"----> Writing keys for {TRexCaches.SpatialSubGridSegmentCacheName(StorageMutability.Mutable)}");
              try
              {
                WriteKeysSpatial(TRexCaches.SpatialSubGridSegmentCacheName(StorageMutability.Mutable), writer, ignite.GetCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.SpatialSubGridSegmentCacheName(StorageMutability.Mutable)));
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
