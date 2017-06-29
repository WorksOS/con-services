using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.GridFabric.Caches
{
    /// <summary>
    /// The Raptor cache store responsible for mutable object caches
    /// </summary>
    [Serializable]
    public class RaptorMutableCacheStore : RaptorCacheStoreBase
    {
        protected override string MutabilitySuffix() => "(Mutable)";
    }
}
