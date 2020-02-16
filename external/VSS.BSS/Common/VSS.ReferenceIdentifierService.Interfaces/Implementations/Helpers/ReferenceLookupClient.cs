using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.Bss;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Constants;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Helpers;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Implementations.Helpers
{
  public class ReferenceLookupClient : IAssetLookup, IDeviceLookup, ICustomerLookup, IServiceLookup, ICredentialLookup, IStoreLookup, IOemLookup
  {
    private readonly IQueryHelper _queryHelper;

    public ReferenceLookupClient(IQueryHelper queryHelper)
    {
      _queryHelper = queryHelper;
    }

    #region Helper Methods

    private Guid? Retrieve(IdentifierDefinition idDef, string controllerName)
    {
      LookupResponse<Guid?> lookupResponse = _queryHelper.QueryServiceToRetrieve<LookupResponse<Guid?>, IdentifierDefinition>(controllerName, idDef);
      if (lookupResponse.Exception != null)
      {
        throw lookupResponse.Exception;
      }

      return lookupResponse.Data;
    }

    private Credentials RetrieveUrl(string url, string controllerName)
    {
      var response = _queryHelper.GetByQuery<LookupResponse<Credentials>>(controllerName, string.Format("url={0}", url));
      if (response.Exception != null)
      {
        throw response.Exception;
      }

      return response.Data;
    }

    private void Create(IdentifierDefinition idDef, string controllerName)
    {
      LookupResponse<Guid?> lookupResponse = _queryHelper.QueryServiceToCreate<LookupResponse<Guid?>, IdentifierDefinition>(controllerName, idDef);

      if (lookupResponse.Exception != null)
      {
        throw lookupResponse.Exception;
      }
    }

    #endregion

    #region IAssetLookup

    Guid? IAssetLookup.Get(long storeId, string alias, string value)
    {
      var identifierDefinition = new IdentifierDefinition {StoreId = storeId, Alias = alias, Value = value};
      return Retrieve(identifierDefinition, ControllerConstants.AssetIdentifierControllerName);
    }

    void IAssetLookup.Add(long storeId, string alias, string value, Guid uid)
    {
      var identifierDefinition = new IdentifierDefinition { StoreId = storeId, Alias = alias, Value = value, UID = uid };
      Create(identifierDefinition, ControllerConstants.AssetIdentifierControllerName);
    }

    IList<Guid> IAssetLookup.GetAssociatedDevices(Guid assetUid)
    {
      var lookupResponse = _queryHelper.GetByQuery<LookupResponse<IList<Guid>>>(ControllerConstants.AssetIdentifierControllerName, string.Format("assetUid={0}", assetUid), "GetAssociatedDevices");
      if (lookupResponse.Exception != null)
      {
        throw lookupResponse.Exception;
      }

      return lookupResponse.Data;
    }

    Guid? IAssetLookup.FindOwner(Guid assetUid)
    {
      var lookupResponse = _queryHelper.GetByQuery<LookupResponse<Guid?>>(ControllerConstants.AssetIdentifierControllerName, string.Format("assetUid={0}", assetUid), "FindOwner");
      if (lookupResponse.Exception != null)
      {
        throw lookupResponse.Exception;
      }

      return lookupResponse.Data;
    }

    #endregion

    #region IDeviceLookup

    Guid? IDeviceLookup.Get(long storeId, string alias, string value)
    {
      var identifierDefinition = new IdentifierDefinition { StoreId = storeId, Alias = alias, Value = value };
      return Retrieve(identifierDefinition, ControllerConstants.DeviceIdentifierControllerName);
    }

    void IDeviceLookup.Add(long storeId, string alias, string value, Guid uid)
    {
      var identifierDefinition = new IdentifierDefinition { StoreId = storeId, Alias = alias, Value = value, UID = uid };
      Create(identifierDefinition, ControllerConstants.DeviceIdentifierControllerName);
    }

    Guid? IDeviceLookup.GetAssociatedAsset(Guid deviceUid)
    {
      var lookupResponse = _queryHelper.GetByQuery<LookupResponse<Guid?>>(ControllerConstants.DeviceIdentifierControllerName, string.Format("deviceUid={0}", deviceUid), "GetAssociatedAsset");
      if (lookupResponse.Exception != null)
      {
        throw lookupResponse.Exception;
      }

      return lookupResponse.Data;
    }

    #endregion

    #region ICustomerLookup

    Guid? ICustomerLookup.Get(long storeId, string alias, string value)
    {
      var identifierDefinition = new IdentifierDefinition { StoreId = storeId, Alias = alias, Value = value };
      return Retrieve(identifierDefinition, ControllerConstants.CustomerIdentifierControllerName);
    }

    void ICustomerLookup.Add(long storeId, string alias, string value, Guid uid)
    {
      var identifierDefinition = new IdentifierDefinition { StoreId = storeId, Alias = alias, Value = value, UID = uid };
      Create(identifierDefinition, ControllerConstants.CustomerIdentifierControllerName);
    }

    void ICustomerLookup.Update(string alias, string value, Guid uid)
    {
      var identifierDefinition = new IdentifierDefinition { Alias = alias, Value = value, UID = uid };
      LookupResponse<Guid?> lookupResponse = _queryHelper.QueryServiceToUpdate<LookupResponse<Guid?>, IdentifierDefinition>(ControllerConstants.CustomerIdentifierControllerName, identifierDefinition);

      if (lookupResponse.Exception != null)
      {
        throw lookupResponse.Exception;
      }
    }

    IList<AccountInfo> ICustomerLookup.GetDealerAccounts(Guid customerUid)
    {
      var lookupResponse = _queryHelper.GetByQuery<LookupResponse<IList<AccountInfo>>>(ControllerConstants.CustomerLookupControllerName, string.Format("dealerUid={0}", customerUid), "FindAccountsForDealer");
      if (lookupResponse.Exception != null)
      {
        throw lookupResponse.Exception;
      }

      return lookupResponse.Data;
    }

    IList<IdentifierDefinition> ICustomerLookup.FindDealers(IList<IdentifierDefinition> serviceViewOrgIdentifiers, long storeId)
    {
      string svOrgIdentifiers = string.Empty;
      foreach (var svOrgIdentifier in serviceViewOrgIdentifiers)
      {
        svOrgIdentifiers += string.Format("{0},{1};", svOrgIdentifier.Alias, svOrgIdentifier.Value);
      }
      svOrgIdentifiers = string.Format("{0}", svOrgIdentifiers.TrimEnd(';'));
      var lookupResponse = _queryHelper.GetByQuery<LookupResponse<IList<IdentifierDefinition>>>(ControllerConstants.CustomerLookupControllerName, string.Format("storeId={0}&svOrgIdentifiers={1}", storeId, svOrgIdentifiers), "FindDealers");
      if (lookupResponse.Exception != null)
      {
        throw lookupResponse.Exception;
      }

      return lookupResponse.Data;
    }

    Guid? ICustomerLookup.FindCustomerGuidByCustomerId(long customerId)
    {
      var lookupResponse = _queryHelper.GetByQuery<LookupResponse<Guid?>>(ControllerConstants.CustomerLookupControllerName, string.Format("customerId={0}", customerId), "FindCustomerGuidByCustomerId");
      if (lookupResponse.Exception != null)
      {
        throw lookupResponse.Exception;
      }

      return lookupResponse.Data;
    }

    List<Guid?> ICustomerLookup.FindAllCustomersForService(Guid serviceUid)
    {
      var lookupResponse = _queryHelper.GetByQuery<LookupResponse<List<Guid?>>>(ControllerConstants.CustomerLookupControllerName, string.Format("serviceUid={0}", serviceUid), "FindAllCustomersForService");
      if (lookupResponse.Exception != null)
      {
        throw lookupResponse.Exception;
      }

      return lookupResponse.Data;
    }

    Guid? ICustomerLookup.FindCustomerParent(Guid childUid, CustomerTypeEnum parentCustomerType)
    {
      var lookupResponse = _queryHelper.GetByQuery<LookupResponse<Guid?>>(ControllerConstants.CustomerLookupControllerName, string.Format("childUid={0}&parentCustomerTypeString={1}", childUid, parentCustomerType), "FindCustomerParent");
      if (lookupResponse.Exception != null)
      {
        throw lookupResponse.Exception;
      }

      return lookupResponse.Data;
    }

    #endregion

    #region IServiceLookup

    Guid? IServiceLookup.Get(long storeId, string alias, string value)
    {
      var identifierDefinition = new IdentifierDefinition { StoreId = storeId, Alias = alias, Value = value };
      return Retrieve(identifierDefinition, ControllerConstants.ServiceIdentifierControllerName);
    }

    void IServiceLookup.Add(long storeId, string alias, string value, Guid uid)
    {
      var identifierDefinition = new IdentifierDefinition { StoreId = storeId, Alias = alias, Value = value, UID = uid };
      Create(identifierDefinition, ControllerConstants.ServiceIdentifierControllerName);
    }

    List<Guid?> IServiceLookup.FindActiveServiceForAsset(Guid assetUid)
    {
      var lookupResponse = _queryHelper.GetByQuery<LookupResponse<List<Guid?>>>(ControllerConstants.ServiceLookupControllerName, string.Format("assetUid={0}", assetUid), "GetAssetActiveServices");
      if (lookupResponse.Exception != null)
      {
        throw lookupResponse.Exception;
      }

      return lookupResponse.Data;
    }

    IList<ServiceLookupItem> IServiceLookup.DeviceActiveServices(string serialNumber, DeviceTypeEnum deviceType)
    {
      var lookupResponse = _queryHelper.GetByQuery<LookupResponse<IList<ServiceLookupItem>>>(ControllerConstants.ServiceLookupControllerName, string.Format("serialNumber={0}&deviceTypeString={1}", serialNumber, deviceType), "GetDeviceActiveServices");
      if (lookupResponse.Exception != null)
      {
        throw lookupResponse.Exception;
      }

      return lookupResponse.Data;
    }

    IList<ServiceLookupItem> IServiceLookup.AssetActiveServices(string serialNumber, string makeCode)
    {
      var lookupResponse = _queryHelper.GetByQuery<LookupResponse<IList<ServiceLookupItem>>>(ControllerConstants.ServiceLookupControllerName, string.Format("serialNumber={0}&makeCode={1}", serialNumber, makeCode), "GetAssetActiveServices");
      if (lookupResponse.Exception != null)
      {
        throw lookupResponse.Exception;
      }

      return lookupResponse.Data;
    }

    #endregion

    #region ICredentialLookup

    Credentials ICredentialLookup.Get(string url)
    {
      return RetrieveUrl(url, ControllerConstants.CredentialControllerName);
    }

    #endregion

    #region IStoreLookup

    long IStoreLookup.FindStoreByCustomerId(long customerId)
    {
      var lookupResponse = _queryHelper.GetByQuery<LookupResponse<long>>(ControllerConstants.StoreLookupControllerName, string.Format("customerId={0}", customerId), "FindStoreByCustomerId");
      if (lookupResponse.Exception != null)
      {
        throw lookupResponse.Exception;
      }

      return lookupResponse.Data;
    }

    #endregion

    #region IOemLookup

    int IOemLookup.FindOemIdentifierByCustomerId(long customerId)
    {
      var lookupResponse = _queryHelper.GetByQuery<LookupResponse<int>>(ControllerConstants.OemLookupControllerName, string.Format("customerId={0}", customerId), "FindOemIdentifierByCustomerId");
      if (lookupResponse.Exception != null)
      {
        throw lookupResponse.Exception;
      }

      return lookupResponse.Data;
    }

    #endregion
  }
}
