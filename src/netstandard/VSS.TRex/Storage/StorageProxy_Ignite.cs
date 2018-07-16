using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Caches;
using VSS.TRex.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.Storage
{
    /// <summary>
    /// Implementation of the IStorageProxy interface that allows to read/write operations against Ignite based IO support.
    /// Note: All read and write operations are sending and receiving MemoryStream objects.
    /// </summary>
    public class StorageProxy_Ignite : StorageProxy_IgniteBase, IStorageProxy
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// The reference to a storage proxy representing the immutable data store derived from a mutable data store
        /// </summary>w
        public IStorageProxy ImmutableProxy;

        /// <summary>
        /// Constructor that obtains references to the mutable and immutable, spatial and non-spatial caches present in the grid
        /// </summary>
        /// <param name="mutability"></param>
        public StorageProxy_Ignite(StorageMutability mutability) : base(mutability)
        {
            EstablishCaches();
        }

        private void EstablishCaches()
        {
            spatialCache = new StorageProxyCache<SubGridSpatialAffinityKey, byte[]>(
                ignite.GetCache<SubGridSpatialAffinityKey, byte[]>(TRexCaches.SpatialCacheName(Mutability)));
            nonSpatialCache =
                new StorageProxyCache<NonSpatialAffinityKey, byte[]>(
                    ignite.GetCache<NonSpatialAffinityKey, byte[]>(TRexCaches.NonSpatialCacheName(Mutability)));
        }

        /// <summary>
    /// Supports reading a stream of spatial data from the persistent store via the grid cache
    /// </summary>
    /// <param name="DataModelID"></param>
    /// <param name="StreamName"></param>
    /// <param name="SubgridX"></param>
    /// <param name="SubgridY"></param>
    /// <param name="SegmentIdentifier"></param>
    /// <param name="StreamType"></param>
    /// <param name="Stream"></param>
    /// <returns></returns>
    public FileSystemErrorStatus ReadSpatialStreamFromPersistentStore(Guid DataModelID,
                                                                          string StreamName,
                                                                          uint SubgridX, uint SubgridY,
                                                                          string SegmentIdentifier,
                                                                          FileSystemStreamType StreamType,
                                                                          out MemoryStream Stream)
        {
            Stream = null;

            try
            {
                SubGridSpatialAffinityKey cacheKey = new SubGridSpatialAffinityKey(DataModelID, SubgridX, SubgridY, SegmentIdentifier);

                // Log.LogInformation(String.Format("Getting key:{0}", StreamName));

                try
                {
                    using (MemoryStream MS = new MemoryStream(spatialCache.Get(cacheKey)))
                    {
                        Stream = MemoryStreamCompression.Decompress(MS);
                        Stream.Position = 0;
                    }
                }
                catch (KeyNotFoundException)
                {
                    return FileSystemErrorStatus.GranuleDoesNotExist;
                }
                catch (Exception)
                {
                    throw;
                }

                return FileSystemErrorStatus.OK;
            }
            catch (Exception e)
            {
                Log.LogInformation(string.Format("Exception occurred: {0}", e));

                Stream = null;
                return FileSystemErrorStatus.UnknownErrorReadingFromFS;
            }
        }

        /// <summary>
        /// Supports reading a named stream from the persistent store via the grid cache
        /// </summary>
        /// <param name="DataModelID"></param>
        /// <param name="StreamName"></param>
        /// <param name="StreamType"></param>
        /// <param name="Stream"></param>
        /// <returns></returns>
        public FileSystemErrorStatus ReadStreamFromPersistentStore(Guid DataModelID, string StreamName, FileSystemStreamType StreamType, out MemoryStream Stream)
        {
            Stream = null;

            try
            {
                NonSpatialAffinityKey cacheKey = ComputeNamedStreamCacheKey(DataModelID, StreamName);

                // Log.LogInformation(String.Format("Getting key:{0}", cacheKey));

                try
                {
                    using (MemoryStream MS = new MemoryStream(nonSpatialCache.Get(cacheKey)))
                    {
                        Stream = MemoryStreamCompression.Decompress(MS);
                        Stream.Position = 0;
                    }
                }
                catch (KeyNotFoundException)
                {
                    return FileSystemErrorStatus.GranuleDoesNotExist;
                }

                return FileSystemErrorStatus.OK;
            }
            catch (Exception e)
            {
                Log.LogInformation(string.Format("Exception occurred: {0}", e));

                Stream = null;
                return FileSystemErrorStatus.UnknownErrorReadingFromFS;
            }
        }

        /// <summary>
        /// Supports reading a named stream from the persistent store via the grid cache
        /// </summary>
        /// <param name="DataModelID"></param>
        /// <param name="StreamName"></param>
        /// <param name="StreamType"></param>
        /// <param name="Stream"></param>
        /// <returns></returns>
        public FileSystemErrorStatus ReadStreamFromPersistentStoreDirect(Guid DataModelID, string StreamName, FileSystemStreamType StreamType, out MemoryStream Stream)
        {
            return ReadStreamFromPersistentStore(DataModelID, StreamName, StreamType, out Stream);
        }

        /// <summary>
        /// Supports removing a named stream from the persistent store via the grid cache
        /// </summary>
        /// <param name="DataModelID"></param>
        /// <param name="StreamName"></param>
        /// <returns></returns>
        public FileSystemErrorStatus RemoveStreamFromPersistentStore(Guid DataModelID, string StreamName)
        {
            try
            {
                NonSpatialAffinityKey cacheKey = ComputeNamedStreamCacheKey(DataModelID, StreamName);

                Log.LogInformation($"Removing key:{cacheKey}");

                // Remove item from both immutable and mutable caches
                try
                {
                    nonSpatialCache.Remove(cacheKey);
                }
                catch
                {
                    // TODO Log the error
                }

                ImmutableProxy?.RemoveStreamFromPersistentStore(DataModelID, StreamName);

                return FileSystemErrorStatus.OK;
            }
            catch
            {
                return FileSystemErrorStatus.UnknownErrorWritingToFS;
            }
        }

        /// <summary>
        /// Supports writing a spatial data stream to the persistent store via the grid cache.
        /// </summary>
        /// <param name="DataModelID"></param>
        /// <param name="StreamName"></param>
        /// <param name="SubgridX"></param>
        /// <param name="SubgridY"></param>
        /// <param name="SegmentIdentifier"></param>
        /// <param name="StreamType"></param>
        /// <param name="Stream"></param>
        /// <returns></returns>
        public FileSystemErrorStatus WriteSpatialStreamToPersistentStore(Guid DataModelID, string StreamName, 
                                                                         uint SubgridX, uint SubgridY,
                                                                         string SegmentIdentifier,
                                                                         FileSystemStreamType StreamType, 
                                                                         MemoryStream Stream)
        {
            try
            {
                SubGridSpatialAffinityKey cacheKey = new SubGridSpatialAffinityKey(DataModelID, SubgridX, SubgridY, SegmentIdentifier);

                using (MemoryStream compressedStream = MemoryStreamCompression.Compress(Stream))
                {
                    // Log.LogInformation($"Putting key:{cacheKey} in {spatialCache.Name}, size:{Stream.Length} -> {compressedStream.Length}");

                    spatialCache.Put(cacheKey, compressedStream.ToArray());
                }

                // Convert the stream to the immutable form and write it to the immutable storage proxy
                try
                {
                    if (Mutability == StorageMutability.Mutable && ImmutableProxy != null)
                    {
                        PerformSpatialImmutabilityConversion(Stream, (ImmutableProxy as StorageProxy_Ignite).SpatialCache, cacheKey, StreamType);
                    }
                }
                catch // (Exception e)
                {
                    // Ignore any exception here which is typically thrown if the element in the cache does not exist, which is entirely possible
                }

                return FileSystemErrorStatus.OK;
            }
            catch
            {
                return FileSystemErrorStatus.UnknownErrorWritingToFS;
            }
        }

        /// <summary>
        /// Supports writing a named data stream to the persistent store via the grid cache.
        /// </summary>
        /// <param name="DataModelID"></param>
        /// <param name="StreamName"></param>
        /// <param name="StreamType"></param>
        /// <param name="Stream"></param>
        /// <returns></returns>
        public FileSystemErrorStatus WriteStreamToPersistentStore(Guid DataModelID, string StreamName, FileSystemStreamType StreamType, MemoryStream Stream)
        {
            try
            {
                NonSpatialAffinityKey cacheKey = ComputeNamedStreamCacheKey(DataModelID, StreamName);

                using (MemoryStream compressedStream = MemoryStreamCompression.Compress(Stream))
                {
                    // Log.LogInformation($"Putting key:{cacheKey} in {nonSpatialCache.Name}, size:{Stream.Length} -> {compressedStream.Length}");

                    nonSpatialCache.Put(cacheKey, compressedStream.ToArray());
                }

                // Convert the stream to the immutable form and write it to the immutable storage proxy
                if (Mutability == StorageMutability.Mutable && ImmutableProxy != null)
                {
                    PerformNonSpatialImmutabilityConversion(Stream, (ImmutableProxy as StorageProxy_Ignite).NonSpatialCache, cacheKey, StreamType);
                }

                return FileSystemErrorStatus.OK;
            }
            catch
            {
                return FileSystemErrorStatus.UnknownErrorWritingToFS;
            }
        }

        /// <summary>
        /// Supports writing a named data stream to the persistent store via the grid cache.
        /// </summary>
        /// <param name="DataModelID"></param>
        /// <param name="StreamName"></param>
        /// <param name="StreamType"></param>
        /// <param name="Stream"></param>
        /// <returns></returns>
        public FileSystemErrorStatus WriteStreamToPersistentStoreDirect(Guid DataModelID, string StreamName, FileSystemStreamType StreamType, MemoryStream Stream)
        {
            try
            {
                NonSpatialAffinityKey cacheKey = ComputeNamedStreamCacheKey(DataModelID, StreamName);

                using (MemoryStream compressedStream = MemoryStreamCompression.Compress(Stream))
                {
                    // Log.LogInformation($"Putting key:{cacheKey} in {nonSpatialCache.Name}, size:{Stream.Length} -> {compressedStream.Length}");

                    nonSpatialCache.Put(cacheKey, compressedStream.ToArray());
                }

                // Convert the stream to the immutable form and write it to the immutable storage proxy
                if (Mutability == StorageMutability.Mutable && ImmutableProxy != null)
                {
                    PerformNonSpatialImmutabilityConversion(Stream, (ImmutableProxy as StorageProxy_Ignite).NonSpatialCache, cacheKey, StreamType);
                }

                return FileSystemErrorStatus.OK;
            }
            catch
            {
                return FileSystemErrorStatus.UnknownErrorWritingToFS;
            }
        }

        /// <summary>
        /// Sets a reference to a storage proxy that proxies the immutable data store for this mutable data store
        /// </summary>
        /// <param name="immutableProxy"></param>
        public void SetImmutableStorageProxy(IStorageProxy immutableProxy)
        {
            if (Mutability != StorageMutability.Mutable)
            {
                throw new ArgumentException("Non-mutable storage proxy may not accept an immutable storage proxy reference");
            }

            if (immutableProxy == null)
            {
                throw new ArgumentException("Null immutable storage proxy reference supplied to SetImmutableStorageProxy()");
            }

            if (immutableProxy.Mutability != StorageMutability.Immutable)
            {
                throw new ArgumentException("Immutable storage proxy reference is not marked with Immutable mutability");
            }

            ImmutableProxy = immutableProxy;
        }

        /// <summary>
        /// Commits unsaved changes in the storage proxy.
        /// No implementation for non-transactional storage proxy
        /// </summary>
        public virtual bool Commit()
        {
            return true;
        }

        /// <summary>
        /// Clears changes in the storage proxy.
        /// No implementation for non-transactional storage proxy
        /// </summary>
        public virtual void Clear()
        {            
        }
    }
}
