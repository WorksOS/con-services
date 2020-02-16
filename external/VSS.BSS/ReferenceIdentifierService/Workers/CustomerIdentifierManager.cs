using System;
using System.Collections.Generic;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Workers
{
  public class CustomerIdentifierManager: ICustomerIdentifierManager
  {
    private readonly IStorage _storage;

    public CustomerIdentifierManager(IStorage storage)
    {
      _storage = storage;
    }

    public void Create(IdentifierDefinition identifierDefinition)
    {
      _storage.AddCustomerReference(identifierDefinition);
    }

    public void Update(IdentifierDefinition identifierDefinition)
    {
      _storage.UpdateCustomerReference(identifierDefinition);
    }

    public Guid? Retrieve(IdentifierDefinition identifierDefinition)
    {
      return _storage.FindCustomerReference(identifierDefinition);
    }

    public IList<IdentifierDefinition> FindDealers(IList<IdentifierDefinition> serviceViewOrgIdentifiers, long storeId)
    {
      return _storage.FindDealers(serviceViewOrgIdentifiers, storeId);
    }

    public Guid? FindCustomerGuidByCustomerId(long customerId)
    {
      return _storage.FindCustomerGuidByCustomerId(customerId);
    }
  }
}
