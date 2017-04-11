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

namespace VSS.VisionLink.Raptor.Storage
{
    /// <summary>
    /// An implementation of the Ignite read-through/write-through cache persistency interface
    /// Note: All read and write operations are sending and receiving MemoryStream objects.
    /// </summary>
    [Serializable]
    public class RaptorCacheStoreFactory : IFactory<ICacheStore>
    {
        public ICacheStore CreateInstance()
        {
            return new RaptorCacheStore();
        }
    }

    /// <summary>
    /// Implements the Ignite ICacheStore interface
    /// </summary>
    [Serializable]
    public class RaptorCacheStore : CacheStoreAdapter, ICacheStore
    {
        private const string path = "C:\\Temp\\RaptorIgniteData";

        public override void Delete(object key)
        {
            throw new NotImplementedException();
        }

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

        public void LoadCache(Action<object, object> act, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void SessionEnd(bool commit)
        {
            // Nothing to do here
        }

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

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
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
