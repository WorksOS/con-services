using System;
using System.IO;
using System.Threading.Tasks;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using Microsoft.Extensions.Logging;
using VSS.TRex.SiteModels.Interfaces;
using Nito.AsyncEx.Synchronous;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common.Exceptions;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using VSS.TRex.Common.Interfaces.Interfaces;

namespace VSS.TRex.Designs
{ 
  public class DesignFiles : IDesignFiles
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<DesignFiles>();

    public const ulong DEFAULT_DESIGN_ELEVATION_CACHE_SIZE = 1 * 1024 * 1024 * 1024;

    /// <summary>
    /// The lock specifically used to serialize operations that evict designs from the cache in order to make space for others
    /// </summary>
    private readonly object _freeSpaceAssuranceLock = new object();

    /// <summary>
    /// The lock specifically used to serialize the core design file loading operation
    /// </summary>
    private readonly object _designFileLoadExclusivityLock = new object();

    /// <summary>
    /// The collection of designs that are currently present in the cache
    /// </summary>
    private readonly ConcurrentDictionary<Guid, DesignCacheItemMetaData> _designs = new ConcurrentDictionary<Guid, DesignCacheItemMetaData>();

    private long _designsCacheSize;

    /// <summary>
    /// The total size of all cached items present
    /// </summary>
    public long DesignsCacheSize { get => _designsCacheSize; }

    /// <summary>
    /// The amount of memory in the design cache available to store additional designs, in bytes
    /// </summary>
    public long FreeSpaceInCache => MaxDesignsCacheSize - DesignsCacheSize;

    /// <summary>
    /// The maximum amount of memory available to store cached designs, in bytes
    /// </summary>
    public long MaxDesignsCacheSize { get; private set; } = (long)DIContext.Obtain<IConfigurationStore>().GetValueUlong("TREX_DESIGN_ELEVATION_CACHE_SIZE", DEFAULT_DESIGN_ELEVATION_CACHE_SIZE);

    public int MaxWaitIterationsDuringDesignEviction { get; private set; } = 100;

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public DesignFiles()
    {
    }

    /// <summary>
    /// Constructs a DesignFiles instance with a specified maximum size for cached designs
    /// </summary>
    public DesignFiles(long maxDesignsCacheSize, int maxWaitIterationsDuringDesignEviction)
    {
      MaxDesignsCacheSize = maxDesignsCacheSize;
      MaxWaitIterationsDuringDesignEviction = maxWaitIterationsDuringDesignEviction;
    }

    /// <summary>
    /// Removes a design from cache and storage
    /// </summary>
    public bool RemoveDesignFromCache(Guid designUid, IDesignBase design, Guid siteModelUid, bool deleteFile)
    {
      _log.LogDebug($"Removing design UID {designUid}, filename = '{design.FileName}'");

      if (deleteFile)
        design.RemoveFromStorage(siteModelUid, Path.GetFileName(design.FileName));

      lock (_designs)
      {
        return _designs.TryRemove(designUid, out _);
      }
    }

    /// <summary>
    /// Acquire a lock and reference to the design referenced by the given design descriptor
    /// </summary>
    public IDesignBase Lock(Guid designUid, ISiteModelBase siteModelBase, double cellSize, out DesignLoadResult loadResult)
    {
      loadResult = DesignLoadResult.UnknownFailure;

      IDesignBase design = null;

      try
      {
        if (!(siteModelBase is ISiteModel siteModel))
        {
          return null;
        }

        DesignCacheItemMetaData designMetaData;
        lock (_designs)
        {
          if (_designs.TryGetValue(designUid, out designMetaData))
          {
            design = designMetaData.Design;
          }

          if (design == null)
          {
            _log.LogDebug($"Design UID {designUid} not present in cached designs for site model {siteModel.ID}");

            // Verify the design does exist in either the designs, surveyed surface or alignment lists for the site model
            var designRef = siteModel.Designs.Locate(designUid);
            var descriptor = designRef?.DesignDescriptor;

            if (descriptor != null)
            {
              _log.LogDebug($"Surface design UID {designUid}, filename = {descriptor.FileName} needs to be loaded");
            }
            else
            {
              var surveyedSurfaceRef = siteModel.SurveyedSurfaces.Locate(designUid);
              descriptor = surveyedSurfaceRef?.DesignDescriptor;

              if (descriptor != null)
              {
                _log.LogDebug($"Surveyed surface design UID {designUid}, filename = {descriptor.FileName} needs to be loaded");
              }
              else
              {
                var alignmentDesignRef = siteModel.Alignments.Locate(designUid);
                descriptor = alignmentDesignRef?.DesignDescriptor;

                if (descriptor != null)
                {
                  _log.LogDebug($"Alignment design UID {designUid}, filename = {descriptor.FileName} needs to be loaded");
                }
              }
            }

            if (descriptor == null)
            {
              _log.LogWarning($"Failed to locate design {designUid} for site model with ID {siteModel.ID}");
              loadResult = DesignLoadResult.DesignDoesNotExist;
              return null;
            }

            _log.LogDebug($"Creating entry for design UID {designUid}, filename = {descriptor.FileName} within the in-memory cache");

            // Add a design in the 'IsLoading state' to control multiple access to this design until it is fully loaded
            design = DIContext.ObtainRequired<IDesignClassFactory>().NewInstance(designUid, Path.Combine(FilePathHelper.GetTempFolderForProject(siteModel.ID), descriptor.FileName), cellSize, siteModel.ID);
            design.IsLoading = true;

            // Set the initial size in cache to 0 pending the load of the design
            designMetaData = new DesignCacheItemMetaData(design, 0);

            if (!_designs.TryAdd(designUid, designMetaData))
            {
              _log.LogError($"Failed to add the design cache entry design UID {designUid}, filename = {descriptor.FileName} to the concurrent dictionary");
            }

            // At this point the lock on the designs is released. There is now a design in the cache representing it which is in 'loading' state
          }
        }

        // Obtain a design specific lock so other design requests are not held up while this design is being loaded
        lock (design)
        {
          design.WindLock();

          if (!design.IsLoading)
          {
            loadResult = DesignLoadResult.Success;

            // Touch the design metadata to make this design the MRU design (and last in the queue for eviction)
            designMetaData.Touch();

            return design;
          }

          // The design is in 'loading' state, and needs to be loaded here.
          if (!File.Exists(design.FileName))
          {
            _log.LogDebug($"Getting design UID {designUid}, filename = {design.FileName} from persistent store (S3)");

            try
            {
              var sw = Stopwatch.StartNew();
              loadResult = design.LoadFromStorage(siteModel.ID, Path.GetFileName(design.FileName), Path.GetDirectoryName(design.FileName), true);
              _log.LogDebug($"design.LoadFromStorage() completed in {sw.Elapsed}");
            }
            catch (Exception e)
            {
              _log.LogError(e, $"Exception getting design UID {designUid}, filename = {design.FileName} from persistent store (S3)");
              loadResult = DesignLoadResult.UnknownFailure;
            }

            if (loadResult != DesignLoadResult.Success)
            {
              _log.LogWarning($"Failed to load design {designUid} from file {design.FileName}, from persistent storage for site model with ID {siteModel.ID}");

              if (!_designs.TryRemove(designUid, out _))
              {
                _log.LogError($"Failed to remove  the design cache entry design UID {designUid}, filename = {design.FileName} from the concurrent dictionary");
              }

              return null;
            }
          }

          var fileInfo = new FileInfo(design.FileName ?? string.Empty);
          _log.LogDebug($"Loading design UID {designUid}, filename = {design.FileName}, size = {fileInfo.Length} bytes");

          lock (_designFileLoadExclusivityLock)
          {
            var sw = Stopwatch.StartNew();
            loadResult = design.LoadFromFile(design.FileName);

            if (loadResult != DesignLoadResult.Success)
            {
              _log.LogWarning($"Failed to load design {designUid} from file {design.FileName}, from local storage for site model with ID {siteModel.ID}, with error {loadResult}");

              if (!_designs.TryRemove(designUid, out _))
              {
                _log.LogError($"Failed to remove  the design cache entry design UID {designUid}, filename = {design.FileName} from the concurrent dictionary");
              }

              return null;
            }

            _log.LogInformation($"Design {designUid} successfully loaded from file {design.FileName} in {sw.Elapsed}");

            // Ensure there is enough space in the cache to accomodate the newly loaded file
            if (!EnsureSufficientSpaceToLoadDesign(design.SizeInCache()))
            {
              _log.LogError("Unable to ensure sufficient free space to add the design to the cache - removing it and failing the Lock()_ operation");

              if (!_designs.TryRemove(designUid, out _))
              {
                _log.LogError("Failed to remove design from dictionary after failure to acquire sufficient memory");
              }

              loadResult = DesignLoadResult.InsufficientMemory;
              return null;
            }

            designMetaData.SizeInCache = design.SizeInCache();
          }

          // Adjust the cached designs size to include the new design
          Interlocked.Add(ref _designsCacheSize, designMetaData.SizeInCache);

          design.IsLoading = false;
          return design;
        }
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception locking design");
        throw;
      }
    }

    /// <summary>
    /// Acquire a lock and reference to the design referenced by the given design descriptor
    /// </summary>
    public IDesignBase Lock(Guid designUid, Guid dataModelId, double cellSize, out DesignLoadResult loadResult)
    {
      var siteModel = DIContext.ObtainRequired<ISiteModels>().GetSiteModel(dataModelId);
      if (siteModel == null)
      {
        _log.LogWarning($"Failed to get site model with ID {dataModelId} for design {designUid}");
        loadResult = DesignLoadResult.SiteModelNotFound;
        return null;
      }

      return Lock(designUid, siteModel, cellSize, out loadResult);
    }

    /// <summary>
    /// Release a lock to the design referenced by the given design descriptor
    /// </summary>
    public bool UnLock(Guid designUid, IDesignBase design)
    {
      // Very simple unlock function...
      design.UnWindLock();

      return true;
    }

    /// <summary>
    /// Removes sufficient designs from the designs list to ensure that the design may be loaded
    /// If there are no available designs to remove it will block loading the design until there is
    /// </summary>
    public bool EnsureSufficientSpaceToLoadDesign(long designFileCacheSize)
    {
      lock (_freeSpaceAssuranceLock)
      {
        try
        {
          if (designFileCacheSize < FreeSpaceInCache) // There's enough space!
            return true;

          _log.LogInformation($"Freeing designs to ensure {designFileCacheSize} bytes available with {FreeSpaceInCache} bytes available");

          var iterationsLeft = MaxWaitIterationsDuringDesignEviction;
          do
          {
            if (_designs.Count == 0) // If there are no designs in the cache then permit the cache to be loaded, even if it exceeds the cache available
              return true;

            // No? Then find some designs to victimize
            var removedDesign = false;

            lock (_designs)
            {
              // Find the oldest unlocked design
              DesignCacheItemMetaData oldestUnlockedDesign = null;
              var oldestDate = DateTime.UtcNow;
              foreach (var designMetaData in _designs.Values)
              {
                if (designMetaData.LastTouchedDate <= oldestDate && !designMetaData.Design.Locked)
                {
                  oldestDate = designMetaData.LastTouchedDate;
                  oldestUnlockedDesign = designMetaData;
                }
              }

              if (oldestUnlockedDesign != null)
              {
                var design = oldestUnlockedDesign.Design;

                _log.LogInformation($"{nameof(EnsureSufficientSpaceToLoadDesign)}: Removing design {design.FileName}/{design.ProjectUid} from cache to make room");

                if (_designs.TryRemove(design.DesignUid, out _))
                {
                  _log.LogInformation($"Removed design {design.FileName} in project {design.ProjectUid} from designs cache");
                  removedDesign = true;

                  // Adjust cached designs size
                  Interlocked.Add(ref _designsCacheSize, -oldestUnlockedDesign.SizeInCache);
                }
                else
                {
                  _log.LogInformation($"{nameof(EnsureSufficientSpaceToLoadDesign)}: Failed to remove design from cache concurrent dictionary");
                }
              }
            }

            if (designFileCacheSize < FreeSpaceInCache)
            {
              _log.LogError($"{FreeSpaceInCache} bytes are now available after design eviction for a design requiring {designFileCacheSize} bytes");
              return true;
            }

            if (!removedDesign)
            {
              _log.LogDebug($"Spinning waiting for design to be released, iterationsLeft = {iterationsLeft}");

              // Still no joy? Spin until a design is released
              Thread.Sleep(1000);
            }

            if (iterationsLeft-- <= 0)
            {
              throw new TRexException($"Failed to ensure sufficient space after waiting for {MaxWaitIterationsDuringDesignEviction} periods");
            }
          } while (FreeSpaceInCache < designFileCacheSize);

          _log.LogError($"Failed to ensure {designFileCacheSize} bytes available, currently {FreeSpaceInCache} bytes are available");

          return false;
        }
        catch (Exception e)
        {
          _log.LogError(e, $"{nameof(EnsureSufficientSpaceToLoadDesign)}: Exception occurred {e.Message}");
          return false;
        }
      }
    }

    public int NumDesignsInCache()
    {
      lock (_designs)
      {
        return _designs.Count;
      }
    }
  }
}
