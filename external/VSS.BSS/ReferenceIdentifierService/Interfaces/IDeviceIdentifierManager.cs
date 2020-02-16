using System;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces
{
  public interface IDeviceIdentifierManager
  {
    void Create(IdentifierDefinition identifierDefinition);
    Guid? Retrieve(IdentifierDefinition identifierDefinition);
    Guid? GetAssociatedAsset(Guid deviceUid);
  }
}
