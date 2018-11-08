using Apache.Ignite.Core.Cache.Store;
using System;
using System.IO;

namespace VSS.TRex.Storage.Caches
{
    public class TRexNonSpatialCacheStore : CacheStoreAdapter<string, MemoryStream>
    {
        private TRexCacheStoreUtilities Utilities;

        public TRexNonSpatialCacheStore(string mutabilitySuffix)
        {
            Utilities = new TRexCacheStoreUtilities(mutabilitySuffix);
        }

        public override void Delete(string key)
        {
            Utilities.Delete(key);
        }

        public override MemoryStream Load(string key)
        {
            return Utilities.Load(key);
        }

        public override void LoadCache(Action<string, MemoryStream> act, params object[] args)
        {
            // Ignore - not a supported activity
            // throw new NotImplementedException();
        }

        public override void SessionEnd(bool commit)
        {
            // Ignore, nothign to do
            // throw new NotImplementedException();
        }

        public override void Write(string key, MemoryStream val)
        {
            Utilities.Write(key, val);
        }
    }
}
