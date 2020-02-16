using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.Bss;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups
{
  public interface ICustomerLookup
  {
    Guid? Get(long storeId, string alias, string value);
    void Add(long storeId, string alias, string value, Guid uid);
    void Update(string alias, string value, Guid uid);
    IList<AccountInfo> GetDealerAccounts(Guid customerUid);
    IList<IdentifierDefinition> FindDealers(IList<IdentifierDefinition> serviceViewOrgIdentifiers, long storeId);
    Guid? FindCustomerGuidByCustomerId(long customerId);
    List<Guid?> FindAllCustomersForService(Guid serviceUid);
    Guid? FindCustomerParent(Guid childUid, CustomerTypeEnum parentCustomerType);
  }
}
