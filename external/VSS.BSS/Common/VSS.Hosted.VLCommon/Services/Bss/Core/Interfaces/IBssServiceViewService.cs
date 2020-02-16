using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public interface IBssServiceViewService
  {
    IList<ServiceViewInfoDto> CreateRelationshipServiceViews(long parentCustomerId, long childCustomerId);
    IList<ServiceViewInfoDto> TerminateRelationshipServiceViews(long parentCustomerId, long childCustomerId, DateTime terminationDate);
    //Tuple<IList<ServiceViewInfoDto>, IList<ServiceViewInfoDto>> TransferDeviceServiceViews(long deviceId, long oldOwnerId, long newOwnerId, DateTime transferDate);
    IList<ServiceViewInfoDto> TerminateServiceViewsForAsset(long assetId, DateTime endDate);
    IList<ServiceViewInfoDto> CreateServiceViewsForAsset(long assetId, DateTime? startDate = null);

    ServiceTypeEnum? GetServiceTypeByPartNumber(string partNumber);
    ServiceDto GetServiceByPlanLineID(string servicePlanLineID);
    bool DeviceSupportsService(ServiceTypeEnum serviceType, DeviceTypeEnum deviceType);
    bool IsDeviceTransferValid(long oldDeviceId, DeviceTypeEnum newDeviceType);
    bool DeviceHasAnActiveService(long deviceId);
    bool IsActiveCATDailyServiceExist(long deviceId);
    bool IsActiveVisionLinkDailyServiceExist(long deviceId);
    bool DeviceHasActiveCoreService(long deviceID);
    string DeviceHasSameActiveService(long deviceID, ServiceTypeEnum serviceType);

    bool ConfigureDeviceForActivatedServicePlans(long assetID, DateTime actionUTC, string gpsDeviceID, DeviceTypeEnum deviceType, ServiceTypeEnum serviceType);
    Tuple<Service, IList<ServiceViewInfoDto>> CreateServiceAndServiceViews(long deviceID, DeviceTypeEnum deviceType, string planLineID, DateTime activationDate, DateTime? ownerVisibilityDate, ServiceTypeEnum serviceType);
    Tuple<Service, IList<ServiceViewInfoDto>> TerminateServiceAndServiceViews(string bssPlanLineID, DateTime terminationDate);
    IList<ServiceViewInfoDto> UpdateServiceAndServiceViews(long assetID, string ownerBSSID, long serviceID, DateTime? OwnerVisibilityDate, ServiceTypeEnum serviceType);
    bool ConfigureForCancelledServicePlans(long assetID, DateTime actionUTC, string gpsDeviceID, DeviceTypeEnum deviceType, ServiceTypeEnum serviceType, long serviceID);
    bool DeActivateDevice(string ibkey);
    List<Service> TransferServices(long oldDeviceID, long newDeviceID, DateTime actionUTC);
    Tuple<IList<ServiceViewInfoDto>, IList<ServiceViewInfoDto>> SwapServiceViewsBetweenOldAndNewAsset(long oldAssetID, long newAssetID, DateTime actionUTC);
    void ReleaseAssetFromStore(long deviceId);

    
  }
}
