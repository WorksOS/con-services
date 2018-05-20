using System;
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
    private static IStorageProxyFactory Factory { get; set; }

    public static void Inject(IStorageProxyFactory factory) => Factory = factory;

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
