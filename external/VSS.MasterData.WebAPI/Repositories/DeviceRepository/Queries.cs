using System;
using VSS.MasterData.WebAPI.Repository.Device.ColumnName;

namespace VSS.MasterData.WebAPI.Repository.Device
{
	public static class Queries
	{
		public static readonly string ReadDeviceTypeQuery = $"select dt.TypeName,dt.DeviceTypeID,dt.DefaultValueJson,dt.fk_DeviceTypeFamilyID from md_device_DeviceType dt";

		public const string CHECK_EXISTING_DEVICE_QUERY = "select count(1) from md_device_Device where DeviceUID = {0}";

		public const string GET_DEVICE_QUERY = "select HEX(DeviceUID) DeviceUID from md_device_Device where SerialNumber = '{0}' and fk_DeviceTypeID = '{1}'";

		public const string CHECK_EXISTING_DEVICE_PROPERTIES_QUERY = @"SELECT 
																			DeviceUID,
																			SerialNumber,
																			fk_DeviceTypeID,
																			fk_DeviceStatusID,
																			DeregisteredUTC,
																			ModuleType,
																			MainboardSoftwareVersion,
																			FirmwarePartNumber,
																			GatewayFirmwarePartNumber,
																			DataLinkType,
																			CellModemIMEI,
																			DevicePartNumber,
																			CellularFirmwarePartnumber,
																			NetworkFirmwarePartnumber,
																			SatelliteFirmwarePartnumber
																		FROM
																			md_device_Device
																		WHERE
																			DeviceUID = {0}";

		public const string CHECK_EXISTING_OWNING_CUSTOMER_UID_QUERY = @"SELECT 
																			HEX(A.OwningCustomerUID) AS OwningCustomerUID
																		FROM
																			md_asset_AssetDevice AD
																				INNER JOIN
																			md_asset_Asset A ON AD.fk_Assetuid = A.Assetuid
																		WHERE
																			AD.fk_DeviceUID = {0}"; 

		public const string GET_DEVICE_TYPE_QUERY = "select dt.{0} from md_device_DeviceType dt where {1}={2}";

		public const string GET_DEVICE_PROPERTIES_V2_BY_DEVICE_UID = @"SELECT 
																				A.DeviceUID AS DeviceUID,
																				A.SerialNumber AS DeviceSerialNumber,
																				TypeName AS DeviceType,
																				fk_DeviceStatusID AS DeviceState,
																				DeregisteredUTC AS DeregisteredUTC,
																				ModuleType AS ModuleType,
																				DataLinkType AS DataLinkType,
																				fk_PersonalityTypeID AS PersonalityTypeId,
																				PersonalityDesc AS PersonalityDescription,
																				PersonalityValue AS PersonalityValue
																			FROM
																				md_device_Device A
																					LEFT JOIN
																				md_device_DevicePersonality B ON A.DeviceUID = B.fk_DeviceUID
																					INNER JOIN
																				md_device_DeviceType C ON A.fk_DeviceTypeID = C.DeviceTypeID
																			WHERE
																				A.DeviceUID = {0};";
		
		public const string GET_DEVICE_PROPERTIES_V2_BY_ASSET_UID = @"SELECT 
																C.AssetUID AS AssetUID,
																A.DeviceUID AS DeviceUID,
																A.SerialNumber AS DeviceSerialNumber,
																TypeName AS DeviceType,
																fk_DeviceStatusID AS DeviceState,
																DeregisteredUTC AS DeregisteredUTC,
																ModuleType AS ModuleType,
																DataLinkType AS DataLinkType,
																fk_PersonalityTypeID AS PersonalityTypeId,
																PersonalityDesc AS PersonalityDescription,
																PersonalityValue AS PersonalityValue
															FROM
																md_asset_Asset C
																	INNER JOIN
																md_asset_AssetDevice AD ON C.AssetUID = AD.fk_AssetUID
																	INNER JOIN
																md_device_Device A ON AD.fk_DeviceUID = A.DeviceUID
																	LEFT JOIN
																md_device_DevicePersonality B ON A.DeviceUID = B.fk_DeviceUID
																	INNER JOIN
																md_device_DeviceType D ON A.fk_DeviceTypeID = D.DeviceTypeID
															WHERE
																C.AssetUID = {0}";

		public static readonly string GetOwningCustomerUIDQuery =
			$"SELECT A.AssetUID as AssetUID, A.OwningCustomerUID as OwningCustomerUID, A.AssetName, A.LegacyAssetID, A.Model, A.AssetTypeName as AssetType, A.IconKey, A.EquipmentVIN, A.ModelYear FROM md_asset_AssetDevice AD inner join md_asset_Asset A on AD.fk_Assetuid = Assetuid";

		public static readonly string GetDevicesByAssetUid = "select D.DeviceUID as DeviceUID, D.fk_DeviceStatusID as DeviceStatusID from md_asset_AssetDevice AD join md_device_Device D on AD.fk_DeviceUID=D.DeviceUID where AD.fk_AssetUID={0}";


		public static readonly string ValidateAuthorizedCustomerAssetQuery = @"SELECT 
																				count(1)
																			FROM
																				md_customer_CustomerUser CU
																					INNER JOIN
																				md_customer_CustomerAsset CA ON CA.fk_CustomerUID = CU.fk_CustomerUID
																			WHERE
																				CU.fk_UserUID = UNHEX('{0}')
																					AND CA.fk_AssetUID = UNHEX('{1}')";
		public static readonly string ValidateAuthorizedCustomerDeviceQuery = @"SELECT 
																				count(1)
																			FROM
																				md_customer_CustomerUser CU
																					INNER JOIN
																				md_customer_CustomerAsset CA ON CA.fk_CustomerUID = CU.fk_CustomerUID
																					INNER JOIN
																				md_asset_AssetDevice AD ON CA.fk_AssetUID = AD.fk_AssetUID
																			WHERE
																				CU.fk_UserUID = UNHEX('{0}')
																					AND AD.fk_DeviceUID = UNHEX('{1}')";

		public static readonly string GetAssetDevice = "select fk_AssetUID,fk_DeviceUID,RowUpdatedUTC,ActionUTC from md_asset_AssetDevice WHERE fk_AssetUID={0} AND fk_DeviceUID={1}";

	}
}
