using System;
using log4net;
using System.Reflection;
using VSS.TRex.Storage;
using VSS.VisionLink.Raptor.GridFabric.Affinity;
using VSS.VisionLink.Raptor.GridFabric.Caches;

namespace VSS.VisionLink.Raptor.Storage
{
    /// <summary>
    /// Implementation of the IStorageProxy interface that provides read through for items covered by the storage proxy
    /// but which buffers all writes (enlists them in a transaction) until commanded to flush the writes to Ignite in a
    /// single transacted PutAll().
    /// Note: All read and write operations are sending and receiving MemoryStream objects.
    /// </summary>
    public class StorageProxy_Ignite_Transactional : StorageProxy_Ignite
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructor that obtains references to the mutable and immutable, spatial and non-spatial caches present in the grid
        /// </summary>
        /// <param name="mutability"></param>
        public StorageProxy_Ignite_Transactional(StorageMutability mutability) : base(mutability)
        {
            EstablishCaches();
        }

        private void EstablishCaches()
        {
            spatialCache = new StorageProxyCacheTransacted<SubGridSpatialAffinityKey, byte[]>(
                ignite.GetCache<SubGridSpatialAffinityKey, byte[]>(RaptorCaches.SpatialCacheName(Mutability)));
            nonSpatialCache =
                new StorageProxyCacheTransacted<string, byte[]>(
                    ignite.GetCache<string, byte[]>(RaptorCaches.NonSpatialCacheName(Mutability)));
        }

        public override bool Commit()
        {
            try
            {
                spatialCache.Commit();
                return true;
            }
            catch ( Exception e)
            {
                Log.Error($"Exception {e} thrown committing changes to Ignite");
                throw;
            }
        }
    }
}
