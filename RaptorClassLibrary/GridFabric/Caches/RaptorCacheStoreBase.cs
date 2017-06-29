using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Apache.Ignite.Core.Common;

namespace VSS.VisionLink.Raptor.GridFabric.Caches
{
    /// <summary>
    /// Implements the Ignite ICacheStore interface
    /// </summary>
    [Serializable]
    public class RaptorCacheStoreBase : CacheStoreAdapter, ICacheStore
    {
        /// <summary>
        /// The suffic to append to the filenames used to to Raptor Ignite Path to separate the immutable and immutable cache stores
        /// </summary>
        /// <returns></returns>
        protected virtual string MutabilitySuffix() => "(None)";

        /// <summary>
        /// The location to store the files containing the persisted cache entries
        /// </summary>
        private string path = "C:\\Temp\\RaptorIgniteData";

        /// <summary>
        /// Default constructor that adds the mutability suffix to the path to store persisted cache items
        /// </summary>
        public RaptorCacheStoreBase() : base()
        {
            path = path + MutabilitySuffix();

            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Deletes an item in the cache identified by the cache key
        /// </summary>
        /// <param name="key"></param>
        public override void Delete(object key)
        {
            // Remove the file representing this element from the file system
            try
            {
                File.Delete(Path.Combine(path, key.ToString()));
            }
            catch (FileNotFoundException)
            {
                // This is fine, carry on
            }
            catch (Exception E)
            {
                // This is less fine...
                throw E;
            }
        }

        /// <summary>
        /// Loads an item from the cache identified by the cache key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override object Load(object key)
        {
            MemoryStream MS = null; // object obj = null;
            bool ok = false;
            while (!ok)
            {
                string fileName = Path.Combine(path, key.ToString());
                try
                {
                    /*
                    BinaryFormatter bf = new BinaryFormatter();
                    using (FileStream fs = new FileStream(fileName, FileMode.Open))
                    {
                        obj = bf.Deserialize(fs);
                    }
                    */

                    MS = new MemoryStream();
                    using (FileStream fs = new FileStream(fileName, FileMode.Open))
                    {
                        fs.CopyTo(MS);
                    }

                    ok = true;
                }
                catch (FileNotFoundException E)
                {
                    Console.WriteLine("Cache Get() for key {0}, filename {1} resulted in file not found, returning null", key.ToString(), fileName);
                    return null;
                }
                catch (Exception E)
                {
                    Console.WriteLine("Cache Get() blocked on key {0}, waiting 1 second.", key.ToString());
                    System.Threading.Thread.Sleep(1000);
                }
            }

            return MS; // obj;        
        }

        /// <summary>
        /// Full cache load. Not implemented.
        /// </summary>
        /// <param name="act"></param>
        /// <param name="args"></param>
        public void LoadCache(Action<object, object> act, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void SessionEnd(bool commit)
        {
            // Nothing to do here
        }

        /// <summary>
        /// Writes a cache item identitied by the cache key to the persisted store
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public override void Write(object key, object val)
        {
            // BinaryFormatter bf = new BinaryFormatter();
            MemoryStream MS = new MemoryStream();

            try
            {
                // bf.Serialize(ms, val);
                MS = val as MemoryStream;
            }
            catch (Exception E)
            {
                throw;
            }

            using (FileStream fs = new FileStream(Path.Combine(path, key.ToString()), FileMode.Create))
            {
                try
                {
                    MS.Position = 0;
                    MS.WriteTo(fs);
                }
                finally
                {
                    fs.Close();
                }
            }
        }
    }
}
