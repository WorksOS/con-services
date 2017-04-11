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
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Storage
{
    /// <summary>
    /// Implementation of the IStorageProxy interface that allows to read/write operations against Ignite based IO support.
    /// Note: All read and write operations are sending and receiving MemoryStream objects.
    /// </summary>
    public class StorageProxy_Ignite : IStorageProxy
    {
        private static IStorageProxy _instance = null;

        private static IIgnite ignite = null;

        private static ICache<String, MemoryStream> cache = null; 

        private static Object LockObj = new Object();

        public StorageProxy_Ignite()
        {
            if (ignite == null)
            {
                IgniteConfiguration cfg = new IgniteConfiguration()
                {
                    GridName = "Raptor",

                    // Register custom class for Ignite serialization
                    BinaryConfiguration = new Apache.Ignite.Core.Binary.BinaryConfiguration(typeof(MemoryStream)),

                    JvmMaxMemoryMb = 6000
                };

                ignite = Ignition.Start(cfg);
                ignite = Ignition.TryGetIgnite("Raptor");

                if (ignite == null)
                {
                    ignite = Ignition.GetIgnite();
                }

                // Add a cache to Ignite
                cache = ignite.GetOrCreateCache<String, MemoryStream>
                    (new CacheConfiguration()
                    {
                        Name = "DataModels",
                        CopyOnRead = false,
                        KeepBinaryInStore = false,
                        MemoryMode = CacheMemoryMode.OnheapTiered,
                        CacheStoreFactory = new RaptorCacheStoreFactory(),
                        ReadThrough = true,
                        WriteThrough = true,
                        WriteBehindFlushFrequency = new TimeSpan(0, 0, 30), // 30 seconds 
                            EvictionPolicy = new LruEvictionPolicy()
                        {
                            MaxMemorySize = 2000000000,
                        }
                    });
            }
        }

        public static IStorageProxy Instance()
        {
            if (_instance == null)
            {
                lock (LockObj)
                {
                    if (_instance == null)
                    {
                        _instance = new StorageProxy_Ignite();
                    }
                }
            }

            return _instance;
        }

        private static string ComputeNamedStreamCacheKey(long DataModelID, string Name)
        {
            return String.Format("{0}-{1}", DataModelID, Name);
        }

        private static string ComputeNamedStreamCacheKey(long DataModelID, string Name, uint SubgridX, uint SubgridY)
        {
            return String.Format("{0}-{1}-{2}-{3}", DataModelID, Name, SubgridX, SubgridY);
        }

        public FileSystemErrorStatus ReadSpatialStreamFromPersistentStore(long DataModelID, string StreamName, uint SubgridX, uint SubgridY, FileSystemSpatialStreamType StreamType, uint GranuleIndex, out MemoryStream Stream, out uint StoreGranuleIndex, out uint StoreGranuleCount)
        {
            StoreGranuleIndex = 0;
            StoreGranuleCount = 0;

            try
            {
                Stream = cache.Get(ComputeNamedStreamCacheKey(DataModelID, StreamName/*, SubgridX, SubgridY*/));
                return FileSystemErrorStatus.OK;
            }
            catch (Exception E)
            {
                Stream = null;
                return FileSystemErrorStatus.UnknownErrorReadingFromFS;
            }
        }

        public FileSystemErrorStatus ReadStreamFromPersistentStore(long DataModelID, string StreamName, out MemoryStream Stream)
        {
            try
            {
                string CacheKey = ComputeNamedStreamCacheKey(DataModelID, StreamName);
                Stream = cache.Get(CacheKey);
                Stream.Position = 0;
                return FileSystemErrorStatus.OK;
            }
            catch (Exception E)
            {
                Stream = null;
                return FileSystemErrorStatus.UnknownErrorReadingFromFS;
            }
        }

        public FileSystemErrorStatus ReadStreamFromPersistentStore(long DataModelID, string StreamName, out MemoryStream Streamout, uint StoreGranuleIndex, out uint StoreGranuleCount)
        {
            StoreGranuleCount = 0;
            StoreGranuleIndex = 0;

            return ReadStreamFromPersistentStore(DataModelID, StreamName, out Streamout);
        }

        public FileSystemErrorStatus ReadStreamFromPersistentStoreDirect(long DataModelID, string StreamName, out MemoryStream Stream)
        {
            return ReadStreamFromPersistentStore(DataModelID, StreamName, out Stream);
        }

        public FileSystemErrorStatus RemoveStreamFromPersistentStore(long DataModelID, string StreamName)
        {
            try
            {
                cache.Remove(ComputeNamedStreamCacheKey(DataModelID, StreamName));
                return FileSystemErrorStatus.OK;
            }
            catch
            {
                return FileSystemErrorStatus.UnknownErrorWritingToFS;
            }
        }

        public FileSystemErrorStatus WriteSpatialStreamToPersistentStore(long DataModelID, string StreamName, uint SubgridX, uint SubgridY, FileSystemSpatialStreamType StreamType, out uint StoreGranuleIndex, out uint StoreGranuleCount, MemoryStream Stream)
        {
            StoreGranuleCount = 0;
            StoreGranuleIndex = 0;

            try
            {
                cache.Put(ComputeNamedStreamCacheKey(DataModelID, StreamName/*, SubgridX, SubgridY*/), Stream);
                return FileSystemErrorStatus.OK;
            }
            catch
            {
                return FileSystemErrorStatus.UnknownErrorWritingToFS;
            }
        }

        public FileSystemErrorStatus WriteStreamToPersistentStore(long DataModelID, string StreamName, FileSystemGranuleType StreamType, out uint StoreGranuleIndex, out uint StoreGranuleCount, MemoryStream Stream)
        {
            StoreGranuleCount = 0;
            StoreGranuleIndex = 0;

            try
            {
                cache.Put(ComputeNamedStreamCacheKey(DataModelID, StreamName), Stream);
                return FileSystemErrorStatus.OK;
            }
            catch
            {
                return FileSystemErrorStatus.UnknownErrorWritingToFS;
            }
        }

        public FileSystemErrorStatus WriteStreamToPersistentStoreDirect(long DataModelID, string StreamName, FileSystemGranuleType StreamType, MemoryStream Stream)
        {
            try
            {

                cache.Put(ComputeNamedStreamCacheKey(DataModelID, StreamName), Stream);
                return FileSystemErrorStatus.OK;
            }
            catch
            {
                return FileSystemErrorStatus.UnknownErrorWritingToFS;
            }
        }
    }
}
