using log4net;
using System;
using System.Linq;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;
using VSS.Hosted.VLCommon.Services.Bss;

namespace VSS.Nighthawk.NHBssSvc
{
  public class BssReference : IBssReference
  {
    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private readonly IAssetLookup _assetLookupService;
    private readonly ICustomerLookup _customerLookupService;

    public BssReference(IAssetLookup assetLookupService, ICustomerLookup customerLookupService)
    {
      _assetLookupService = assetLookupService;
      _customerLookupService = customerLookupService;
    }

    public void AddAssetReference(long storeId, string alias, string value, Guid uid)
    {
      Log.IfInfoFormat("AddBssReference.AddAssetReference: Adding storeId={0}, alias={1}, value={2}, uid={3}", storeId, alias, value, uid);
      var existingUid = _assetLookupService.Get(storeId, alias, value);
      if (existingUid != null)
      {
        Log.IfInfoFormat("AddBssReference.AddAssetReference: storeId={0}, alias={1}, value={2}, uid={3} already exists", storeId, alias, value, existingUid);
        return;
      }
      _assetLookupService.Add(storeId, alias, value, uid);
    }

    public void AddCustomerReference(long storeId, string alias, string value, Guid uid)
    {
      Log.IfInfoFormat("AddBssReference.AddCustomerReference: Adding storeId={0}, alias={1}, value={2}, uid={3}", storeId, alias, value, uid);
      var existingUid = _customerLookupService.Get(storeId, alias, value);
      if (existingUid != null)
      {
        Log.IfInfoFormat("AddBssReference.AddCustomerReference: storeId={0}, alias={1}, value={2}, uid={3} already exists", storeId, alias, value, existingUid);
        return;
      }

      _customerLookupService.Add(storeId, alias, value, uid);
    }

    public void UpdateCustomerReference(string alias, string value, Guid uid)
    {
      Log.IfInfoFormat("BssReference.UpdateCustomerReference: Updating alias={0}, value={1}, uid={2}", alias, value, uid);
      _customerLookupService.Update(alias, value, uid);
    }

    public IList<AccountInfo> GetDealerAccounts(Guid customerUid)
    {
      Log.IfInfoFormat("BssReference.GetDealerAccounts: Getting accounts for Uid={0}", customerUid);
      return _customerLookupService.GetDealerAccounts(customerUid);

    }
  }
}
