using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces
{
  public interface IServiceLookupManager
  {
    List<Guid?> GetAssetActiveServices(Guid assetUid);
    IList<ServiceLookupItem> GetAssetActiveServices(string serialNumber, string makeCode);
    IList<ServiceLookupItem> GetDeviceActiveServices(string serialNumber, DeviceTypeEnum deviceType);
  }
}
