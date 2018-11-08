using System;

namespace VSS.TRex.GridFabric.Interfaces
{
  public interface INonSpatialAffinityKey : IProjectAffinity
  {
    /// <summary>
    /// Name of the object in the cache, encoded as a string
    /// </summary>
    string KeyName { get; set; }

    /// <summary>
    /// Converts the affinity key into a string representation suitable for use as a unique string
    /// identifying this data element in the cache.
    /// </summary>
    /// <returns></returns>
    string ToString();
  }
}
