using Apache.Ignite.Core.Cache.Store;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Apache.Ignite.Core.Common;

namespace Apache_Ignite_DotNet_Test
{
    [Serializable]
    public class TRexCacheStoreFactory : IFactory<ICacheStore>
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
    public class RaptorCacheStore : CacheStoreAdapter<object, object>, ICacheStore
    {
        private const string path = "C:\\Temp\\TRexIgniteData";

        public override void Delete(object key)
        {
            throw new NotImplementedException();
        }

        public override object Load(object key)
        {
            object obj = null;
            bool ok = false;
            while (!ok)
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream fs = new FileStream(Path.Combine(path, key.ToString()), FileMode.Open);

                    obj = bf.Deserialize(fs);
                    ok = true;
                }
                catch // (Exception E)
                {
                    Console.WriteLine("Cache Get() blocked on key {0}, waiting 1 second.", key.ToString());
                    System.Threading.Thread.Sleep(1000);
                }
            }

            return obj;        
        }

        public override void LoadCache(Action<object, object> act, params object[] args)
        {
            throw new NotImplementedException();
        }

        public override void SessionEnd(bool commit)
        {
            // Nothing to do here
        }

        public override void Write(object key, object val)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            try
            {
                bf.Serialize(ms, val);
            }
            catch // (Exception E)
            {
                throw;
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            FileStream fs = new FileStream(Path.Combine(path, key.ToString()), FileMode.Create);

            ms.Position = 0;
            ms.WriteTo(fs);
        }
    }
}
