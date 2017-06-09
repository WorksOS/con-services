using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Interfaces;

namespace VSS.VisionLink.Raptor.Storage
{
    /// <summary>
    ///  StorageProxyFactory is a factory for the storage proxy used in Raptor
    /// </summary>
    public static class StorageProxyFactory
    {
        /// <summary>
        /// Creates the storage proxy to be used. Currently hard wired to the Igniote storage proxy,
        /// should be replaced with the type from Dependency Injection when implemented.
        /// </summary>
        /// <returns></returns>
        public static IStorageProxy Storage(string gridName)
        {
            return new StorageProxy_Ignite(gridName);
        }
    }
}
