using System;
using System.Collections.Generic;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups
{
  public interface IAssetLookup
  {
    Guid? Get(long storeId, string alias, string value);
    void Add(long storeId, string alias, string value, Guid uid);
    IList<Guid> GetAssociatedDevices(Guid assetUid);
    Guid? FindOwner(Guid assetUid);
  }
}
