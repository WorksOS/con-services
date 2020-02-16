using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.Bss;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces
{
  public interface IStorage
  {
    Guid? FindCustomerReference(IdentifierDefinition identifierDefinition);
    void AddCustomerReference(IdentifierDefinition identifierDefinition);
    void UpdateCustomerReference(IdentifierDefinition identifierDefinition);
    IList<AccountInfo> FindAccountsForDealer(Guid dealerUid);
    IList<IdentifierDefinition> FindDealers(IList<IdentifierDefinition> orgIdentifierDefinitions, long storeId);
    Guid? FindCustomerGuidByCustomerId(long customerId);
    List<Guid?> FindAllCustomersForService(Guid serviceUid);
    Guid? FindCustomerParent(Guid childUid, CustomerTypeEnum parentCustomerType);

    long FindStoreByCustomerId(long customerId);

    int FindOemIdentifierByCustomerId(long customerId);

    Guid? FindAssetReference(IdentifierDefinition identifierDefinition);
    void AddAssetReference(IdentifierDefinition identifierDefinition);
    IList<Guid> GetAssociatedDevices(Guid assetUid);
    Guid? FindOwner(Guid assetUid);

    Guid? FindDeviceReference(IdentifierDefinition identifierDefinition);
    void AddDeviceReference(IdentifierDefinition identifierDefinition);
    Guid? GetAssociatedAsset(Guid deviceUid);

    Guid? FindServiceReference(IdentifierDefinition identifierDefinition);
    void AddServiceReference(IdentifierDefinition identifierDefinition);
    List<Guid?> GetAssetActiveServices(Guid assetUid);
    IList<ServiceLookupItem> GetAssetActiveServices(string serialNumber, string makeCode);
    IList<ServiceLookupItem> GetDeviceActiveServices(string serialNumber, DeviceTypeEnum deviceType);

    Credentials FindCredentialsForUrl(string url);
  }
}
