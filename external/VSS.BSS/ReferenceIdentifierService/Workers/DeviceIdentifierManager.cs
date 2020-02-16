using System;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Workers
{
  public class DeviceIdentifierManager: IDeviceIdentifierManager
  {
    private readonly IStorage _storage;

    public DeviceIdentifierManager(IStorage storage)
    {
      _storage = storage;
    }

    public void Create(IdentifierDefinition identifierDefinition)
    {
      _storage.AddDeviceReference(identifierDefinition);
    }

    public Guid? Retrieve(IdentifierDefinition identifierDefinition)
    {
      return _storage.FindDeviceReference(identifierDefinition);
    }

    public Guid? GetAssociatedAsset(Guid deviceUid)
    {
      return _storage.GetAssociatedAsset(deviceUid);
    }
  }
}
