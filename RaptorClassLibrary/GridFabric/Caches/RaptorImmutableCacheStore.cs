using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.GridFabric.Caches
{
    /// <summary>
    /// The Raptor cache store responsible for immutable object caches
    /// </summary>
    [Serializable]
    public class RaptorImmutableCacheStore : RaptorCacheStoreBase
    {
        protected override string MutabilitySuffix() => "(Immutable)";
    }
}
