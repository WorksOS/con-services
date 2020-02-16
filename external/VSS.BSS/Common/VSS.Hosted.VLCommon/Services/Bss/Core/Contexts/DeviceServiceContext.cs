using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceServiceContext
  {
    public string IBKey { get; set; }
    public string PartNumber { get; set; }
    public string PlanLineID { get; set; }
    public DateTime? OwnerVisibilityDate { get; set; }
    public DateTime? ServiceTerminationDate { get; set; }
    public DateTime ActionUTC { get; set; }
    public long SequenceNumber { get; set; }

    public DeviceAssetDto ExistingDeviceAsset { get; set; }
    public ServiceDto ExistingService { get; set; }
    public ServiceTypeEnum? ServiceType { get; set; }

    public DeviceServiceContext()
    {
      ExistingDeviceAsset = new DeviceAssetDto();
      ExistingService = new ServiceDto();
    }

    public void MapServiceToExistingService(Service service)
    {
      ExistingService = new ServiceDto
      {
        ActivationKeyDate = service.ActivationKeyDate,
        CancellationKeyDate = service.CancellationKeyDate,
        OwnerVisibilityKeyDate = service.OwnerVisibilityKeyDate,
        PlanLineID = service.BSSLineID,
        ServiceID = service.ID,
        ServiceType = (ServiceTypeEnum)service.fk_ServiceTypeID,
        IBKey = ExistingDeviceAsset.IbKey,
        DeviceID = ExistingDeviceAsset.DeviceId,
        GPSDeviceID = ExistingDeviceAsset.GpsDeviceId
      };
    }

    /// <summary>
    /// Verifies if a duplicate service being added to a device while the same existing service is still active
    /// </summary>
    /// <returns></returns>
    public bool IsAddingDuplicateService()
    {
      return ExistingService.CancellationKeyDate > DateTime.UtcNow.KeyDate() && ExistingService.ServiceType == ServiceType;
    }

    public bool IsDeviceDeregistered()
    {
      return ExistingDeviceAsset.DeviceState == DeviceStateEnum.DeregisteredStore ||
             ExistingDeviceAsset.DeviceState == DeviceStateEnum.DeregisteredTechnician;
    }

    public bool IsCoreService()
    {
      return ServiceType == ServiceTypeEnum.Essentials || ServiceType == ServiceTypeEnum.ManualMaintenanceLog || ServiceType == ServiceTypeEnum.VisionLinkDaily ;
    }
  }

  public class ServiceDto
  {
    public long ServiceID { get; set; }
    public ServiceTypeEnum? ServiceType { get; set; }
    public long DeviceID { get; set; }
    public string IBKey { get; set; }
    public string PlanLineID { get; set; }
    public string GPSDeviceID { get; set; }
    public int ActivationKeyDate { get; set; }
    public int CancellationKeyDate { get; set; }
    public int? OwnerVisibilityKeyDate { get; set; }

    public bool ServiceExists { get { return ServiceID > 0; } }
    public bool NewService { get { return !ServiceExists; } }

    //NOTE: This property holds the existing service plan line ID of an asset/device combination, if any
    //and used during service plan activation to check if the same service is already active on the same asset/device combination
    public string DifferentServicePlanLineID { get; set; }
  }
}
