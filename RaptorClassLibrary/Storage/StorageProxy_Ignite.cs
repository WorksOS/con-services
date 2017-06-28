using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Eviction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Storage
{
    /// <summary>
    /// Implementation of the IStorageProxy interface that allows to read/write operations against Ignite based IO support.
    /// Note: All read and write operations are sending and receiving MemoryStream objects.
    /// </summary>
    public class StorageProxy_Ignite : IStorageProxy
    {
        private static IIgnite ignite = null;

        private static ICache<String, MemoryStream> mutableSpatialCache = null;
        private static ICache<String, MemoryStream> immutableSpatialCache = null;
        private static ICache<String, MemoryStream> mutableNonSpatialCache = null;
        private static ICache<String, MemoryStream> immutableNonSpatialCache = null;

        private static Object LockObj = new Object();

        /// <summary>
        /// Constructor that obtains references to the mutable and immutable, spatial and non-spatial caches present in the grid
        /// </summary>
        /// <param name="gridName"></param>
        public StorageProxy_Ignite(string gridName)
        {
            if (ignite == null)
            {
                ignite = Ignition.TryGetIgnite(gridName);

                if (ignite != null)
                {
                    mutableSpatialCache = ignite.GetCache<String, MemoryStream>(RaptorCaches.MutableSpatialCacheName());
                    immutableSpatialCache = ignite.GetCache<String, MemoryStream>(RaptorCaches.ImmutableSpatialCacheName());

                    mutableNonSpatialCache = ignite.GetCache<String, MemoryStream>(RaptorCaches.MutableNonSpatialCacheName());
                    immutableNonSpatialCache = ignite.GetCache<String, MemoryStream>(RaptorCaches.ImmutableNonSpatialCacheName());
                }
            }
        }

        /// <summary>
        /// Supports taking a mutable version of a piece of data and transforming it into the immutable form if not present in the immutable cache
        /// </summary>
        /// <param name="mutableCache"></param>
        /// <param name="immutableCache"></param>
        /// <param name="cacheKey"></param>
        /// <param name="streamType"></param>
        /// <returns></returns>
        private MemoryStream PerformImmutabilityConversion(ICache<String, MemoryStream> mutableCache,
                                                           ICache<String, MemoryStream> immutableCache,
                                                           string cacheKey, 
                                                           FileSystemStreamType streamType)
        {
            MemoryStream mutableStream = mutableSpatialCache.Get(cacheKey);

            // If successfully read, convert from the mutable to the immutable form and store it into the immutable cache
            MemoryStream immutableStream = null;

            if (MutabilityConverter.ConvertToImmutable(streamType, mutableStream, out immutableStream) && (immutableStream != null))
            {
                // Place the converted immutable item into the immutable cache
                immutableNonSpatialCache.Put(cacheKey, immutableStream);
            }
            else
            {
                // There was no mutable version of the requested information. Allow this to bubble up the stack...
                // TODO Log the failure

                immutableStream = null;
            }

            return immutableStream;
        }

        /// <summary>
        /// Computes the cache key name for a given data model and a given named stream within that datamodel
        /// </summary>
        /// <param name="DataModelID"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        private static string ComputeNamedStreamCacheKey(long DataModelID, string Name)
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
        private static string ComputeNamedStreamCacheKey(long DataModelID, string Name, uint SubgridX, uint SubgridY)
        {
            return String.Format("{0}-{1}-{2}-{3}", DataModelID, Name, SubgridX, SubgridY);
        }

        /// <summary>
        /// Supports reading a stream of spatial data from the persistent store via the grid cache
        /// </summary>
        /// <param name="DataModelID"></param>
        /// <param name="StreamName"></param>
        /// <param name="SubgridX"></param>
        /// <param name="SubgridY"></param>
        /// <param name="StreamType"></param>
        /// <param name="GranuleIndex"></param>
        /// <param name="Stream"></param>
        /// <param name="StoreGranuleIndex"></param>
        /// <param name="StoreGranuleCount"></param>
        /// <returns></returns>
        public FileSystemErrorStatus ReadSpatialStreamFromPersistentStore(long DataModelID, 
                                                                          string StreamName, 
                                                                          uint SubgridX, uint SubgridY,
                                                                          FileSystemStreamType StreamType, 
                                                                          uint GranuleIndex, 
                                                                          out MemoryStream Stream, 
                                                                          out uint StoreGranuleIndex, 
                                                                          out uint StoreGranuleCount)
        {
            StoreGranuleIndex = 0;
            StoreGranuleCount = 0;

            try
            {
                string cacheKey = ComputeNamedStreamCacheKey(DataModelID, StreamName/*, SubgridX, SubgridY*/);

                try
                {
                    // First look to see if the immutable item is in the cache
                    Stream = immutableSpatialCache.Get(cacheKey);
                }
                catch (KeyNotFoundException e)
                {
                    Stream = PerformImmutabilityConversion(mutableNonSpatialCache, immutableNonSpatialCache, cacheKey, StreamType);
                }
                return FileSystemErrorStatus.OK;
            }
            catch (Exception E)
            {
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
            try
            {
                string cacheKey = ComputeNamedStreamCacheKey(DataModelID, StreamName);

                try
                {
                    Stream = immutableNonSpatialCache.Get(cacheKey);
                }
                catch (KeyNotFoundException e)
                {
                    Stream = PerformImmutabilityConversion(mutableNonSpatialCache, immutableNonSpatialCache, cacheKey, StreamType);
                }

                Stream.Position = 0;
                return FileSystemErrorStatus.OK;
            }
            catch (Exception E)
            {
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
        public FileSystemErrorStatus ReadStreamFromPersistentStore(long DataModelID, string StreamName, FileSystemStreamType StreamType, out MemoryStream Streamout, uint StoreGranuleIndex, out uint StoreGranuleCount)
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

                // Remove item from both immutable and mutable caches
                try
                {
                    mutableNonSpatialCache.Remove(cacheKey);
                }
                catch
                {
                    // TODO Log the error
                }

                try
                {
                    immutableNonSpatialCache.Remove(cacheKey);
                }
                catch
                {
                    // TODO Log the error
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
        /// <param name="StreamType"></param>
        /// <param name="StoreGranuleIndex"></param>
        /// <param name="StoreGranuleCount"></param>
        /// <param name="Stream"></param>
        /// <returns></returns>
        public FileSystemErrorStatus WriteSpatialStreamToPersistentStore(long DataModelID, string StreamName, uint SubgridX, uint SubgridY, FileSystemStreamType StreamType, out uint StoreGranuleIndex, out uint StoreGranuleCount, MemoryStream Stream)
        {
            StoreGranuleCount = 0;
            StoreGranuleIndex = 0;

            try
            {
                string cacheKey = ComputeNamedStreamCacheKey(DataModelID, StreamName/*, SubgridX, SubgridY*/);

                mutableSpatialCache.Put(cacheKey, Stream);

                // Invalidate the immutable version
                immutableSpatialCache.Remove(cacheKey);

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
        public FileSystemErrorStatus WriteStreamToPersistentStore(long DataModelID, string StreamName, FileSystemGranuleType StreamType, out uint StoreGranuleIndex, out uint StoreGranuleCount, MemoryStream Stream)
        {
            StoreGranuleCount = 0;
            StoreGranuleIndex = 0;

            try
            {
                string cacheKey = ComputeNamedStreamCacheKey(DataModelID, StreamName);

                mutableNonSpatialCache.Put(cacheKey, Stream);

                // Invalidate the immutable version
                immutableNonSpatialCache.Remove(cacheKey);

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
        public FileSystemErrorStatus WriteStreamToPersistentStoreDirect(long DataModelID, string StreamName, FileSystemGranuleType StreamType, MemoryStream Stream)
        {
            try
            {
                string cacheKey = ComputeNamedStreamCacheKey(DataModelID, StreamName);

                mutableNonSpatialCache.Put(cacheKey, Stream);

                // Invalidate the immutable version
                immutableNonSpatialCache.Remove(cacheKey);
                
                return FileSystemErrorStatus.OK;
            }
            catch
            {
                return FileSystemErrorStatus.UnknownErrorWritingToFS;
            }
        }
    }
}
