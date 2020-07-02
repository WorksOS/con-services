using System.Collections.Generic;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Interfaces;
using System;

namespace VSS.TRex.ExistenceMaps.Servers
{
  /// <summary>
  /// A server representing access operations for existence maps derived from topological surfaces such as TTM designs
  /// and surveyed surfaces
  /// </summary>
  public class ExistenceMapServer : IExistenceMapServer
  {
    /// <summary>
    /// A cache that holds the existence maps derived from design files (eg: TTM files)
    /// Each existence map is stored in it's serialized byte stream from. It does not define the grid per se, but does
    /// define a cache that is used within the grid to stored existence maps
    /// </summary>
    //   private readonly ICache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper> _designTopologyExistenceMapsCache;

    /// <summary>
    /// Default no-arg constructor that creates the Ignite cache within the server
    /// </summary>
    public ExistenceMapServer()
    {
      //var ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Immutable);
    }

    /// <summary>
    /// Get a specific existence map given its key
    /// </summary>
    public ISerialisedByteArrayWrapper GetExistenceMap(INonSpatialAffinityKey key)
    {
      try
      {
        throw new NotImplementedException();
        //        return _designTopologyExistenceMapsCache.Get(key);
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
