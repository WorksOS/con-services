using System;
using VSS.VisionLink.Raptor.Interfaces;

namespace VSS.VisionLink.Raptor.Storage
{
    /// <summary>
    ///  StorageProxy hides the implementation details of the underlying storage metaphor and provides an
    ///  IStorageProxy interface on demand.
    /// </summary>
    public static class StorageProxy
    {
        public static IStorageProxy RaptorInstance(StorageMutability mutability)
        {
            switch (mutability)
            {
                case StorageMutability.Mutable: return StorageProxyFactory.Storage(StorageMutability.Mutable);
                case StorageMutability.Immutable: return StorageProxyFactory.Storage(StorageMutability.Immutable);
                default:
                    throw new ArgumentException($"{mutability} is an unknown mutability type");
            }
        }
    }
}
