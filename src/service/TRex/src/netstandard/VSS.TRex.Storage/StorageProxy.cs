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
      return DIContext.Obtain<IStorageProxyFactory>()?.Storage(mutability);
    }
  }
}
