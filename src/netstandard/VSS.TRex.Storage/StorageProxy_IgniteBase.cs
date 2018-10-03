using System;
using Apache.Ignite.Core;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.Storage.Utilities;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Storage
{
    public abstract class StorageProxy_IgniteBase
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<StorageProxy_IgniteBase>();

        private static readonly IMutabilityConverter MutabilityConverter = DIContext.Obtain<IMutabilityConverter>();

        protected IIgnite ignite;

        protected IStorageProxyCache<INonSpatialAffinityKey, byte[]> nonSpatialCache;

        public IStorageProxyCache<INonSpatialAffinityKey, byte[]> NonSpatialCache
        {
          get { return nonSpatialCache; }
        }

        protected IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]> spatialCache;
        public IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]> SpatialCache
        {
          get { return spatialCache; }
        }

        /// <summary>
        /// Controls which grid (Mutable or Immutable) this storage proxy performs reads and writes against.
        /// </summary>
        public StorageMutability Mutability { get; set; }

        public StorageProxy_IgniteBase(StorageMutability mutability)
        {
            Mutability = mutability;

            ignite = DIContext.Obtain<ITRexGridFactory>().Grid(mutability);
        }

        /// <summary>
        /// Computes the cache key name for a given data model and a given named stream within that datamodel
        /// </summary>
        /// <param name="DataModelID"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        protected static INonSpatialAffinityKey ComputeNamedStreamCacheKey(Guid DataModelID, string Name) => new NonSpatialAffinityKey(DataModelID, Name);

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
            return $"{DataModelID}-{Name}-{SubgridX}-{SubgridY}";
        }

        /// <summary>
        /// Supports taking a mutable version of a piece of data and transforming it into the immutable form if not present in the immutable cache
        /// </summary>
        /// <param name="mutableCache"></param>
        /// <param name="immutableCache"></param>
        /// <param name="cacheKey"></param>
        /// <param name="streamType"></param>
        /// <returns></returns>
        protected MemoryStream PerformNonSpatialImmutabilityConversion(IStorageProxyCache<INonSpatialAffinityKey, byte[]> mutableCache,
                                                                       IStorageProxyCache<INonSpatialAffinityKey, byte[]> immutableCache,
                                                                       INonSpatialAffinityKey cacheKey,
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
                                                                       IStorageProxyCache<INonSpatialAffinityKey, byte[]> immutableCache,
                                                                       INonSpatialAffinityKey cacheKey,
                                                                       FileSystemStreamType streamType)
        {
            if (mutableStream == null || immutableCache == null)
            {
                return null;
            }

            // Convert from the mutable to the immutable form and store it into the immutable cache
            if (MutabilityConverter.ConvertToImmutable(streamType, mutableStream, out MemoryStream immutableStream) && immutableStream != null)
            {
                using (MemoryStream compressedStream = MemoryStreamCompression.Compress(immutableStream))
                {
                    // Log.LogInformation($"Putting key:{cacheKey} in {immutableCache.Name}, size:{immutableStream.Length} -> {compressedStream.Length}");
                    
                    // Place the converted immutable item into the immutable cache
                    immutableCache.Put(cacheKey, compressedStream.ToArray());
                }
            }
            else
            {
              // There was no immutable version of the requested information. Allow this to bubble up the stack...
              Log.LogError($"MutabilityConverter.ConvertToImmutable failed to convert mutable data for streamType={streamType}");

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
        protected MemoryStream PerformSpatialImmutabilityConversion(IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]> mutableCache,
                                                                    IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]> immutableCache,
                                                                    ISubGridSpatialAffinityKey cacheKey,
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
                                                                    IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]> immutableCache,
                                                                    ISubGridSpatialAffinityKey cacheKey,
                                                                    FileSystemStreamType streamType)
        {
            if (mutableStream == null || immutableCache == null)
            {
                return null;
            }

            // Convert from the mutable to the immutable form and store it into the immutable cache
            if (MutabilityConverter.ConvertToImmutable(streamType, mutableStream, out MemoryStream immutableStream) && immutableStream != null)
            {
                using (MemoryStream compressedStream = MemoryStreamCompression.Compress(immutableStream))
                {
                    // Log.LogInformation($"Putting key:{cacheKey} in {immutableCache.Name}, size:{immutableStream.Length} -> {compressedStream.Length}");

                    // Place the converted immutable item into the immutable cache
                    immutableCache.Put(cacheKey, compressedStream.ToArray());
                }
            }
            else
            {
               // There was no immutable version of the requested information. Allow this to bubble up the stack...
               Log.LogError($"MutabilityConverter.ConvertToImmutable failed to convert mutable data for streamType={streamType}");

               immutableStream = null;
            }

            return immutableStream;
        }
    }
}
