using System.Collections.Generic;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Interfaces;
using System;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using Microsoft.Extensions.Logging;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ExistenceMaps.Servers
{
  /// <summary>
  /// A server representing access operations for existence maps derived from topological surfaces such as TTM designs
  /// and surveyed surfaces
  /// </summary>
  public class ExistenceMapServer : IExistenceMapServer
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<ExistenceMapServer>();

    /// <summary>
    /// Get a specific existence map given its key
    /// </summary>
    public ISubGridTreeBitMask GetExistenceMap(INonSpatialAffinityKey key)
    {
      try
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(key.ProjectUID);

        if (siteModel == null)
          return null;

        var readResult = siteModel.PrimaryStorageProxy.ReadStreamFromPersistentStore(siteModel.ID, key.KeyName, Types.FileSystemStreamType.DesignTopologyExistenceMap, out var ms);
        if (readResult != Types.FileSystemErrorStatus.OK)
        {
          _log.LogError($"Failed to read existence map in project {key.ProjectUID} for key {key.KeyName}");
          return null;
        }

        if (ms != null)
        {
          using (ms)
          {
            var map = new SubGridTreeSubGridExistenceBitMask();// SubGridTreeBitMask();
            map.FromStream(ms);
            return map;
          }
        }

        return null;
      }
      catch (KeyNotFoundException)
      {
        // If the key is not present, return a null/empty array
        return null;
      }
    }

    /// <summary>
    /// Set or update a given existence map given its key.
    /// </summary>
    public void SetExistenceMap(INonSpatialAffinityKey key, ISerialisedByteArrayWrapper map)
    {
      throw new NotImplementedException();
      // _designTopologyExistenceMapsCache.Put(key, map);
    }
  }
}
