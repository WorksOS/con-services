﻿using System;
using VSS.TRex.Interfaces;

namespace VSS.TRex.Storage
{
    /// <summary>
    ///  StorageProxy hides the implementation details of the underlying storage metaphor and provides an
    ///  IStorageProxy interface on demand.
    /// </summary>
    public static class StorageProxy
    {
        public static IStorageProxy Instance(StorageMutability mutability)
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
