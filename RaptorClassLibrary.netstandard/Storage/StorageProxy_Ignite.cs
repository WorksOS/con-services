using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using VSS.VisionLink.Raptor.GridFabric.Affinity;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.Storage.Utilities;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Storage
{
    /// <summary>
    /// Implementation of the IStorageProxy interface that allows to read/write operations against Ignite based IO support.
    /// Note: All read and write operations are sending and receiving MemoryStream objects.
    /// </summary>
    public class StorageProxy_Ignite : StorageProxy_IgniteBase, IStorageProxy
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The reference to a storage proxy representing the immutable data store derived from a mutable data store
        /// </summary>
        public IStorageProxy ImmutableProxy;

        /// <summary>
        /// Constructor that obtains references to the mutable and immutable, spatial and non-spatial caches present in the grid
        /// </summary>
        /// <param name="mutability"></param>
        public StorageProxy_Ignite(StorageMutability mutability) : base(mutability)
        {
            EstablishCaches();
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
        /// <param name="GranuleIndex"></param>
        /// <param name="Stream"></param>
        /// <param name="StoreGranuleIndex"></param>
        /// <param name="StoreGranuleCount"></param>
        /// <returns></returns>
        public FileSystemErrorStatus ReadSpatialStreamFromPersistentStore(long DataModelID,
                                                                          string StreamName,
                                                                          uint SubgridX, uint SubgridY,
                                                                          string SegmentIdentifier,
                                                                          FileSystemStreamType StreamType,
                                                                          uint GranuleIndex,
                                                                          out MemoryStream Stream,
                                                                          out uint StoreGranuleIndex,
                                                                          out uint StoreGranuleCount)
        {
            Stream = null;

            StoreGranuleIndex = 0;
            StoreGranuleCount = 0;

            try
            {
                SubGridSpatialAffinityKey cacheKey = new SubGridSpatialAffinityKey(DataModelID, SubgridX, SubgridY, SegmentIdentifier);

                // Log.Info(String.Format("Getting key:{0}", StreamName));

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
                Log.Info(string.Format("Exception occurred: {0}", e));

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
        public FileSystemErrorStatus ReadStreamFromPersistentStore(long DataModelID, string StreamName, FileSystemStreamType StreamType, out MemoryStream Stream)
        {
            Stream = null;

            try
            {
                string cacheKey = ComputeNamedStreamCacheKey(DataModelID, StreamName);

                // Log.Info(String.Format("Getting key:{0}", cacheKey));

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
                catch (Exception e)
                {
                    throw;
                }

                return FileSystemErrorStatus.OK;
            }
            catch (Exception e)
            {
                Log.Info(string.Format("Exception occurred: {0}", e));

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
        /// <param name="Streamout"></param>
        /// <param name="StoreGranuleIndex"></param>
        /// <param name="StoreGranuleCount"></param>
        /// <returns></returns>
        public FileSystemErrorStatus ReadStreamFromPersistentStore(long DataModelID, string StreamName, FileSystemStreamType StreamType, out MemoryStream Streamout, out uint StoreGranuleIndex, out uint StoreGranuleCount)
        {
            StoreGranuleCount = 0;
            StoreGranuleIndex = 0;

            return ReadStreamFromPersistentStore(DataModelID, StreamName, StreamType, out Streamout);
        }

        /// <summary>
        /// Supports reading a named stream from the persistent store via the grid cache
        /// </summary>
        /// <param name="DataModelID"></param>
        /// <param name="StreamName"></param>
        /// <param name="StreamType"></param>
        /// <param name="Stream"></param>
        /// <returns></returns>
        public FileSystemErrorStatus ReadStreamFromPersistentStoreDirect(long DataModelID, string StreamName, FileSystemStreamType StreamType, out MemoryStream Stream)
        {
            return ReadStreamFromPersistentStore(DataModelID, StreamName, StreamType, out Stream);
        }

        /// <summary>
        /// Supports removing a named stream from the persistent store via the grid cache
        /// </summary>
        /// <param name="DataModelID"></param>
        /// <param name="StreamName"></param>
        /// <returns></returns>
        public FileSystemErrorStatus RemoveStreamFromPersistentStore(long DataModelID, string StreamName)
        {
            try
            {
                string cacheKey = ComputeNamedStreamCacheKey(DataModelID, StreamName);

                Log.Info(string.Format("Removing key:{0}", cacheKey));

                // Remove item from both immutable and mutable caches
                try
                {
                    nonSpatialCache.GetAndRemove(cacheKey);
                }
                catch
                {
                    // TODO Log the error
                }

                if (ImmutableProxy != null)
                {
                    ImmutableProxy.RemoveStreamFromPersistentStore(DataModelID, StreamName);
                }

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
        /// <param name="StoreGranuleIndex"></param>
        /// <param name="StoreGranuleCount"></param>
        /// <param name="Stream"></param>
        /// <returns></returns>
        public FileSystemErrorStatus WriteSpatialStreamToPersistentStore(long DataModelID, string StreamName, 
                                                                         uint SubgridX, uint SubgridY,
                                                                         string SegmentIdentifier,
                                                                         FileSystemStreamType StreamType, 
                                                                         out uint StoreGranuleIndex, out uint StoreGranuleCount, 
                                                                         MemoryStream Stream)
        {
            StoreGranuleCount = 0;
            StoreGranuleIndex = 0;

            try
            {
                SubGridSpatialAffinityKey cacheKey = new SubGridSpatialAffinityKey(DataModelID, SubgridX, SubgridY, SegmentIdentifier);

                using (MemoryStream compressedStream = MemoryStreamCompression.Compress(Stream))
                {
                    Log.Info($"Putting key:{cacheKey} in {spatialCache.Name}, size:{Stream.Length} -> {compressedStream.Length}");

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
        /// <param name="StoreGranuleIndex"></param>
        /// <param name="StoreGranuleCount"></param>
        /// <param name="Stream"></param>
        /// <returns></returns>
        public FileSystemErrorStatus WriteStreamToPersistentStore(long DataModelID, string StreamName, FileSystemStreamType StreamType, out uint StoreGranuleIndex, out uint StoreGranuleCount, MemoryStream Stream)
        {
            StoreGranuleCount = 0;
            StoreGranuleIndex = 0;

            try
            {
                string cacheKey = ComputeNamedStreamCacheKey(DataModelID, StreamName);

                using (MemoryStream compressedStream = MemoryStreamCompression.Compress(Stream))
                {
                    Log.Info($"Putting key:{cacheKey} in {nonSpatialCache.Name}, size:{Stream.Length} -> {compressedStream.Length}");

                    // IIgnite ignite = Ignition.GetIgnite(RaptorGrids.RaptorGridName());
                    // ICache<string, byte[]> cache = ignite.GetCache<string, byte[]>(RaptorCaches.MutableNonSpatialCacheName());
                    // cache.Put(cacheKey, compressedStream.ToArray());

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
        public FileSystemErrorStatus WriteStreamToPersistentStoreDirect(long DataModelID, string StreamName, FileSystemStreamType StreamType, MemoryStream Stream)
        {
            try
            {
                string cacheKey = ComputeNamedStreamCacheKey(DataModelID, StreamName);

                using (MemoryStream compressedStream = MemoryStreamCompression.Compress(Stream))
                {
                    Log.Info($"Putting key:{cacheKey} in {nonSpatialCache.Name}, size:{Stream.Length} -> {compressedStream.Length}");

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
    }
}
