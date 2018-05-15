using System;
using log4net;
using System.Reflection;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Caches;
using VSS.TRex.Storage;

namespace VSS.TRex.Storage
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

        /// <summary>
        /// Creates transactional storage proxies to be used by the consuming client
        /// </summary>
        private void EstablishCaches()
        {
            spatialCache = new StorageProxyCacheTransacted<SubGridSpatialAffinityKey, byte[]>(
                ignite.GetCache<SubGridSpatialAffinityKey, byte[]>(RaptorCaches.SpatialCacheName(Mutability)));
            nonSpatialCache =
                new StorageProxyCacheTransacted<NonSpatialAffinityKey, byte[]>(
                    ignite.GetCache<NonSpatialAffinityKey, byte[]>(RaptorCaches.NonSpatialCacheName(Mutability)));
        }

        /// <summary>
        /// Commits all unsaved changes in the spatial and non-spatial stores
        /// </summary>
        /// <returns></returns>
        public override bool Commit()
        {
            try
            {
                spatialCache.Commit();
            }
            catch ( Exception e)
            {
                Log.Error($"Exception {e} thrown committing changes to Ignite for spatial cache");
                throw;
            }

            try
            {
                nonSpatialCache.Commit();
            }
            catch (Exception e)
            {
                Log.Error($"Exception {e} thrown committing changes to Ignite for non spatial cache");
                throw;
            }

            return ImmutableProxy?.Commit() ?? true;
        }

        /// <summary>
        /// Clears all changes in the spatial and non spatial stores
        /// </summary>
        public override void Clear()
        {
            try
            {
                spatialCache.Clear();
            }
            catch (Exception e)
            {
                Log.Error($"Exception {e} thrown clearing changes for spatial cache");
                throw;
            }

            try
            {
                nonSpatialCache.Clear();
            }
            catch (Exception e)
            {
                Log.Error($"Exception {e} thrown clearing changes for non spatial cache");
                throw;
            }

            ImmutableProxy?.Clear();
        }
    }
}
