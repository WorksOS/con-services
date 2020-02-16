using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Models;

namespace VSS.Hosted.VLCommon
{
  public interface IServiceViewAPI
  {
    // Basic Service Processing Functions
    Service CreateService(INH_OP opContext, long deviceID, string bssPlanLineID, DateTime activationDate, ServiceTypeEnum serviceType);

    bool TerminateService(INH_OP opContext, string bssPlanlineID, DateTime terminationUTC);
    
    Tuple<Service, IList<ServiceView>> CreateServiceAndServiceViews(INH_OP opContext, long deviceID, DeviceTypeEnum deviceType, string bssPlanLineID, DateTime activationDate, DateTime? ownerVisibilityDate, ServiceTypeEnum serviceType);
    bool TerminateServiceAndServiceViews(INH_OP opContext, string bssPlanLineID, DateTime terminationUTC);
    List<ServiceView> UpdateServiceAndServiceViews(INH_OP opContext, DateTime? ownerVisibilityDate, string ownerBSSID, long serviceID, long assetID, ServiceTypeEnum serviceType);

    List<Service> TransferServices(INH_OP opContext, long oldDeviceID, long newDeviceID, DateTime actionUTC);
    bool SwapServiceViewsBetweenOldAndNewAsset(INH_OP opContext, long oldAssetID, long newAssetID, DateTime actionUTC);
    bool ConfigureDeviceForActivatedServicePlans(INH_OP opContext, long assetID, DateTime actionUTC, string gpsDeviceID, DeviceTypeEnum deviceType, ServiceTypeEnum serviceType, bool isAdded, long? serviceID);

	  ServiceView CreateServiceView(INH_OP opContext, long customerID, long assetID, Guid assetGuid, long serviceID, int startKeyDate, RelationType relationType);

    bool DeviceHasActiveBSSService(INH_OP opContext, long deviceID, long serviceID);
    bool IsDeviceTransferValid(INH_OP opContext, long oldDeviceId, DeviceTypeEnum newDeviceType);
    bool BssPlanLineIDExists(INH_OP opContext, string bssPlanLineID);
    bool DeviceSupportsService(INH_OP opContext, ServiceTypeEnum serviceType, DeviceTypeEnum deviceType);
    bool DeviceHasAnActiveService(INH_OP opContext, long deviceID);
    bool DeviceHasActiveCoreService(INH_OP opContext, long deviceID);
    bool DeviceHasActiveBSSService(INH_OP opContext, long deviceID);
    bool CanCreateServiceViewForOrganization(INH_OP ctx, long deviceOwnerCustomerId, long organizationId);

    AssetDeviceHistory CreateAssetDeviceHistory(INH_OP opContext, long assetId, long deviceId, string ownerBssId, DateTime startUtc);

    IList<ServiceView> CreateRelationshipServiceViews(long parentId, long childId);
    IList<ServiceView> TerminateRelationshipServiceViews(long parentId, long childId, DateTime endDateUtc);
    IList<ServiceView> CreateAssetServiceViews(long assetId, DateTime? startDateUtc = null);
    IList<ServiceView> TerminateAssetServiceViews(long assetId, DateTime terminationDate);
    IList<ServiceView> TerminateServiceViewAtBeginRepo(long assetID, long customerID, INH_OP ctx, int today, int yesterday, DateTime updateUTC);
    IList<ServiceView> CreateServiceViewAfterEndRepo(long assetID, long customerID, AssetReposessionHistory assetRepoStatus, INH_OP ctx, int today, int tomorrow, DateTime updateUTC);
    bool TerminateVisibility(long customerId, long assetId, long subscriptionId, DateTime endKeyDate,INH_OP opContext);
    bool ReleaseAsset(INH_OP opContext, long deviceId);
    void UpdateSubscription(INH_OP opContext, ServiceView sv, bool isStartDate = false, bool isEndDate = false);
    void CreateSubscription(INH_OP opContext, ServiceView sv);
    
  }
}
