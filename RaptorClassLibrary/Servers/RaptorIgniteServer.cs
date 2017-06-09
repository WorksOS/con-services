using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Servers.Compute;

namespace VSS.VisionLink.Raptor.Servers
{
    /// <summary>
    /// A base class for deriving server and client instances that interact with the Ignite In Memory Data Grid
    /// </summary>
    public class RaptorIgniteServer
    {
//        protected IIgnite spatialGrid /*ignite*/ = null;
        protected IIgnite raptorGrid /*ignite*/ = null;
        protected static ICache<String, MemoryStream> raptorCache = null;
        protected static ICache<String, MemoryStream> spatialCache = null;

    }
}
