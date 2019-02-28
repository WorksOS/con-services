using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.ExistenceMaps.Interfaces
{
  public interface IExistenceMapServer
  {
    /// <summary>
    /// Get a specific existence map given its key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    byte[] GetExistenceMap(INonSpatialAffinityKey key);

    /// <summary>
    /// Set or update a given existence map given its key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="map"></param>
    void SetExistenceMap(INonSpatialAffinityKey key, byte[] map);
  }
}
