using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Extensions;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.SiteModels
{
  /// <summary>
  /// Manages the life cycle of activities in the project rebuilder across the set of projects being rebuilt
  /// </summary>
  public class SiteModelRebuilderManager : ISiteModelRebuilderManager
  {
    private static ILogger _log = Logging.Logger.CreateLogger<SiteModelRebuilderManager>();

    /// <summary>
    /// The collection of rebuilder the manager is looking after
    /// </summary>
    private Dictionary<Guid, (ISiteModelRebuilder, Task<IRebuildSiteModelMetaData>)> Rebuilders = new Dictionary<Guid, (ISiteModelRebuilder, Task<IRebuildSiteModelMetaData>)>();

    /// <summary>
    /// The storage proxy cache for the rebuilder to use for tracking metadata
    /// </summary>
    private IStorageProxyCache<INonSpatialAffinityKey, IRebuildSiteModelMetaData> MetadataCache { get; }

    /// <summary>
    /// The storage proxy cache for the rebuilder to use to store names of TAG files requested from S3
    /// </summary>
    public IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper> FilesCache { get; }

    public SiteModelRebuilderManager()
    {
      MetadataCache = DIContext.Obtain<Func<RebuildSiteModelCacheType, IStorageProxyCacheCommit>>()(RebuildSiteModelCacheType.Metadata)
        as IStorageProxyCache<INonSpatialAffinityKey, IRebuildSiteModelMetaData>;

      FilesCache = DIContext.Obtain<Func<RebuildSiteModelCacheType, IStorageProxyCacheCommit>>()(RebuildSiteModelCacheType.KeyCollections)
         as IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>;
    }

    public bool Rebuild(Guid projectUid, bool archiveTAGFiles)
    {
      _log.LogInformation($"Site model rebuilder executing rebuild for project {projectUid}, archiving tag files = {archiveTAGFiles}");

      // Check if there is an existing rebuilder 
      if (Rebuilders.TryGetValue(projectUid, out var existingRebuilder))
      {
        _log.LogError($"A site model rebuilder for project {projectUid} is already present, current phase is {existingRebuilder.Item1.Metadata.Phase}");
        return false;
      }

      var rebuilder = DIContext.Obtain<Func<Guid, bool, ISiteModelRebuilder>>()(projectUid, archiveTAGFiles);
      // Inject caches
      rebuilder.MetadataCache = MetadataCache;
      rebuilder.FilesCache = FilesCache;

      lock (Rebuilders)
      {
        Rebuilders.Add(projectUid, (rebuilder, rebuilder.ExecuteAsync()));
      }
      return true;
    }

    /// <summary>
    /// Accepts a builder instantiated out side the manager to be handed to the manager to look after.
    /// Note: The manager wil NOT manage life cycle initiation of the rebuilder passed to it and will
    ///       NOT create a Task to represent it's execution, nor will it inject the caches into the passed builder.
    ///       Use Rebuild() if this behaviour is required
    /// This call will fail if there is a rebuilder for the same project already present
    /// </summary>
    public bool AddRebuilder(ISiteModelRebuilder rebuilder)
    {
      // Check if there is an existing rebuilder 
      if (Rebuilders.TryGetValue(rebuilder.ProjectUid, out var existingRebuilder))
      {
        _log.LogError($"A site model rebuilder for project {rebuilder.ProjectUid} is already present, current phase is {existingRebuilder.Item1.Metadata.Phase}");
        return false;
      }

      lock (Rebuilders)
      {
        Rebuilders.Add(rebuilder.ProjectUid, (rebuilder, null));
      }

      return true;
    }

    /// <summary>
    /// Aborts all rebuilders known by the manager
    /// </summary>
    public void AbortAll()
    {
      lock (Rebuilders)
      {
        Rebuilders.ForEach(x => x.Value.Item1.Abort());
      }
    }

    /// <summary>
    /// The total number of rebuilders being managed by the rebuild project manager
    /// </summary>
    public int RebuildCount()
    {
      lock (Rebuilders)
      {
        return Rebuilders.Count;
      }
    }

    /// <summary>
    /// Supplies a vector of meta data state relating to project builders present in the manager
    /// </summary>
    public List<IRebuildSiteModelMetaData> GetRebuildersState()
    {
      lock (Rebuilders)
      {
        return Rebuilders.Values.Select(x => x.Item1.Metadata).ToList();
      }
    }

    /// <summary>
    /// Handles an event generated from the TAG file processor that a TAG file has been processed with the notify rebuilder flag set on it
    /// </summary>
    public void TAGFileProcessed(Guid projectUid, IProcessTAGFileResponseItem[] responseItems)
    {
      lock (Rebuilders)
      {
        if (Rebuilders.TryGetValue(projectUid, out var rebuilder))
        {
          _log.LogWarning($"Site model rebuilder manager received {responseItems.Length} TAG file notifications for {projectUid}");

          rebuilder.Item1.TAGFilesProcessed(responseItems);
        }
        else
        {
          _log.LogWarning($"Site model rebuilder manager found no active rebuilder for project {projectUid}");
        }
      }
    }
  }
}
