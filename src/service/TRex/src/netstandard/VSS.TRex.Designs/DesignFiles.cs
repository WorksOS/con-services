using System;
using System.Collections.Generic;
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

namespace VSS.TRex.Designs
{ 
/*
TDesignFiles = class(TObject)
  public
    function GetCombinedSubgridIndexStream(const Surfaces: TICGroundSurfaceDetailsList;
                                           const ProjectUid : Int64; const ACellSize: Double;
                                           out MS: TMemoryStream): Boolean;
end;
*/

  public class DesignFiles : IDesignFiles
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<DesignFiles>();

    public const ulong DEFAULT_DESIGN_ELEVATION_CACHE_SIZE = 1 * 1024 * 1024 * 1024;

    /// <summary>
    /// The collection of designs that are currently present in the cache
    /// </summary>
    private readonly Dictionary<Guid, IDesignBase> _designs = new Dictionary<Guid, IDesignBase>();

    /// <summary>
    /// The total size of all cached items present
    /// </summary>
    public long DesignsCacheSize { get; private set; }

    /// <summary>
    /// The amount of memory in the design cache available to store additional designs, in bytes
    /// </summary>
    public long FreeSpaceInCache => MaxDesignsCacheSize - DesignsCacheSize;

    /// <summary>
    /// The maximum amount of memory available to store cached designs, in bytes
    /// </summary>
    public long MaxDesignsCacheSize { get; private set; } = (long)DIContext.Obtain<IConfigurationStore>().GetValueUlong("TREX_DESIGN_ELEVATION_CACHE_SIZE", DEFAULT_DESIGN_ELEVATION_CACHE_SIZE);

    /// <summary>
    /// Removes a design from cache and storage
    /// </summary>
    public bool RemoveDesignFromCache(Guid designUid, IDesignBase design, Guid siteModelUid, bool deleteFile)
    {
      _log.LogDebug($"Removing design UID {designUid}, filename = '{design.FileName}'");

      if (deleteFile)
        design.RemoveFromStorage(siteModelUid, Path.GetFileName(design.FileName));

      if (_designs.TryGetValue(designUid, out _))
      {
        return _designs.Remove(designUid);
      }

      return false;
    }

    /// <summary>
    /// Acquire a lock and reference to the design referenced by the given design descriptor
    /// </summary>
    public IDesignBase Lock(Guid designUid, Guid dataModelId, double cellSize, out DesignLoadResult loadResult)
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(dataModelId);
      if (siteModel == null)
      {
        _log.LogWarning($"Failed to get site model with ID {dataModelId} for design {designUid}");
        loadResult = DesignLoadResult.SiteModelNotFound;
        return null;
      }

      IDesignBase design;

      lock (_designs)
      {
        _designs.TryGetValue(designUid, out design);

        if (design == null)
        {
          _log.LogDebug($"Design UID {designUid} not present in cached designs for site model {dataModelId}");

          // Verify the design does exist in either the designs, surveyed surface or alignment lists for the site model
          var designRef = siteModel.Designs.Locate(designUid);
          var descriptor = designRef?.DesignDescriptor;

          if (descriptor != null)
          {
            _log.LogDebug($"Surface design UID {designUid}, filename = {descriptor.FileName} needs to be loaded");
          }
          else
          {
            var surveyedSurfaceRef = siteModel.SurveyedSurfaces?.Locate(designUid);
            descriptor = surveyedSurfaceRef?.DesignDescriptor;

            if (descriptor != null)
            {
              _log.LogDebug($"Surveyed surface design UID {designUid}, filename = {descriptor.FileName} needs to be loaded");
            }
            else
            {
              var alignmentDesignRef = siteModel.Alignments?.Locate(designUid);
              descriptor = alignmentDesignRef?.DesignDescriptor;

              if (descriptor != null)
              {
                _log.LogDebug($"Alignment design UID {designUid}, filename = {descriptor.FileName} needs to be loaded");
              }
            }
          }

          if (descriptor == null)
          {
            _log.LogWarning($"Failed to locate design {designUid} for site model with ID {dataModelId}");
            loadResult = DesignLoadResult.DesignDoesNotExist;
            return null;
          }

          _log.LogDebug($"Creating entry for design UID {designUid}, filename = {descriptor.FileName} within the in-memory cache");

          // Add a design in the 'IsLoading state' to control multiple access to this design until it is fully loaded
          design = DIContext.Obtain<IDesignClassFactory>().NewInstance(designUid, Path.Combine(FilePathHelper.GetTempFolderForProject(dataModelId), descriptor.FileName), cellSize, dataModelId);
          design.IsLoading = true;

          _designs.Add(designUid, design);

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
          return design;
        }

        // The design is in 'loading' state, and needs to be loaded here.
        if (!File.Exists(design.FileName))
        {
          _log.LogDebug($"Getting design UID {designUid}, filename = {design.FileName} from persistent store (S3)");

          loadResult = design.LoadFromStorage(dataModelId, Path.GetFileName(design.FileName), Path.GetDirectoryName(design.FileName), true).WaitAndUnwrapException();
          if (loadResult != DesignLoadResult.Success)
          {
            _log.LogWarning($"Failed to load design {designUid} from file {design.FileName}, from persistent storage for site model with ID {dataModelId}");
            _designs.Remove(designUid);
            return null;
          }
        }

        var fileInfo = new FileInfo(design.FileName);
        _log.LogDebug($"Loading design UID {designUid}, filename = {design.FileName}, size = {fileInfo.Length} bytes");

        // As a first approximation, ensure there is at least enough space in the cache to accomodate the load file size
        EnsureSufficientSpaceToLoadDesign(fileInfo.Length);

        loadResult = design.LoadFromFile(design.FileName);
        if (loadResult != DesignLoadResult.Success)
        {
          _log.LogWarning($"Failed to load design {designUid} from file {design.FileName}, from local storage for site model with ID {dataModelId}, with error {loadResult}");
          _designs.Remove(designUid);
          return null;
        }

        // As a second approximation, ensure there is sufficient space to contain the now loaded design
        EnsureSufficientSpaceToLoadDesign(design.SizeInCache());

        design.IsLoading = false;
        return design;
      }
    }

    /// <summary>
    /// Release a lock to the design referenced by the given design descriptor
    /// </summary>
    public bool UnLock(Guid designUid, IDesignBase design)
    {
      lock (_designs)
      {
        // Very simple unlock function...
        design.UnWindLock();

        // If the design is not locked check if there should be some maintenance of the content of the cache...
        if (design.Locked || (DesignsCacheSize <= MaxDesignsCacheSize))
          return true;

        lock (design)
        {
          if (design.Locked) // Another thread locked the design before we could acquire the lock
            return true;

          if (_designs.Remove(designUid))
          {
            DesignsCacheSize -= design.SizeInCache();
            design.Dispose();
            _log.LogInformation($"Removed design {designUid} in project {design.ProjectUid} from designs cache");
          }
          else
          {
            _log.LogError($"Failed to remove design {designUid} in project {design.ProjectUid} from designs cache");
          }
        }

        return true;
      }
    }

    /// <summary>
    /// Removes sufficient designs from the designs list to ensure that the design may be loaded
    /// If there are no available designs to remove it will block loading the design until there is
    /// </summary>
    public bool EnsureSufficientSpaceToLoadDesign(long designCacheSize)
    {
      const int MaxWaitIterations = 1000;
      try
      {
        if (designCacheSize < FreeSpaceInCache)
          return true;

        var iterationsLeft = MaxWaitIterations;
        do
        {
          if (_designs.Count == 0) // If there are no designs in the cache then permit the cache to be loaded, even if it exceeds the cache available
            return true;

          // No? Then find some designs to victimise
          var removedDesign = false;

          lock (_designs)
          {
            foreach (var design in _designs.Values)
            {
              if (design.Locked) continue;

              _log.LogInformation($"{nameof(EnsureSufficientSpaceToLoadDesign)}: Removing design {design.FileName}/{design.ProjectUid} from cache to make room");

              if (_designs.Remove(design.DesignUid))
              {
                _log.LogInformation($"Removed design {design.FileName} in project {design.ProjectUid} from designs cache");
                removedDesign = true;
                break;
              }

              _log.LogInformation($"{nameof(EnsureSufficientSpaceToLoadDesign)}: Failed to remove design from cache");
            }
          }

          if (designCacheSize < FreeSpaceInCache)
            return true;

          if (!removedDesign)
          {
            // Still no joy? Spin until a design is released
            Task.Delay(100).WaitAndUnwrapException();
          }

          if (iterationsLeft-- <= 0)
            throw new TRexException($"Failed to ensure sufficient space after waiting for {MaxWaitIterations} periods");
        } while (FreeSpaceInCache < designCacheSize);

        return false;
      }
      catch (Exception e)
      {
        _log.LogError(e, $"{nameof(EnsureSufficientSpaceToLoadDesign)}: Exception occurred {e.Message}");
        return false;
      }
    }

    public int NumDesignsInCache()
    {
      lock (_designs)
      {
        return _designs.Count;
      }
    }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public DesignFiles()
    {
    }

    public DesignFiles(long maxDesignsCacheSize)
    {
      MaxDesignsCacheSize = maxDesignsCacheSize;
    }
  }
}
