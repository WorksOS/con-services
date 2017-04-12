using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Servers
{
    /// <summary>
    /// A base class for deriving server and client instances that interact witht eh Ignite In Memory Data Grid
    /// </summary>
    public class RaptorIgniteServer
    {
        protected IIgnite ignite = null;
        protected static ICache<String, MemoryStream> cache = null;

    }
}
