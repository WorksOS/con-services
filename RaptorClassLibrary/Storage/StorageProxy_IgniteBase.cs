using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Affinity;
using VSS.VisionLink.Raptor.GridFabric.Caches;

namespace VSS.VisionLink.Raptor.Storage
{
    public class StorageProxy_IgniteBase
    {
        protected static IIgnite ignite = null;

        protected static ICache<SubGridSpatialAffinityKey, MemoryStream> mutableSpatialCache = null;
        protected static ICache<SubGridSpatialAffinityKey, MemoryStream> immutableSpatialCache = null;
        protected static ICache<String, MemoryStream> mutableNonSpatialCache = null;
        protected static ICache<String, MemoryStream> immutableNonSpatialCache = null;

        protected static Object LockObj = new Object();

        public StorageProxy_IgniteBase(string gridName)
        {
            if (ignite == null)
            {
                ignite = Ignition.TryGetIgnite(gridName);
            }
        }

        protected void EstablishMutableCaches()
        {
            if (ignite != null)
            {
                mutableSpatialCache = ignite.GetCache<SubGridSpatialAffinityKey, MemoryStream>(RaptorCaches.MutableSpatialCacheName());
                mutableNonSpatialCache = ignite.GetCache<String, MemoryStream>(RaptorCaches.MutableNonSpatialCacheName());
            }
        }

        protected void EstablishImmutableCaches()
        {
            if (ignite != null)
            {
                immutableSpatialCache = ignite.GetCache<SubGridSpatialAffinityKey, MemoryStream>(RaptorCaches.ImmutableSpatialCacheName());
                immutableNonSpatialCache = ignite.GetCache<String, MemoryStream>(RaptorCaches.ImmutableNonSpatialCacheName());
            }
        }
        /// <summary>
        /// Computes the cache key name for a given data model and a given named stream within that datamodel
        /// </summary>
        /// <param name="DataModelID"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        protected static string ComputeNamedStreamCacheKey(long DataModelID, string Name)
        {
            return String.Format("{0}-{1}", DataModelID, Name);
        }

        /// <summary>
        /// Computes the cache key name for the given data model and a given spatial data stream within that datamodel
        /// </summary>
        /// <param name="DataModelID"></param>
        /// <param name="Name"></param>
        /// <param name="SubgridX"></param>
        /// <param name="SubgridY"></param>
        /// <returns></returns>
        protected static string ComputeNamedStreamCacheKey(long DataModelID, string Name, uint SubgridX, uint SubgridY)
        {
            return String.Format("{0}-{1}-{2}-{3}", DataModelID, Name, SubgridX, SubgridY);
        }
    }
}
