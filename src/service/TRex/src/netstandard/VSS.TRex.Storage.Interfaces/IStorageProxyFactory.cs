using VSS.TRex.Storage.Models;

namespace VSS.TRex.Storage.Interfaces
{
    /// <summary>
    /// Defines the interface for the TRex storage proxy factory
    /// </summary>
    public interface IStorageProxyFactory
    {
        /// <summary>
        /// Creates the storage proxy to be used. 
        /// This factory method provides access to the mutable grid storage
        /// </summary>
        /// <returns></returns>
        IStorageProxy MutableGridStorage();

        /// <summary>
        /// Creates the storage proxy to be used. 
        /// This factory method provides access to the immutable grid storage
        /// </summary>
        /// <returns></returns>
        IStorageProxy ImmutableGridStorage();

        /// <summary>
        /// Creates the storage proxy to be used. 
        /// This factory method provides access to the immutable grid storage
        /// </summary>
        /// <returns></returns>
        IStorageProxy Storage(StorageMutability mutability);
    }
}
