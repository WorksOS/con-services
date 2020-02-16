using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  public class BssServiceViewServiceExceptionFake : IBssServiceViewService
  {
    private readonly Exception _exception;

    public BssServiceViewServiceExceptionFake(Exception exception)
    {
      _exception = exception;
    }

    public IList<ServiceViewInfoDto> CreateRelationshipServiceViews(long parentCustomerId, long childCustomerId)
    {
      throw _exception;
    }

    public IList<ServiceViewInfoDto> TerminateRelationshipServiceViews(long parentCustomerId, long childCustomerId, DateTime terminationDate)
    {
      throw _exception;
    }

    public IList<ServiceViewInfoDto> TerminateServiceViewsForAsset(long assetId, DateTime endDateUTC)
    {
      throw _exception;
    }

    public IList<ServiceViewInfoDto> CreateServiceViewsForAsset(long assetId, DateTime? startDateUTC = null)
    {
      throw _exception;
    }

    public bool IsEssentialsExistsForDevice(long deviceId)
    {
      throw _exception;
    }

    public bool DeviceHasAnActiveService(long deviceId)
    {
      throw _exception;
    }

    public bool IsDeviceTransferValid(long oldDeviceId, DeviceTypeEnum newDeviceType)
    {
      throw _exception;
    }

    public Tuple<IList<ServiceViewInfoDto>, IList<ServiceViewInfoDto>> SwapServiceViewsBetweenOldAndNewAsset(long oldAssetID, long newAssetID, DateTime actionUTC)
    {
      throw _exception;
    }

    public bool UpdateDeviceState(string gpsDeviceID, DeviceTypeEnum deviceType, long sequenceNumber, DeviceStateEnum deviceState)
    {
      throw _exception;
    }

    public bool ConfigureDeviceForActivatedServicePlans(long assetID, DateTime actionUTC, string gpsDeviceID, DeviceTypeEnum deviceType, ServiceTypeEnum serviceType)
    {
      throw _exception;
    }

    public VSS.Hosted.VLCommon.ServiceTypeEnum? GetServiceTypeByPartNumber(string partNumber)
    {
      throw new NotImplementedException();
    }

    public ServiceDto GetServiceByPlanLineID(string servicePlanLineID)
    {
      throw new NotImplementedException();
    }

    public bool DeviceSupportsService(VSS.Hosted.VLCommon.ServiceTypeEnum serviceType, VSS.Hosted.VLCommon.DeviceTypeEnum deviceType)
    {
      throw new NotImplementedException();
    }

    public Tuple<Service, IList<ServiceViewInfoDto>> CreateServiceAndServiceViews(long deviceID, DeviceTypeEnum deviceType, string planLineID, DateTime activationDate, DateTime? ownerVisibilityDate, ServiceTypeEnum serviceType)
    {
      throw _exception;
    }

    public Tuple<Service, IList<ServiceViewInfoDto>> TerminateServiceAndServiceViews(string bssPlanLineID, DateTime terminationDate)
    {
      throw _exception;
    }

    public bool DeActivateDevice(string ibkey)
    {
      throw _exception;
    }
    
    public bool ConfigureForCancelledServicePlans(long assetID, DateTime actionUTC, string gpsDeviceID, DeviceTypeEnum deviceType, ServiceTypeEnum serviceType, long serviceID)
    {
      throw _exception;
    }

    public IList<ServiceViewInfoDto> UpdateServiceAndServiceViews(long assetID, string ownerBSSID, long serviceID, DateTime? OwnerVisibiltyDate, ServiceTypeEnum serviceType)
    {
      throw _exception;
    }

    public string DeviceHasSameActiveService(long deviceID, ServiceTypeEnum serviceType)
    {
      throw new NotImplementedException();
    }

    public bool DeviceHasActiveCoreService(long deviceID)
    {
      throw new NotImplementedException();
    }

    public void ReleaseAssetFromStore(long deviceId)
    {
      throw new NotImplementedException();
    }


    public List<Service> TransferServices(long oldDeviceID, long newDeviceID, DateTime actionUTC)
    {
      throw _exception;
    }
  }
}