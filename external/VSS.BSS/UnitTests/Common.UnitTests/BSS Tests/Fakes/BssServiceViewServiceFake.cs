using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  public class BssServiceViewServiceFake : IBssServiceViewService
  {
    private readonly Tuple<IList<ServiceViewInfoDto>, IList<ServiceViewInfoDto>> _updatedServiceViews;
    private readonly Tuple<Service, IList<ServiceViewInfoDto>> _serviceServiceViews;
    private readonly IList<ServiceViewInfoDto> _serviceViewDtos;
    private bool _boolValue;
    private List<Service> _services;
    public bool WasExecuted = false;
    public bool HasActiveService = false;

    public BssServiceViewServiceFake(IList<ServiceViewInfoDto> serviceViewDtos)
    {
      _serviceViewDtos = serviceViewDtos;
    }

    public BssServiceViewServiceFake(bool value)
    {
      _boolValue = value;
    }

    public BssServiceViewServiceFake(List<Service> services)
    {
      _services = services;
    }

    public BssServiceViewServiceFake(Tuple<Service, IList<ServiceViewInfoDto>> serviceServiceViews)
    {
      _serviceServiceViews = serviceServiceViews;
    }

    public BssServiceViewServiceFake(Tuple<IList<ServiceViewInfoDto>, IList<ServiceViewInfoDto>> updatedServiceViews)
    {
      _updatedServiceViews = updatedServiceViews;
    }

    public BssServiceViewServiceFake()
    {
    }

    public IList<ServiceViewInfoDto> CreateRelationshipServiceViews(long parentCustomerId, long childCustomerId)
    {
      WasExecuted = true;
      return _serviceViewDtos;
    }

    public IList<ServiceViewInfoDto> TerminateRelationshipServiceViews(long parentCustomerId, long childCustomerId, DateTime terminationDate)
    {
      WasExecuted = true;
      return _serviceViewDtos;
    }

    public IList<ServiceViewInfoDto> TerminateServiceViewsForAsset(long assetId, DateTime endDateUTC)
    {
      WasExecuted = true;
      return _serviceViewDtos;
    }

    public IList<ServiceViewInfoDto> CreateServiceViewsForAsset(long assetId, DateTime? startDateUTC = null)
    {
      WasExecuted = true;
      return _serviceViewDtos;
    }

    public bool DeviceHasAnActiveService(long deviceId)
    {
      WasExecuted = true;
      return HasActiveService;
    }

    public bool IsDeviceTransferValid(long oldDeviceId, DeviceTypeEnum newDeviceType)
    {
      WasExecuted = true;
      return _boolValue;
    }

    public void ReleaseAssetFromStore(long deviceId)
    {
      WasExecuted = true;
    }

    public Tuple<IList<ServiceViewInfoDto>, IList<ServiceViewInfoDto>> SwapServiceViewsBetweenOldAndNewAsset(long oldAssetID, long newAssetID, DateTime actionUTC)
    {
      WasExecuted = true;
      return _updatedServiceViews;
    }

    public bool UpdateDeviceState(string gpsDeviceID, DeviceTypeEnum deviceType, long sequenceNumber, DeviceStateEnum deviceState)
    {
      WasExecuted = true;
      return _boolValue;
    }

    public bool ConfigureDeviceForActivatedServicePlans(long assetID, DateTime actionUTC, string gpsDeviceID, DeviceTypeEnum deviceType, ServiceTypeEnum serviceType)
    {
      WasExecuted = true;
      return _boolValue;
    }

    public bool DeviceSupportsService(ServiceTypeEnum serviceType, DeviceTypeEnum deviceType)
    {
      WasExecuted = true;
      return _boolValue;
    }

    public Tuple<Service, IList<ServiceViewInfoDto>> CreateServiceAndServiceViews(long deviceID, DeviceTypeEnum deviceType, string planLineID, DateTime activationDate, DateTime? ownerVisibilityDate, ServiceTypeEnum serviceType)
    {
      WasExecuted = true;
      return _serviceServiceViews;
    }

    public ServiceTypeEnum? GetServiceTypeByPartNumber(string partNumber)
    {
      throw new NotImplementedException();
    }

    public ServiceDto GetServiceByPlanLineID(string servicePlanLineID)
    {
      throw new NotImplementedException();
    }

    public Tuple<Service, IList<ServiceViewInfoDto>> TerminateServiceAndServiceViews(string bssPlanLineID, DateTime terminationDate)
    {
      WasExecuted = true;
      return _serviceServiceViews;
    }

    public bool DeActivateDevice(string ibkey)
    {
      WasExecuted = true;
      return _boolValue;
    }

    public bool ConfigureForCancelledServicePlans(long assetID, DateTime actionUTC, string gpsDeviceID, DeviceTypeEnum deviceType, ServiceTypeEnum serviceType, long serviceID)
    {
      WasExecuted = true;
      return _boolValue;
    }

    public IList<ServiceViewInfoDto> UpdateServiceAndServiceViews(long assetID, string ownerBSSID, long serviceID, DateTime? OwnerVisibiltyDate, ServiceTypeEnum serviceType)
    {
      WasExecuted = true;
      return _serviceViewDtos;
    }

    public string DeviceHasSameActiveService(long deviceID, ServiceTypeEnum serviceType)
    {
      WasExecuted = true;
      return string.Empty;
    }

    public bool DeviceHasActiveCoreService(long deviceID)
    {
      WasExecuted = true;
      return _boolValue;
    }


    public List<Service> TransferServices(long oldDeviceID, long newDeviceID, DateTime actionUTC)
    {
      WasExecuted = true;
      return _services;
    }
  }
}