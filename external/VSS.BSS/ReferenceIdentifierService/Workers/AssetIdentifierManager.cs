using System;
using System.Collections.Generic;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Workers
{
  public class AssetIdentifierManager: IAssetIdentifierManager
  {
    private readonly IStorage _storage;

    public AssetIdentifierManager(IStorage storage)
    {
      _storage = storage;
    }

    public void Create(IdentifierDefinition identifierDefinition)
    {
      _storage.AddAssetReference(identifierDefinition);
    }

    public Guid? Retrieve(IdentifierDefinition identifierDefinition)
    {
      return _storage.FindAssetReference(identifierDefinition);
    }

    public IList<Guid> GetAssociatedDevices(Guid assetUid)
    {
      return _storage.GetAssociatedDevices(assetUid);
    }

    public Guid? FindOwner(Guid assetUid)
    {
      return _storage.FindOwner(assetUid);
    }
  }
}
