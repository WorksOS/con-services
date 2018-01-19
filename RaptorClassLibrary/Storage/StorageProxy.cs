using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Interfaces;

namespace VSS.VisionLink.Raptor.Storage
{
    /// <summary>
    ///  StorageProxy hides the implementation details of the underlying storage metaphor and provides an
    ///  IStorageProxy interface on demand.
    /// </summary>
    public static class StorageProxy
    {
        private static IStorageProxy raptorInstance_Mutable = null;
        private static IStorageProxy raptorInstance_Immutable = null;

        public static IStorageProxy RaptorInstance(StorageMutability mutability)
        {
            switch (mutability)
            {
                case StorageMutability.Mutable: return raptorInstance_Mutable ?? (raptorInstance_Mutable = StorageProxyFactory.Storage(StorageMutability.Mutable));
                case StorageMutability.Immutable: return raptorInstance_Immutable ?? (raptorInstance_Immutable = StorageProxyFactory.Storage(StorageMutability.Immutable));
                default:
                    throw new ArgumentException($"{mutability} is an unknown mutability type");
            }
        }
    }
}
