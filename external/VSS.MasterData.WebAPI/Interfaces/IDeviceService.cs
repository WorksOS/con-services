using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.DbModel.Device;
using VSS.MasterData.WebAPI.ClientModel.Device;
using VSS.MasterData.WebAPI.KafkaModel.Device;
using VSS.MasterData.WebAPI.DbModel;

namespace VSS.MasterData.WebAPI.Interfaces.Device
{
	public interface IDeviceService
	{
		bool CreateDevice(CreateDeviceEvent device, DeviceStateEnum deviceState);
		bool UpdateDevice(UpdateDeviceEvent device, DeviceStateEnum deviceState);
		bool ValidateAuthorizedCustomerByAsset(Guid userGuid, Guid assetGuid);
		bool ValidateAuthorizedCustomerByDevice(Guid userGuid, Guid deviceGuid);
		bool UpdateDeviceProperties(UpdateDeviceProperties device, Guid deviceUid);
		bool AssociateAssetDevice(AssociateDeviceAssetEvent associateDeviceAsset);
		bool DissociateAssetDevice(DissociateDeviceAssetEvent dissociateDeviceAsset);
		bool CheckExistingDevice(Guid deviceGuid);
		List<Guid> GetDeviceDetailsBySerialNumberAndType(string deviceSerialNumber, string deviceType);
		IEnumerable<DevicePropertiesV2> GetDevicePropertiesV2ByDeviceGuid(Guid deviceGuid);
		IEnumerable<AssetDevicePropertiesV2> GetDevicePropertiesV2ByAssetGuid(Guid assetGuid);
		DeviceProperties GetExistingDeviceProperties(Guid deviceUID);
		List<Guid> GetCustomersForApplication(string applicationName);
		DeviceDto GetAssociatedDevicesByAsset(Guid assetGuid);
		DbAssetDevice GetAssetDevice(Guid assetGuid, Guid deviceGuid);
	}
}