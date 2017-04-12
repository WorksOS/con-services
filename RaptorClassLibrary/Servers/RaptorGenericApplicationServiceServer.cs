using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Servers
{
    /// <summary>
    /// An application service server that can take a generic action to perform in the context of an application service service
    /// </summary>
    public class RaptorGenericApplicationServiceServer : RaptorApplicationServiceServer, IDisposable
    {
        public RaptorGenericApplicationServiceServer() : base()
        {
        }

        public static TResult PerformAction<TResult>(Func<TResult> func)
        {
            using (var server = new RaptorGenericApplicationServiceServer())
            {
                return func();
            }
        }

        public void Dispose()
        {
            // No specialist IDispose behaviour yet
        }
    }
}
