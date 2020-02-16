using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon;


namespace VSS.Hosted.VLCommon.Bss
{
  public class MapServicePlanToDeviceServiceContext : Activity
  {
    public override ActivityResult Execute(Inputs inputs)
    {
      var message = inputs.Get<ServicePlan>();
      var context = inputs.GetOrNew<DeviceServiceContext>();

      AddSummary("Mapping message to DeviceServiceContext");

      MapMessageToDeviceServiceContext(message, context);

      AddSummary("Mapping existing device to DeviceServiceContext");

      MapExistingDeviceToDeviceAssetContext(message.IBKey, context);

      AddSummary("Mapping existing service to DeviceServiceContext");

      MapExistingServiceToServiceContext(message.ServicePlanlineID, context);

      AddSummary("Mapping device to customer");

      MapMessageToCustomerContex(context);

      return Success();
    }

    private void MapMessageToCustomerContex(DeviceServiceContext context)
    {
      var customer = Services.Customers().GetCustomerByBssId(context.ExistingDeviceAsset.OwnerBSSID);

      //check if the owner bssid of the device is pointing to a invalid customer
      if (customer == null)
      {
        context.ExistingDeviceAsset.OwnerBSSID = null;
        AddSummary(string.Format(MapAccountHierarchyToCustomerContext.BSSID_NOT_FOUND_MESSAGE, context.ExistingDeviceAsset.OwnerBSSID));
      }
    }

    private void MapExistingServiceToServiceContext(string planLineID, DeviceServiceContext context)
    {
      var service = Services.ServiceViews().GetServiceByPlanLineID(planLineID);

      //this is to check to see if the device/asset already have any same active service plan. 
      //If yes, the DifferentServicePlanLineID will be set to the plan line ID otherwise it is empty/null
      context.ExistingService.DifferentServicePlanLineID = Services.ServiceViews().DeviceHasSameActiveService(context.ExistingDeviceAsset.DeviceId, context.ServiceType.Value);

      if (service == null || !service.ServiceExists)
      {
        AddSummary("Service not found for Plan Line ID: {0}", planLineID);
        return;
      }

      AddSummary("Mapped Service for Plan Line ID: {0} to Device Service Context.", planLineID);
      context.ExistingService = service;

      AddSummary(context.ExistingService.PropertiesAndValues().ToNewLineTabbedString());

    }

    private void MapExistingDeviceToDeviceAssetContext(string ibkey, DeviceServiceContext context)
    {
      var existingDevice = Services.Devices().GetDeviceByIbKey(ibkey);

      if (existingDevice == null || !existingDevice.Exists)
      {
        AddSummary("Device not found for IBKey: {0}", ibkey);
        return;
      }

      AddSummary("Mapped device for IBKey: {0} to Device Service Context.", ibkey);

      context.ExistingDeviceAsset = new DeviceAssetDto
      {
        AssetId = existingDevice.AssetId,
        AssetStore = (StoreEnum)existingDevice.Asset.StoreID,
        AssetSerialNumber = existingDevice.Asset.SerialNumber,
        AssetMakeCode = existingDevice.Asset.MakeCode,
        DeviceId = existingDevice.Id,
        GpsDeviceId = existingDevice.GpsDeviceId,
        IbKey = existingDevice.IbKey,
        Type = existingDevice.Type,
        OwnerBSSID = existingDevice.Owner.BssId,
        DeviceState = existingDevice.DeviceState
      };

      AddSummary(context.ExistingDeviceAsset.PropertiesAndValues().ToNewLineTabbedString());
    }

    private void MapMessageToDeviceServiceContext(ServicePlan message, DeviceServiceContext context)
    {
      context.IBKey = message.IBKey;
      context.PartNumber = message.ServicePlanName;
      context.PlanLineID = message.ServicePlanlineID;
      context.OwnerVisibilityDate = message.OwnerVisibilityDate.IsNotDefined() ? (DateTime?)null : Convert.ToDateTime(message.OwnerVisibilityDate);
      context.ServiceTerminationDate = message.ServiceTerminationDate.IsNotDefined() ? (DateTime?)null : Convert.ToDateTime(message.ServiceTerminationDate);
      context.ActionUTC = Convert.ToDateTime(message.ActionUTC);
      context.SequenceNumber = message.SequenceNumber;

      var serviceType = Services.ServiceViews().GetServiceTypeByPartNumber(message.ServicePlanName);
      context.ServiceType = serviceType;

      if (!serviceType.HasValue || serviceType == null || serviceType == ServiceTypeEnum.Unknown)
      {
        AddSummary("ServiceType not found for ServicePlanName: {0}", message.ServicePlanName);
      }
      else
      {
        AddSummary("Mapped ServicePlanName: {0} to context", serviceType);
      }
      AddSummary("IBKey: {0}", context.IBKey);
      AddSummary("Type: {0}", context.ServiceType);
      AddSummary("PartNumber: {0}", context.PartNumber);
      AddSummary("PlanLineID: {0}", context.PlanLineID);
      AddSummary("ActionUTC: {0}", context.ActionUTC);
      AddSummary("OwnerVisibilityDate: {0}", context.OwnerVisibilityDate);
      AddSummary("ServiceTerminationDate: {0}", context.ServiceTerminationDate);
    }
  }
}
