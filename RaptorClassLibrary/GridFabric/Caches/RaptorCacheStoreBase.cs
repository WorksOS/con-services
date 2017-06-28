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
        protected virtual string MutabilitySuffix() => " (None)";

        private static string path = "C:\\Temp\\RaptorIgniteData";

        public RaptorCacheStoreBase() : base()
        {
            path = path + MutabilitySuffix();

            Directory.CreateDirectory(path);
        }

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
