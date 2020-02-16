using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Workers
{
  public class ServiceIdentifierManager: IServiceIdentifierManager
  {
    private readonly IStorage _storage;

    public ServiceIdentifierManager(IStorage storage)
    {
      _storage = storage;
    }

    public void Create(IdentifierDefinition identifierDefinition)
    {
      _storage.AddServiceReference(identifierDefinition);
    }

    public Guid? Retrieve(IdentifierDefinition identifierDefinition)
    {
      return _storage.FindServiceReference(identifierDefinition);
    }
  }
}
