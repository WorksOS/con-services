using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Interfaces;

namespace VSS.VisionLink.Raptor.Storage
{
    /// <summary>
    ///  StorageProxy hides the implementation details of the underlying storage metaphor and provides an
    ///  IStorageProxy interface on demand.
    /// </summary>
    public static class StorageProxy
    {
        private static IStorageProxy instance = null;

        public static IStorageProxy Instance()
        {
            if (instance == null)
            {
                instance = StorageProxyFactory.Storage();
            }

            return instance;
        }
    }
}
