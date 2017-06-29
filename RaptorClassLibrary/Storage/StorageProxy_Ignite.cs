﻿using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Eviction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Affinity;
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
    public class StorageProxy_Ignite : StorageProxy_IgniteBase, IStorageProxy
    {
        /// <summary>
        /// Constructor that obtains references to the mutable and immutable, spatial and non-spatial caches present in the grid
        /// </summary>
        /// <param name="gridName"></param>
        public StorageProxy_Ignite(string gridName) : base(gridName)
        {
            EstablishMutableCaches();
            EstablishImmutableCaches();
        }

        /// <summary>
        /// Supports taking a mutable version of a piece of data and transforming it into the immutable form if not present in the immutable cache
        /// </summary>
        /// <param name="mutableCache"></param>
        /// <param name="immutableCache"></param>
        /// <param name="cacheKey"></param>
        /// <param name="streamType"></param>
        /// <returns></returns>
        private MemoryStream PerformNonSpatialImmutabilityConversion(ICache<String, MemoryStream> mutableCache,
                                                                     ICache<String, MemoryStream> immutableCache,
                                                                     string cacheKey, 
                                                                     FileSystemStreamType streamType)
        {
            MemoryStream immutableStream = null;
            MemoryStream mutableStream = mutableCache.Get(cacheKey);

            // If successfully read, convert from the mutable to the immutable form and store it into the immutable cache
            if (mutableStream != null)
            {
                if (MutabilityConverter.ConvertToImmutable(streamType, mutableStream, out immutableStream) && (immutableStream != null))
                {
                    // Place the converted immutable item into the immutable cache
                    immutableCache.Put(cacheKey, immutableStream);
                }
                else
                {
                    // There was no immutable version of the requested information. Allow this to bubble up the stack...
                    // TODO Log the failure

                    immutableStream = null;
                }
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
        private MemoryStream PerformSpatialImmutabilityConversion(ICache<SubGridSpatialAffinityKey, MemoryStream> mutableCache,
                                                                  ICache<SubGridSpatialAffinityKey, MemoryStream> immutableCache,
                                                                  SubGridSpatialAffinityKey cacheKey,
                                                                  FileSystemStreamType streamType)
        {
            MemoryStream immutableStream = null;
            MemoryStream mutableStream = mutableCache.Get(cacheKey);

            // If successfully read, convert from the mutable to the immutable form and store it into the immutable cache
            if (mutableStream != null)
            {
                if (MutabilityConverter.ConvertToImmutable(streamType, mutableStream, out immutableStream) && (immutableStream != null))
                {
                    // Place the converted immutable item into the immutable cache
                    immutableCache.Put(cacheKey, immutableStream);
                }
                else
                {
                    // There was no immutable version of the requested information. Allow this to bubble up the stack...
                    // TODO Log the failure

                    immutableStream = null;
                }
            }

            return immutableStream;
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
                                                                          string SegmentIdentifier,
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
                SubGridSpatialAffinityKey cacheKey = new SubGridSpatialAffinityKey(DataModelID, SubgridX, SubgridY, SegmentIdentifier);

                try
                {
                    // First look to see if the immutable item is in the cache
                    Stream = immutableSpatialCache.Get(new SubGridSpatialAffinityKey(DataModelID, SubgridX, SubgridY));
                }
                catch (KeyNotFoundException e)
                {
                    Stream = PerformSpatialImmutabilityConversion(mutableSpatialCache, immutableSpatialCache, cacheKey, StreamType);
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
                    Stream = PerformNonSpatialImmutabilityConversion(mutableNonSpatialCache, immutableNonSpatialCache, cacheKey, StreamType);
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
                    mutableNonSpatialCache.GetAndRemove(cacheKey);
                }
                catch
                {
                    // TODO Log the error
                }

                try
                {
                    immutableNonSpatialCache.GetAndRemove(cacheKey);
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

                mutableSpatialCache.Put(cacheKey, Stream);

                try
                {
                    // Invalidate the immutable version
                    immutableSpatialCache.GetAndRemove(cacheKey);
                }
                catch (Exception e)
                {
                    // Ignore any excpetion here which is typically thrown if the element in the
                    // cache does not exist, which is entirely possible
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
        public FileSystemErrorStatus WriteStreamToPersistentStore(long DataModelID, string StreamName, FileSystemGranuleType StreamType, out uint StoreGranuleIndex, out uint StoreGranuleCount, MemoryStream Stream)
        {
            StoreGranuleCount = 0;
            StoreGranuleIndex = 0;

            try
            {
                string cacheKey = ComputeNamedStreamCacheKey(DataModelID, StreamName);

                mutableNonSpatialCache.Put(cacheKey, Stream);

                try
                {
                    // Invalidate the immutable version
                    immutableNonSpatialCache.GetAndRemove(cacheKey);
                }
                catch (Exception e)
                {
                    // Ignore any excpetion here which is typically thrown if the element in the
                    // cache does not exist, which is entirely possible
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
        public FileSystemErrorStatus WriteStreamToPersistentStoreDirect(long DataModelID, string StreamName, FileSystemGranuleType StreamType, MemoryStream Stream)
        {
            try
            {
                string cacheKey = ComputeNamedStreamCacheKey(DataModelID, StreamName);

                mutableNonSpatialCache.Put(cacheKey, Stream);

                try
                {
                    // Invalidate the immutable version
                    immutableNonSpatialCache.GetAndRemove(cacheKey);
                }
                catch (Exception e)
                {
                    // Ignore any excpetion here which is typically thrown if the element in the
                    // cache does not exist, which is entirely possible
                }

                return FileSystemErrorStatus.OK;
            }
            catch
            {
                return FileSystemErrorStatus.UnknownErrorWritingToFS;
            }
        }
    }
}
