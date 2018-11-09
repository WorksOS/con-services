using System;

namespace VSS.TRex.Caching.Interfaces
{
  [Flags]
  public enum TRexSpatialMemoryCacheInvalidationSensitivity
  {
    /// <summary>
    /// Cache items are insensitive to indirect stimuli regards invalidation.
    /// Direct stimuli such as specific removal are still permitted
    /// </summary>
    None = 0,

    /// <summary>
    /// Spatial (subgrid) updates driven by ingest of new production data will cause invalidation and
    /// eviction of all relevant spatial dat held within the cache
    /// </summary>
    ProductionDataIngest = 1
  }
}
