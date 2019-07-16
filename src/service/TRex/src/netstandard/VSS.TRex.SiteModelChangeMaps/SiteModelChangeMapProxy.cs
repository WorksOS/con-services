using System;
using System.Collections.Generic;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Types;

namespace VSS.TRex.SiteModelChangeMaps
{
  /// <summary>
  /// Provides a proxy for accessing site model/asset change map information
  /// </summary>
  public class SiteModelChangeMapProxy
  {
    private readonly IStorageProxyCache<ISiteModelMachineAffinityKey, byte[]> _proxyStorageCache;

    public SiteModelChangeMapProxy()
    {
      _proxyStorageCache = DIContext.Obtain<ISiteModels>().PrimaryImmutableStorageProxy.ProjectMachineCache(FileSystemStreamType.SiteModelMachineElevationChangeMap);
    }

    public SubGridTreeSubGridExistenceBitMask Get(Guid siteModelUid, Guid assetUid)
    {
      try
      {
        var cacheItem = _proxyStorageCache.Get(new SiteModelMachineAffinityKey(siteModelUid, assetUid, FileSystemStreamType.SiteModelMachineElevationChangeMap));
        var result = new SubGridTreeSubGridExistenceBitMask();

        if (cacheItem != null)
        {
          result.FromBytes(cacheItem);
        }

        return result;
      }
      catch (KeyNotFoundException)
      {
        return null;
      }
    }

    public void Put(Guid siteModelUid, Guid assetUid, SubGridTreeSubGridExistenceBitMask changeMap)
    {
      if (changeMap == null)
      {
        throw new ArgumentException("Change map cannot be null");
      }

      _proxyStorageCache.Put(new SiteModelMachineAffinityKey(siteModelUid, assetUid, FileSystemStreamType.SiteModelMachineElevationChangeMap), changeMap.ToBytes());
    }
  }
}
