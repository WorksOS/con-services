using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Servers.Compute
{
    /// <summary>
    /// A server type that represents a server useful for context processing sets of SubGrid information. This is essentially an analogue of
    /// the PSNode servers in legacy Raptor and contains both a cache of data and processing against it in response to client context server requests.
    /// </summary>
    public class RaptorSubGridProcessingServer : RaptorComputeServer
    {
        public RaptorSubGridProcessingServer()
        {

        }
    }
}
