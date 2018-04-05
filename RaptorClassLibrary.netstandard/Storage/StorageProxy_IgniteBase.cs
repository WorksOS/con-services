using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using log4net;
using System.IO;
using System.Reflection;
using VSS.VisionLink.Raptor.GridFabric.Affinity;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Storage.Utilities;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Storage
{
    public class StorageProxy_IgniteBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected IIgnite ignite;

        protected ICache<string, byte[]> nonSpatialCache;
        public ICache<string, byte[]> NonSpatialCache => nonSpatialCache;

        protected ICache<SubGridSpatialAffinityKey, byte[]> spatialCache;
        public ICache<SubGridSpatialAffinityKey, byte[]> SpatialCache => spatialCache;

        /// <summary>
        /// Controls which grid (Mutable or Immutable) this storage proxy performs reads and writes against.
        /// </summary>
        public StorageMutability Mutability { get; set; } = StorageMutability.Immutable;

        public StorageProxy_IgniteBase(StorageMutability mutability)
        {
            Mutability = mutability;

            ignite = RaptorGridFactory.Grid(RaptorGrids.RaptorGridName(Mutability));
        }

        protected void EstablishCaches()
        {
           spatialCache = ignite.GetCache<SubGridSpatialAffinityKey, byte[]>(RaptorCaches.SpatialCacheName(Mutability));
           nonSpatialCache = ignite.GetCache<string, byte[]>(RaptorCaches.NonSpatialCacheName(Mutability));
        }

        /// <summary>
        /// Computes the cache key name for a given data model and a given named stream within that datamodel
        /// </summary>
        /// <param name="DataModelID"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        protected static string ComputeNamedStreamCacheKey(long DataModelID, string Name)
        {
            return string.Format("{0}-{1}", DataModelID, Name);
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
            return string.Format("{0}-{1}-{2}-{3}", DataModelID, Name, SubgridX, SubgridY);
        }

        /// <summary>
        /// Supports taking a mutable version of a piece of data and transforming it into the immutable form if not present in the immutable cache
        /// </summary>
        /// <param name="mutableCache"></param>
        /// <param name="immutableCache"></param>
        /// <param name="cacheKey"></param>
        /// <param name="streamType"></param>
        /// <returns></returns>
        protected MemoryStream PerformNonSpatialImmutabilityConversion(ICache<string, byte[]> mutableCache,
                                                                       ICache<string, byte[]> immutableCache,
                                                                       string cacheKey,
                                                                       FileSystemStreamType streamType)
        {
            if (mutableCache == null || immutableCache == null)
            {
                return null;
            }

            MemoryStream immutableStream;
            using (MemoryStream MS = new MemoryStream(mutableCache.Get(cacheKey)))
            {
                MemoryStream mutableStream = MemoryStreamCompression.Decompress(MS);
                {
                    immutableStream = PerformNonSpatialImmutabilityConversion(mutableStream, immutableCache, cacheKey, streamType);                

                    if (mutableStream != immutableStream)
                    {
                        mutableStream.Dispose();
                    }
                }
            }

            return immutableStream;
        }

        /// <summary>
        /// Supports taking a mutable version of a piece of data and transforming it into the immutable form if not present in the immutable cache
        /// </summary>
        /// <param name="mutableStream"></param>
        /// <param name="immutableCache"></param>
        /// <param name="cacheKey"></param>
        /// <param name="streamType"></param>
        /// <returns></returns>
        protected MemoryStream PerformNonSpatialImmutabilityConversion(MemoryStream mutableStream,
                                                                       ICache<string, byte[]> immutableCache,
                                                                       string cacheKey,
                                                                       FileSystemStreamType streamType)
        {
            if (mutableStream == null || immutableCache == null)
            {
                return null;
            }

            // Convert from the mutable to the immutable form and store it into the immutable cache
            if (MutabilityConverter.ConvertToImmutable(streamType, mutableStream, out MemoryStream immutableStream) && (immutableStream != null))
            {
                using (MemoryStream compressedStream = MemoryStreamCompression.Compress(immutableStream))
                {
                    Log.Info(string.Format($"Putting key:{cacheKey} in {immutableCache.Name}, size:{immutableStream.Length} -> {compressedStream.Length}"));
                    
                    // Place the converted immutable item into the immutable cache
                    immutableCache.Put(cacheKey, compressedStream.ToArray());
                }
            }
            else
            {
                // There was no immutable version of the requested information. Allow this to bubble up the stack...
                // TODO Log the failure

                immutableStream = null;
            }

            if (mutableStream != immutableStream)
            {
                mutableStream.Dispose();
            }

            return immutableStream;
        }

        /// <summary>
        /// Supports taking a mutable version of a piece of data and transforming it into the immutable form if not present in the immutable cache
        /// </summary>
        /// <param name="mutableCache"></param>
        /// <param name="immutableCache"></param>
        /// <param name="cacheKey"></param>
        /// <param name="streamType"></param>
        /// <returns></returns>
        protected MemoryStream PerformSpatialImmutabilityConversion(ICache<SubGridSpatialAffinityKey, byte[]> mutableCache,
                                                                    ICache<SubGridSpatialAffinityKey, byte[]> immutableCache,
                                                                    SubGridSpatialAffinityKey cacheKey,
                                                                    FileSystemStreamType streamType)
        {
            if (mutableCache == null || immutableCache == null)
            {
                return null;
            }

            MemoryStream immutableStream;
            using (MemoryStream MS = new MemoryStream(mutableCache.Get(cacheKey)), mutableStream = MemoryStreamCompression.Decompress(MS))
            {
                immutableStream = PerformSpatialImmutabilityConversion(mutableStream, immutableCache, cacheKey, streamType);
            }

            return immutableStream;
        }

        /// <summary>
        /// Supports taking a mutable version of a piece of data and transforming it into the immutable form if not present in the immutable cache
        /// </summary>
        /// <param name="mutableStream"></param>
        /// <param name="immutableCache"></param>
        /// <param name="cacheKey"></param>
        /// <param name="streamType"></param>
        /// <returns></returns>
        protected MemoryStream PerformSpatialImmutabilityConversion(MemoryStream mutableStream,
                                                                    ICache<SubGridSpatialAffinityKey, byte[]> immutableCache,
                                                                    SubGridSpatialAffinityKey cacheKey,
                                                                    FileSystemStreamType streamType)
        {
            if (mutableStream == null || immutableCache == null)
            {
                return null;
            }

            // Convert from the mutable to the immutable form and store it into the immutable cache
            if (MutabilityConverter.ConvertToImmutable(streamType, mutableStream, out MemoryStream immutableStream) && (immutableStream != null))
            {
                using (MemoryStream compressedStream = MemoryStreamCompression.Compress(immutableStream))
                {
                    Log.Info(string.Format($"Putting key:{cacheKey} in {immutableCache.Name}, size:{immutableStream.Length} -> {compressedStream.Length}"));

                    // Place the converted immutable item into the immutable cache
                    immutableCache.Put(cacheKey, compressedStream.ToArray());
                }
            }
            else
            {
                // There was no immutable version of the requested information. Allow this to bubble up the stack...
                // TODO Log the failure

                immutableStream = null;
            }

            return immutableStream;
        }
    }
}
