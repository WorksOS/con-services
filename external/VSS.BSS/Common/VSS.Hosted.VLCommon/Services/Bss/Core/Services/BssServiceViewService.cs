using System;
using System.Collections.Generic;
using System.Linq;

namespace VSS.Hosted.VLCommon.Bss
{
  public class BssServiceViewService : IBssServiceViewService
  {
    public IList<ServiceViewInfoDto> CreateRelationshipServiceViews(long parentCustomerId, long childCustomerId)
    {
      IList<ServiceView> serviceViews = API.ServiceView.CreateRelationshipServiceViews(parentCustomerId, childCustomerId);
      return GetServiceViewInfoDto(serviceViews);
    }

    public IList<ServiceViewInfoDto> TerminateRelationshipServiceViews(long parentCustomerId, long childCustomerId, DateTime terminationDate)
    {
      IList<ServiceView> serviceViews = API.ServiceView.TerminateRelationshipServiceViews(parentCustomerId, childCustomerId, terminationDate);
      return GetServiceViewInfoDto(serviceViews);
    }

    //public Tuple<IList<ServiceViewInfoDto>, IList<ServiceViewInfoDto>> TransferDeviceServiceViews(long deviceId, long oldOwnerId, long newOwnerId, DateTime transferDate)
    //{
    //  IList<ServiceView> terminatedViews = API.ServiceView.MATTTerminateDeviceServiceViews(deviceId, transferDate);
    //  IList<ServiceView> createdViews = API.ServiceView.MATTCreateDeviceServiceViews(deviceId);

    //  //Tuple<IList<ServiceView>, IList<ServiceView>> updatedViews = API.ServiceView.TransferDeviceServiceViews(deviceId, oldOwnerId, newOwnerId, transferDate);

    //  return new Tuple<IList<ServiceViewInfoDto>, IList<ServiceViewInfoDto>>(
    //    GetServiceViewInfoDto(terminatedViews), 
    //    GetServiceViewInfoDto(createdViews));

    //}

    //public Tuple<IList<ServiceViewInfoDto>, IList<ServiceViewInfoDto>> TransferDeviceServiceViews(long oldAssetId, long newAssetId, DateTime transferDate)
    //{
    //  Tuple<IList<ServiceView>, IList<ServiceView>> updatedViews = API.ServiceView.TransferDeviceServiceViews(oldAssetId, newAssetId, transferDate);

    //  if (updatedViews == null)
    //    return new Tuple<IList<ServiceViewInfoDto>, IList<ServiceViewInfoDto>>(new List<ServiceViewInfoDto>(), new List<ServiceViewInfoDto>());

    //  List<ServiceViewInfoDto> terminatedViews = GetServiceViewInfoDto(updatedViews.Item1);
    //  List<ServiceViewInfoDto> createdViews = GetServiceViewInfoDto(updatedViews.Item2);

    //  return new Tuple<IList<ServiceViewInfoDto>, IList<ServiceViewInfoDto>>(terminatedViews, createdViews);
    //}

    public IList<ServiceViewInfoDto> CreateServiceViewsForAsset(long assetId, DateTime? startDate = null)
    {
      IList<ServiceView> createdViews = API.ServiceView.CreateAssetServiceViews(assetId, startDate);

      return GetServiceViewInfoDto(createdViews);
    }

    public IList<ServiceViewInfoDto> TerminateServiceViewsForAsset(long assetId, DateTime terminationDate)
    {
      IList<ServiceView> terminatedViews = API.ServiceView.TerminateAssetServiceViews(assetId, terminationDate);

      return GetServiceViewInfoDto(terminatedViews);
    }

    #region DeviceReplacement

    /// <summary>
    /// Transfer services from one device to another device
    /// </summary>
    /// <param name="oldDeviceID">ID of the device from which the services needs to be transferred</param>
    /// <param name="newDeviceID">ID of the new Device for which the services getting transferred to</param>
    /// <param name="actionUTC">Date since the transfer is effective</param>
    /// <returns>List of services which are transferred from old device to new device</returns>
    public List<Service> TransferServices(long oldDeviceID, long newDeviceID, DateTime actionUTC)
    {
      return API.ServiceView.TransferServices(Data.Context.OP, oldDeviceID, newDeviceID, actionUTC);
    }

    /// <summary>
    /// Move Service Views from old asset and re-create the same service views for the new asset
    /// Actions:
    ///   1. Terminate services views on old asset 
    ///   2. Create service views on new asset
    /// </summary>
    /// <param name="oldAssetID">ID of the old asset for which the service views needs to be terminated</param>
    /// <param name="newAssetID">ID of the new asset for which the service views needs to be re-created</param>
    /// <param name="actionUTC">Date since the service view termination/re-creation start</param>
    /// <returns>A tuple containing the service views terminated for old asset and service views created for new asset</returns>
    public Tuple<IList<ServiceViewInfoDto>, IList<ServiceViewInfoDto>> SwapServiceViewsBetweenOldAndNewAsset(long oldAssetID, long newAssetID, DateTime actionUTC)
    {
      var success = API.ServiceView.SwapServiceViewsBetweenOldAndNewAsset(Data.Context.OP, oldAssetID, newAssetID, actionUTC);
      if (success)
        return null;

      var list = new List<long> { oldAssetID, newAssetID };

      var svs = (from sv in Data.Context.OP.ServiceViewReadOnly
                 where list.Contains(sv.fk_AssetID)
                 select sv).ToList();

      return new Tuple<IList<ServiceViewInfoDto>, IList<ServiceViewInfoDto>>(
        GetServiceViewInfoDto(svs.Where(t => t.fk_AssetID == oldAssetID).ToList()),
        GetServiceViewInfoDto(svs.Where(t => t.fk_AssetID == newAssetID).ToList()));
    }

    /// <summary>
    /// Verifys if device has any active service plans
    /// </summary>
    /// <param name="deviceID">ID of the device for which the active service plans needs to be verified</param>
    /// <returns>A boolean value indicating the status. A true value indicates that the device has active service and false indicates not</returns>
    public bool DeviceHasAnActiveService(long deviceId)
    {
      int keyDateNow = DateTime.UtcNow.KeyDate();
      return (from s in Data.Context.OP.ServiceReadOnly
              where s.fk_DeviceID == deviceId
              && s.ActivationKeyDate <= keyDateNow
              && s.CancellationKeyDate == DotNetExtensions.NullKeyDate
              select s).Any();
    }


        /// <summary>
        /// Verify the device have existing CATDailyInVisionLink service plan
        /// </summary>
        /// <param name="deviceId">ID of the device for which the active service plans needs to be verified</param>
        /// <returns>A boolean value indicating the status. A true value indicates that the device has active Vision link CAT Daily service and false indicates not</returns>
        public bool IsActiveVisionLinkDailyServiceExist(long deviceId)
        {
            int keyDateNow = DateTime.UtcNow.KeyDate();
            return (from s in Data.Context.OP.ServiceReadOnly
                    where s.fk_DeviceID == deviceId
                    && s.fk_ServiceTypeID == (int)ServiceTypeEnum.VisionLinkDaily
                    && s.ActivationKeyDate <= keyDateNow
                    && s.CancellationKeyDate == DotNetExtensions.NullKeyDate
                    select s).Any();
        }


        /// <summary>
        /// Verifys if device has active CAT Daily service plan
        /// </summary>
        /// <param name="deviceID">ID of the device for which the active service plans needs to be verified</param>
        /// <returns>A boolean value indicating the status. A true value indicates that the device has active CAT Daily service and false indicates not</returns>
        public bool IsActiveCATDailyServiceExist(long deviceId)
        {
            int keyDateNow = DateTime.UtcNow.KeyDate();
            return (from s in Data.Context.OP.ServiceReadOnly
                    where s.fk_DeviceID == deviceId
                    && s.fk_ServiceTypeID == (int)ServiceTypeEnum.CATDaily
                    && s.ActivationKeyDate <= keyDateNow
                    && s.CancellationKeyDate == DotNetExtensions.NullKeyDate
                    select s).Any();
        }

        /// <summary>
        /// Verifies that the device transfer is valid or not
        /// </summary>
        /// <param name="oldDeviceId">Old Device ID which is being replaced</param>
        /// <param name="newDeviceType">new Device Type which is replacing the old device</param>
        /// <returns>A boolean value indicating the validity of the transfer. A true value indicates valid and false indicates invalid</returns>
        public bool IsDeviceTransferValid(long oldDeviceId, DeviceTypeEnum newDeviceType)
    {
      return API.ServiceView.IsDeviceTransferValid(Data.Context.OP, oldDeviceId, newDeviceType);
    }

    #endregion

    #region ServicePlan

    /// <summary>
    /// Create Service and service views for the service
    /// </summary>
    /// <param name="deviceID">Device ID for which the service is being added</param>
    /// <param name="deviceType">Type of the Device like PL321/Series 522 etc.</param>
    /// <param name="planLineID">Unique ID of the Plan that is being added</param>
    /// <param name="activationDate">The date since the service plan becomes active</param>
    /// <param name="ownerVisibilityDate">The date since the customer gets visibility to the Device/Asset</param>
    /// <param name="serviceType">Type of service that is being added</param>
    /// <returns>A tuple containing the service and the corresponding service views which are created for the hierarchy</returns>
    public Tuple<Service, IList<ServiceViewInfoDto>> CreateServiceAndServiceViews(long deviceID, DeviceTypeEnum deviceType, string planLineID, DateTime activationDate, DateTime? ownerVisibilityDate, ServiceTypeEnum serviceType)
    {
      var result = API.ServiceView.CreateServiceAndServiceViews(Data.Context.OP, deviceID, deviceType, planLineID, activationDate, ownerVisibilityDate, serviceType);
      UpdateAssetStore(deviceId:deviceID);
      return new Tuple<Service, IList<ServiceViewInfoDto>>(result.Item1, GetServiceViewInfoDto(result.Item2));
    }

    private void UpdateAssetStore(long? deviceId = null, long? assetID = null)
    {
      List<Param> modifiedProperties = new List<Param>();
      Asset asset = null;
      if (deviceId.HasValue && deviceId != 0)
      {
        asset = (from a in Data.Context.OP.Asset where a.fk_DeviceID == deviceId.Value && a.fk_StoreID == (int)StoreEnum.NoStore select a).FirstOrDefault();
        if (asset != null)
        {    
          modifiedProperties.Add(new Param { Name = "fk_StoreID", Value = (int)StoreEnum.Trimble });
        }
      }
      else if (assetID.HasValue && assetID != 0)
      {
        asset = (from a in Data.Context.OP.Asset where a.AssetID == assetID.Value && a.fk_StoreID == (int)StoreEnum.NoStore select a).FirstOrDefault();
        if (asset != null)
          modifiedProperties.Add(new Param { Name = "fk_StoreID", Value = (int)StoreEnum.Trimble });
      }

      if (modifiedProperties.Count > 0 && asset != null)
        API.Update<Asset>(Data.Context.OP, asset, modifiedProperties);
    }

    /// <summary>
    /// termiante service and all service views related to that service
    /// Invoked by BSS ServicePlan Cancelled action
    /// </summary>
    /// <param name="bssPlanLineID">BSS Line ID of the service which needs to be terminated</param>
    /// <param name="terminationUTC">Date on which this service needs to be terminated</param>
    /// <returns>A tuple containing the service and the corresponding service views which are terminated for the hierarchy</returns>
    public Tuple<Service, IList<ServiceViewInfoDto>> TerminateServiceAndServiceViews(string bssPlanLineID, DateTime terminationDate)
    {
      var success = API.ServiceView.TerminateServiceAndServiceViews(Data.Context.OP, bssPlanLineID, terminationDate);

      if (!success)
        return null;

      //returu the service and the service views which are terminated
      var service = Data.Context.OP.ServiceReadOnly.Where(t => t.BSSLineID == bssPlanLineID).SingleOrDefault();
      return new Tuple<Service, IList<ServiceViewInfoDto>>(service, GetServiceViewsByServiceID(service.ID));
    }

    public void ReleaseAssetFromStore(long deviceId)
    {
      API.ServiceView.ReleaseAsset(Data.Context.OP, deviceId);
    }

    /// <summary>
    /// Create/Update/Terminate service views for the customer hierarchy to which the device/asset is associated.
    /// </summary>
    /// <param name="assetID">Asset to which the service view is associated with</param>
    /// <param name="ownerBSSID">Customer BSSID to which the asset is associated</param>
    /// <param name="serviceID">The service that is being updated/termianted</param>
    /// <param name="OwnerVisibilityDate">The date since the customer gets visibility to the Device/Asset</param>
    /// <param name="serviceType">The type of the service that is being updated</param>
    /// <returns>A list of service views which are created/updated/termianted for the given combination fo Customer/Service/Asset</returns>
    public IList<ServiceViewInfoDto> UpdateServiceAndServiceViews(long assetID, string ownerBSSID, long serviceID, DateTime? OwnerVisibilityDate, ServiceTypeEnum serviceType)
    {
      var result = API.ServiceView.UpdateServiceAndServiceViews(Data.Context.OP, OwnerVisibilityDate, ownerBSSID, serviceID, assetID, serviceType);
      UpdateAssetStore(assetID: assetID);
      return GetServiceViewInfoDto(result);
    }

    /// <summary>
    /// Reconfigure the device reporting intervals based on the service plas activated
    /// </summary>
    /// <param name="assetID">Asset for which the device is associated</param>
    /// <param name="actionUTC">The date since these changes will take effect</param>
    /// <param name="gpsDeviceID">Gps Device ID of the device</param>
    /// <param name="deviceType">Type of the device</param>
    /// <param name="serviceType">Type of the service</param>
    /// <returns>A boolean value indicating the status of the configuration. A true value indicates success and false indicates failure</returns>
    public bool ConfigureDeviceForActivatedServicePlans(long assetID, DateTime actionUTC, string gpsDeviceID, DeviceTypeEnum deviceType, ServiceTypeEnum serviceType)
    {
      return API.ServiceView.ConfigureDeviceForActivatedServicePlans(Data.Context.OP, assetID, actionUTC, gpsDeviceID, deviceType, serviceType, true, null);
    }

    /// <summary>
    /// Deactivetes device for which the ibkey is given
    /// </summary>
    /// <param name="ibkey">ibkey of the device</param>
    /// <returns>A boolean value indicating the status of the deactivation. A true value indicates success and false indicates failure</returns>
    public bool DeActivateDevice(string ibkey)
    {
      return API.Device.DeActivateDevice(Data.Context.OP, ibkey, DeviceStateEnum.Provisioned);
    }

    /// <summary>
    /// Reconfigure the device reporting intervals based on the service plas activated
    /// </summary>
    /// <param name="assetID">Asset for which the device is associated</param>
    /// <param name="actionUTC">The date since these changes will take effect</param>
    /// <param name="gpsDeviceID">Gps Device ID of the device</param>
    /// <param name="deviceType">Type of the device</param>
    /// <param name="serviceType">Type of the service</param>
    /// <param name="serviceID">ID of the Service which is being terminated</param>
    /// <returns>A boolean value indicating the status of the configuration. A true value indicates success and false indicates failure</returns>
    public bool ConfigureForCancelledServicePlans(long assetID, DateTime actionUTC, string gpsDeviceID, DeviceTypeEnum deviceType, ServiceTypeEnum serviceType, long serviceID)
    {
      return API.ServiceView.ConfigureDeviceForActivatedServicePlans(Data.Context.OP, assetID, actionUTC, gpsDeviceID, deviceType, serviceType, false, serviceID);
    }

    /// <summary>
    /// Check if the device support the service type that is being activated
    /// </summary>
    /// <param name="serviceType">Service Type that is being activated</param>
    /// <param name="deviceType">Device Type for which the service is being associated</param>
    /// <returns>A boolean value indicating the support status. A true value indicates that it supports the service type and false indicates not</returns>
    public bool DeviceSupportsService(ServiceTypeEnum serviceType, DeviceTypeEnum deviceType)
    {
      return API.ServiceView.DeviceSupportsService(Data.Context.OP, serviceType, deviceType);
    }

    public string DeviceHasSameActiveService(long deviceID, ServiceTypeEnum serviceType)
    {
      var keydate = DateTime.UtcNow.KeyDate();

      return (from s in Data.Context.OP.ServiceReadOnly
              join d in Data.Context.OP.DeviceReadOnly on s.fk_DeviceID equals d.ID
              where s.fk_ServiceTypeID == (int)serviceType &&
                    d.ID == deviceID &&
                    s.ActivationKeyDate <= keydate &&
                    s.CancellationKeyDate > keydate
              select s.BSSLineID).SingleOrDefault();
    }

    /// <summary>
    /// Return the Service for the given Service Plan Line ID
    /// </summary>
    /// <param name="servicePlanLineID">Service Plan Line ID for which the service needs to be found</param>
    /// <returns>Return a Service Object which contain the details about the service</returns>
    public ServiceDto GetServiceByPlanLineID(string servicePlanLineID)
    {
      var service = (from s in Data.Context.OP.ServiceReadOnly
                     join d in Data.Context.OP.DeviceReadOnly on s.fk_DeviceID equals d.ID
                     where s.BSSLineID.Equals(servicePlanLineID, StringComparison.OrdinalIgnoreCase)
                     select new
                     {
                       ServiceID = s.ID,
                       DeviceID = s.fk_DeviceID,
                       IBKey = d.IBKey,
                       GPSDeviceID = d.GpsDeviceID,
                       PlanLineID = s.BSSLineID,
                       ServiceType = s.fk_ServiceTypeID,
                       ActivationKeyDate = s.ActivationKeyDate,
                       CancellationKeyDate = s.CancellationKeyDate,
                       OwnerVisibilityKeyDate = s.OwnerVisibilityKeyDate,
                     }).SingleOrDefault();

      if (service == null)
        return null;

      return new ServiceDto
      {
        ServiceID = service.ServiceID,
        PlanLineID = service.PlanLineID,
        ServiceType = (ServiceTypeEnum?)service.ServiceType,
        ActivationKeyDate = service.ActivationKeyDate,
        CancellationKeyDate = service.CancellationKeyDate,
        OwnerVisibilityKeyDate = service.OwnerVisibilityKeyDate,
        IBKey = service.IBKey,
        DeviceID = service.DeviceID,
        GPSDeviceID = service.GPSDeviceID
      };
    }

    /// <summary>
    /// Return the service Type for the given part number/plan name
    /// </summary>
    /// <param name="partNumber">part number/plan name for which the service type needs to be found</param>
    /// <returns>Return valid service type if found. Null/Unknow if no service type found</returns>
    public ServiceTypeEnum? GetServiceTypeByPartNumber(string partNumber)
    {
      var servicePartNumber = (from st in Data.Context.OP.ServiceTypeReadOnly
                               where st.BSSPartNumber.Equals(partNumber, StringComparison.OrdinalIgnoreCase)
                               select new { st.ID }).FirstOrDefault();

      if (servicePartNumber == null)
        return ServiceTypeEnum.Unknown;

      return (ServiceTypeEnum)servicePartNumber.ID;
    }

    /// <summary>
    /// Verifys if device has any active service plans
    /// </summary>
    /// <param name="deviceID">ID of the device for which the active service plans needs to be verified</param>
    /// <param name="serviceID">ID of the service which should not be considered while checking</param>
    /// <returns>A boolean value indicating the status. A true value indicates that the device has active service and false indicates not</returns>
    public bool DeviceHasActiveCoreService(long deviceID)
    {
      int keyDateNow = DateTime.UtcNow.KeyDate();
      return (from s in Data.Context.OP.ServiceReadOnly
              join st in Data.Context.OP.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
              where s.fk_DeviceID == deviceID
              && s.ActivationKeyDate <= keyDateNow
              && s.CancellationKeyDate == DotNetExtensions.NullKeyDate
              && st.IsCore == true
              select s).Any();
    }

    #endregion

    #region Private Methods

    private List<ServiceViewInfoDto> GetServiceViewInfoDto(IList<ServiceView> serviceViews)
    {
      if(serviceViews == null || serviceViews.Count() == 0)
        return new List<ServiceViewInfoDto>();

      if(serviceViews.Count() > 30)
        return new List<ServiceViewInfoDto>{new ServiceViewInfoDto{ServiceTypeName = string.Format("NOTICE: Batch ServiceView operation modified {0} records.", serviceViews.Count)}};

      long[] serviceViewIds = serviceViews.Select(x => x.ID).ToArray();

      var serviceViewDtos = (from serviceView in Data.Context.OP.ServiceViewReadOnly
                             join service in Data.Context.OP.ServiceReadOnly on
                               serviceView.fk_ServiceID equals service.ID
                             join serviceType in Data.Context.OP.ServiceTypeReadOnly on
                               service.fk_ServiceTypeID equals serviceType.ID
                             join customer in Data.Context.OP.CustomerReadOnly on
                               serviceView.fk_CustomerID equals customer.ID
                             join asset in Data.Context.OP.AssetReadOnly on
                               serviceView.fk_AssetID equals asset.AssetID
                             where serviceViewIds.Contains(serviceView.ID)
                             select new ServiceViewInfoDto
                             {
                               ServiceViewId = serviceView.ID,
                               ServiceTypeName = serviceType.Name,
                               CustomerId = serviceView.fk_CustomerID,
                               CustomerName = customer.Name,
                               AssetId = serviceView.fk_ServiceID,
                               StartDateKey = serviceView.StartKeyDate,
                               EndDateKey = serviceView.EndKeyDate
                             }).ToList();

      return serviceViewDtos;
    }

    private IList<ServiceViewInfoDto> GetServiceViewsByServiceID(long serviceID)
    {
      var serviceViewDtos = (from serviceView in Data.Context.OP.ServiceViewReadOnly
                             join service in Data.Context.OP.ServiceReadOnly on
                               serviceView.fk_ServiceID equals service.ID
                             join serviceType in Data.Context.OP.ServiceTypeReadOnly on
                               service.fk_ServiceTypeID equals serviceType.ID
                             join customer in Data.Context.OP.CustomerReadOnly on
                               serviceView.fk_CustomerID equals customer.ID
                             join asset in Data.Context.OP.AssetReadOnly on
                               serviceView.fk_AssetID equals asset.AssetID
                             where serviceView.fk_ServiceID == serviceID
                             select new ServiceViewInfoDto
                             {
                               ServiceViewId = serviceView.ID,
                               ServiceTypeName = serviceType.Name,
                               CustomerId = customer.ID,
                               CustomerName = customer.Name,
                               AssetId = asset.AssetID,
                               AssetSerialNumber = asset.SerialNumberVIN,
                               StartDateKey = serviceView.StartKeyDate,
                               EndDateKey = serviceView.EndKeyDate
                             }).ToList();

      return serviceViewDtos;
    }

   

    #endregion

  }
}
