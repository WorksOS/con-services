using VSS.TRex.Interfaces;

namespace VSS.TRex.Storage.Interfaces
{
    /// <summary>
    /// Defines the interface for the TRex storage proxy factory
    /// </summary>
    public interface IStorageProxyFactory
    {
        /// <summary>
        /// Creates the storage proxy to be used. Currently hard wired to the Ignite storage proxy,
        /// should be replaced with the type from Dependency Injection when implemented.
        /// This factory method provides access to the mutable grid storage
        /// </summary>
        /// <returns></returns>
        IStorageProxy MutableGridStorage();

        /// <summary>
        /// Creates the storage proxy to be used. Currently hard wired to the Ignite storage proxy,
        /// should be replaced with the type from Dependency Injection when implemented.
        /// This factory method provides access to the immutable grid storage
        /// </summary>
        /// <returns></returns>
        IStorageProxy ImmutableGridStorage();

        /// <summary>
        /// Creates the storage proxy to be used. Currently hard wired to the Ignite storage proxy,
        /// should be replaced with the type from Dependency Injection when implemented.
        /// This factory method provides access to the immutable grid storage
        /// </summary>
        /// <returns></returns>
        IStorageProxy Storage(StorageMutability mutability);
    }
}
