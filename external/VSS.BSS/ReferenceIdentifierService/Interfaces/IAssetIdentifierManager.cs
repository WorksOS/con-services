using System;
using System.Collections.Generic;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces
{
  public interface IAssetIdentifierManager
  {
    void Create(IdentifierDefinition identifierDefinition);
    Guid? Retrieve(IdentifierDefinition identifierDefinition);
    IList<Guid> GetAssociatedDevices(Guid assetUid);
    Guid? FindOwner(Guid assetUid);
  }
}
