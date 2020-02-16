using System;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups
{
  public interface IDeviceLookup
  {
    Guid? Get(long storeId, string alias, string value);
    void Add(long storeId, string alias, string value, Guid uid);
    Guid? GetAssociatedAsset(Guid deviceUid);
  }
}
