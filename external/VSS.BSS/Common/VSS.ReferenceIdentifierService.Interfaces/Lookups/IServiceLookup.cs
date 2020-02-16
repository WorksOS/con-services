using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups
{
  public interface IServiceLookup
  {
    Guid? Get(long storeId, string alias, string value);
    void Add(long storeId, string alias, string value, Guid uid);
    List<Guid?> FindActiveServiceForAsset(Guid assetUid);
    IList<ServiceLookupItem> DeviceActiveServices(string serialNumber, DeviceTypeEnum compositeDeviceType);
    IList<ServiceLookupItem> AssetActiveServices(string serialNumber, string makeCode);
  }
}
