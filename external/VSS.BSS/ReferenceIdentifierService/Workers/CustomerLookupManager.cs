using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.Bss;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Workers
{
  public class CustomerLookupManager: ICustomerLookupManager
  {
    private readonly IStorage _storage;

    public CustomerLookupManager(IStorage storage)
    {
      _storage = storage;
    }

    public IList<IdentifierDefinition> FindDealers(IList<IdentifierDefinition> serviceViewOrgIdentifiers, long storeId)
    {
      return _storage.FindDealers(serviceViewOrgIdentifiers, storeId);
    }

    public Guid? FindCustomerGuidByCustomerId(long customerId)
    {
      return _storage.FindCustomerGuidByCustomerId(customerId);
    }

    public List<Guid?> FindAllCustomersForService(Guid serviceUid)
    {
      return _storage.FindAllCustomersForService(serviceUid);
    }

    public Guid? FindCustomerParent(Guid childUid, CustomerTypeEnum parentCustomerType)
    {
      return _storage.FindCustomerParent(childUid, parentCustomerType);
    }

    public IList<AccountInfo> FindAccountsForDealer(Guid dealerUid)
    {
      return _storage.FindAccountsForDealer(dealerUid);
    }
  }
}
