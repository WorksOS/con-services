using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Eviction;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Affinity;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.Storage.Utilities;
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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            StoreGranuleIndex = 0;
            StoreGranuleCount = 0;

            try
            {
                SubGridSpatialAffinityKey cacheKey = new SubGridSpatialAffinityKey(DataModelID, SubgridX, SubgridY, SegmentIdentifier);

                Log.Info(String.Format("Getting key:{0}", StreamName));

                if (ReadFromImmutableDataCaches)
                {
                    try
                    {
                        // First look to see if the immutable item is in the cache
                        Stream = MemoryStreamCompression.Decompress(immutableSpatialCache.Get(cacheKey));
                    }
                    catch (KeyNotFoundException e)
                    {
                        Stream = PerformSpatialImmutabilityConversion(mutableSpatialCache, immutableSpatialCache, cacheKey, StreamType);
                    }
                }
                else
                {
                    Stream = MemoryStreamCompression.Decompress(mutableSpatialCache.Get(cacheKey));
                }

                Stream.Position = 0;

                return FileSystemErrorStatus.OK;
            }
            catch (Exception e)
            {
                Log.Info(String.Format("Exception occurred: {0}", e));

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

                Log.Info(String.Format("Getting key:{0}", cacheKey));

                if (ReadFromImmutableDataCaches)
                {
                    try
                    {
                        Stream = MemoryStreamCompression.Decompress(immutableNonSpatialCache.Get(cacheKey));
                    }
                    catch (KeyNotFoundException e)
                    {
                        Stream = PerformNonSpatialImmutabilityConversion(mutableNonSpatialCache, immutableNonSpatialCache, cacheKey, StreamType);
                    }
                }
                else
                {
                    Stream = MemoryStreamCompression.Decompress(mutableNonSpatialCache.Get(cacheKey));
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

                Log.Info(String.Format("Removing key:{0}", cacheKey));

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

                Log.Info(String.Format("Putting key:{0}, size:{1}", cacheKey, Stream.Length));

                using (MemoryStream compressedStream = MemoryStreamCompression.Compress(Stream))
                {
                    mutableSpatialCache.Put(cacheKey, compressedStream);
                }
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

                Log.Info(String.Format("Putting key:{0}, size:{1}", cacheKey, Stream.Length));

                using (MemoryStream compressedStream = MemoryStreamCompression.Compress(Stream))
                {
                    mutableNonSpatialCache.Put(cacheKey, compressedStream);
                }

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

                Log.Info(String.Format("Putting key:{0}, size:{1}", cacheKey, Stream.Length));

                using (MemoryStream compressedStream = MemoryStreamCompression.Compress(Stream))
                {
                    mutableNonSpatialCache.Put(cacheKey, compressedStream);
                }

                try
                {
                    // Invalidate the immutable version if there is a cache reference
                    immutableNonSpatialCache.GetAndRemove(cacheKey);
                }
                catch (Exception e)
                {
                    // Ignore any exception here which is typically thrown if the element in the
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
