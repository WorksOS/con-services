using System;
using System.Collections.Generic;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces
{
  public interface ICustomerIdentifierManager
  {
    void Create(IdentifierDefinition identifierDefinition);
    void Update(IdentifierDefinition identifierDefinition);
    Guid? Retrieve(IdentifierDefinition identifierDefinition);
    IList<IdentifierDefinition> FindDealers(IList<IdentifierDefinition> serviceViewOrgIdentifiers, long storeId);
    Guid? FindCustomerGuidByCustomerId(long customerId);
  }
}
