using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces
{
  public interface IServiceIdentifierManager
  {
    void Create(IdentifierDefinition identifierDefinition);
    Guid? Retrieve(IdentifierDefinition identifierDefinition);
  }
}
