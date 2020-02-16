using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;

namespace VSS.Nighthawk.ReferenceIdentifierService.Workers
{
  public class ServiceLookupManager: IServiceLookupManager
  {
    private readonly IStorage _storage;

    public ServiceLookupManager(IStorage storage)
    {
      _storage = storage;
    }

    public List<Guid?> GetAssetActiveServices(Guid assetUid)
    {
      return _storage.GetAssetActiveServices(assetUid);
    }

    public IList<ServiceLookupItem> GetAssetActiveServices(string serialNumber, string makeCode)
    {
      return _storage.GetAssetActiveServices(serialNumber, makeCode);
    }

    public IList<ServiceLookupItem> GetDeviceActiveServices(string serialNumber, DeviceTypeEnum deviceType)
    {
      return _storage.GetDeviceActiveServices(serialNumber, deviceType);
    }
  }
}
