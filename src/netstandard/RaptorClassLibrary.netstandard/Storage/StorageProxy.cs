using System;
using VSS.TRex.DI;
using VSS.TRex.Interfaces;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Storage
{
  /// <summary>
  ///  StorageProxy hides the implementation details of the underlying storage metaphor and provides an
  ///  IStorageProxy interface on demand.
  /// </summary>
  public static class StorageProxy
  {
    // Get the storage proxy factory from the DI context
    private static IStorageProxyFactory Factory = DIContext.Obtain<IStorageProxyFactory>();

    public static IStorageProxy Instance(StorageMutability mutability)
    {
      switch (mutability)
      {
        case StorageMutability.Mutable: return Factory?.Storage(StorageMutability.Mutable); // ?? StorageProxyFactory.Storage(StorageMutability.Mutable);
        case StorageMutability.Immutable: return Factory?.Storage(StorageMutability.Immutable); // ?? StorageProxyFactory.Storage(StorageMutability.Immutable);
        default:
          throw new ArgumentException($"{mutability} is an unknown mutability type");
      }
    }
  }
}
