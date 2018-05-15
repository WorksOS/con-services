using System;
using VSS.TRex.DI;
using VSS.TRex.Interfaces;

namespace VSS.TRex.Storage
{
    /// <summary>
    ///  StorageProxyFactory is a factory for the storage proxy used in TRex
    /// </summary>
    public static class StorageProxyFactory
    {
        /// <summary>
        /// Creates the storage proxy to be used. Currently hard wired to the Ignite storage proxy,
        /// should be replaced with the type from Dependency Injection when implemented.
        /// This factory method provides access to the mutable grid storage
        /// </summary>
        /// <returns></returns>
        public static IStorageProxy MutableGridStorage()
        {
            IStorageProxy proxy = 
                StorageProxyDIContext.storageProxyFactory?.MutableGridStorage() ??
                new StorageProxy_Ignite_Transactional(StorageMutability.Mutable);

            // Establish any available immutable storage proxy into the mutable storage proxy to allow transparnet
            // promotion of data changes in the mutable data store to the immutabvle data store.
            proxy.SetImmutableStorageProxy(ImmutableGridStorage());

            return proxy;
        }

        /// <summary>
        /// Creates the storage proxy to be used. Currently hard wired to the Ignite storage proxy,
        /// should be replaced with the type from Dependency Injection when implemented.
        /// This factory method provides access to the immutable grid storage
        /// </summary>
        /// <returns></returns>
        public static IStorageProxy ImmutableGridStorage()
        {
            return StorageProxyDIContext.storageProxyFactory?.ImmutableGridStorage() ??
                   new StorageProxy_Ignite_Transactional(StorageMutability.Immutable);
        }

        /// <summary>
        /// Creates the storage proxy to be used. Currently hard wired to the Ignite storage proxy,
        /// should be replaced with the type from Dependency Injection when implemented.
        /// This factory method provides access to the immutable grid storage
        /// </summary>
        /// <returns></returns>
        public static IStorageProxy Storage(StorageMutability mutability)
        {
            switch (mutability)
            {
                case StorageMutability.Mutable: return MutableGridStorage();
                case StorageMutability.Immutable: return ImmutableGridStorage();
                default:
                    throw new ArgumentException($"Unknown mutability type {mutability} in proxy storage factory.");
            }
        }
    }
}
