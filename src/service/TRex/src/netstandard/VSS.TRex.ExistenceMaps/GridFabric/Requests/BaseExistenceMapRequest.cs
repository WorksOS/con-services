using System;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.ExistenceMaps.GridFabric.Requests
{
    /// <summary>
    /// Base class for existence maps requests. Defines existence map type descriptors and related base functionality such as cache key calculation
    /// </summary>
    public class BaseExistenceMapRequest
    {
        /// <summary>
        /// Construct a unique key for the existence map comprised of a type descriptor and an ID
        /// </summary>
        [Obsolete("Remove once design managament via requests is completed")]
        public static INonSpatialAffinityKey CacheKey(Guid siteModelID, long typeDescriptor, Guid ID) => new NonSpatialAffinityKey(siteModelID, $"ExistenceMapDescriptor-{typeDescriptor}-ID-{ID}");

        public static string CacheKeyString(long typeDescriptor, Guid ID) => $"ExistenceMapDescriptor-{typeDescriptor}-ID-{ID}";
  }
}
