using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.Bss;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces
{
  public interface ICustomerLookupManager
  {
    IList<IdentifierDefinition> FindDealers(IList<IdentifierDefinition> serviceViewOrgIdentifiers, long storeId);
    Guid? FindCustomerGuidByCustomerId(long customerId);
    List<Guid?> FindAllCustomersForService(Guid serviceUid);
    Guid? FindCustomerParent(Guid childUid, CustomerTypeEnum parentCustomerType);
    IList<AccountInfo> FindAccountsForDealer(Guid dealerUid);
  }
}
