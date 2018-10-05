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
        /// <param name="siteModelID"></param>
        /// <param name="typeDescriptor"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static INonSpatialAffinityKey CacheKey(Guid siteModelID, long typeDescriptor, Guid ID) => new NonSpatialAffinityKey(siteModelID, $"Descriptor:{typeDescriptor}-ID:{ID}");
    }
}
