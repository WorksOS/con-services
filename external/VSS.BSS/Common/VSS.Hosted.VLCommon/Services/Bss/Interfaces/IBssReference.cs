using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon.Services.Bss;

namespace VSS.Hosted.VLCommon.Services.Interfaces
{
  public interface IBssReference
  {
    void AddAssetReference(long storeId, string alias, string value, Guid uid);
    void AddCustomerReference(long storeId, string alias, string value, Guid uid);
    void UpdateCustomerReference(string alias, string value, Guid uid);
    IList<AccountInfo> GetDealerAccounts(Guid customerUid);
  }
}
