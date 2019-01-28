using System;
using VSS.TRex.DI;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;

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
      var Factory = DIContext.Obtain<IStorageProxyFactory>();

      switch (mutability)
      {
        case StorageMutability.Mutable: return Factory?.Storage(StorageMutability.Mutable);
        case StorageMutability.Immutable: return Factory?.Storage(StorageMutability.Immutable); 
        default:
          throw new ArgumentException($"{mutability} is an unknown mutability type");
      }
    }
  }
}
