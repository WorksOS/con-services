using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3.Model.Internal.MarshallTransformations;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.SiteModels
{
  /// <summary>
  /// Manages the life cycle of activities in the project rebuilder across the set of projects being rebuilt
  /// </summary>
  public class SiteModelRebuilderManager : ISiteModelRebuilderManager
  {
    private static ILogger _log = Logging.Logger.CreateLogger<SiteModelRebuilderManager>();

    private Dictionary<Guid, (SiteModelRebuilder, Task<IRebuildSiteModelMetaData>)> Rebuilders = new Dictionary<Guid, (SiteModelRebuilder, Task<IRebuildSiteModelMetaData>)>();

    /// <summary>
    /// The storage proxy cache for the rebuilder to use for tracking metadata
    /// </summary>
    private IStorageProxyCache<INonSpatialAffinityKey, IRebuildSiteModelMetaData> MetadataCache { get; set; }

    /// <summary>
    /// The storage proxy cache for the rebuilder to use to store names of TAG files requested from S3
    /// </summary>
    public IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper> FilesCache { get; set; }

    public SiteModelRebuilderManager()
    {
      MetadataCache = DIContext.Obtain<Func<RebuildSiteModelCacheType, IStorageProxyCacheCommit>>()(RebuildSiteModelCacheType.Metedata)
        as IStorageProxyCache<INonSpatialAffinityKey, IRebuildSiteModelMetaData>;

      FilesCache = DIContext.Obtain<Func<RebuildSiteModelCacheType, IStorageProxyCacheCommit>>()(RebuildSiteModelCacheType.KeyCollections)
         as IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>;
    }

    public bool Rebuild(Guid projectUid, bool archiveTAGFiles)
    {
      _log.LogInformation($"Site model rebuilder executing rebuild for proejct {projectUid}, archiving tag files = {archiveTAGFiles}");

      // Check if there is an existing rebuilder 
      if (Rebuilders.TryGetValue(projectUid, out var existingRebuilder))
      {
        _log.LogError($"A site model rebuilder for project {projectUid} is already present, current phase is {existingRebuilder.Item1.Metadata.Phase}");
        return false;
      }

      var rebuilder = new SiteModelRebuilder(projectUid, archiveTAGFiles)
      {
        // Inject cahces
        MetadataCache = MetadataCache,
        FilesCache = FilesCache
      };

      lock (Rebuilders)
      {
        Rebuilders.Add(projectUid, (rebuilder, rebuilder.ExecuteAsync()));
      }
      return true;
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

    public List<IRebuildSiteModelMetaData> GetRebuilersState()
    {
      lock (Rebuilders)
      {
        return Rebuilders.Values.Select(x => x.Item1.Metadata).ToList();
      }
    }
  }
}
