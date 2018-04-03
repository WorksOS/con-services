using Apache.Ignite.Core.Cache.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.GridFabric.Caches
{
    public class RaptorNonSpatialCacheStore : CacheStoreAdapter<string, MemoryStream>
    {
        private RaptorCacheStoreUtilities Utilities = null;

        public RaptorNonSpatialCacheStore(string mutabilitySuffix) : base()
        {
            Utilities = new RaptorCacheStoreUtilities(mutabilitySuffix);
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
