using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KafkaModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VSS.MasterData.WebAPI.DbModel.Device;
using VSS.MasterData.WebAPI.ClientModel.Device;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.KafkaModel.Device;
using VSS.MasterData.WebAPI.Repository.Device.ColumnName;
using VSS.MasterData.WebAPI.Repository.Device.Helpers;
using VSS.MasterData.WebAPI.Transactions;
using DeviceDataModels = VSS.MasterData.WebAPI.DbModel.Device;
using VSS.MasterData.WebAPI.Interfaces.Device;
using Newtonsoft.Json;
using Asset = VSS.MasterData.WebAPI.DbModel.Asset;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.Interfaces;
using AutoMapper;

namespace VSS.MasterData.WebAPI.Repository.Device
{
	public class DeviceService : IDeviceService
	{
		#region Declarations
		private readonly ILogger logger;
		private readonly IMapper mapper;
		private readonly ITransactions transactions;
		private readonly IConfiguration configuration;
		private readonly IAssetServices assetServices;

		private readonly List<string> kafkaTopicNames;
		private readonly List<string> kafkaTopicNamesV2;
		private readonly string topicSuffix;
		private readonly Dictionary<string, DbDeviceType> deviceTypesCache = new Dictionary<string, DbDeviceType>(StringComparer.InvariantCultureIgnoreCase);
		private const string INVALID_STRING_VALUE = "$#$#$";
		private static readonly List<string> PLDeviceTypes = new List<string> { "pl121", "pl321" };
		#endregion Declarations
		#region Constructors
		public DeviceService(ILogger logger, IConfiguration configuration, ITransactions transactions, IAssetServices assetServices, IMapper mapper, IDeviceTypeService deviceTypeService)
		{
			this.logger = logger;
			this.configuration = configuration;
			this.transactions = transactions;
			this.assetServices = assetServices;
			this.mapper = mapper;
			topicSuffix = this.configuration["topicSuffix"];
			kafkaTopicNames = configuration["KafkaTopicNames"].Split(',').Select(x => x = x + topicSuffix).ToList();
			kafkaTopicNamesV2 = configuration["KafkaTopicNamesV2"].Split(',').Select(x => x = x + topicSuffix).ToList();
			deviceTypesCache = deviceTypeService.GetDeviceType();
		}
		#endregion Constructors
		#region CUD
		public bool CreateDevice(CreateDeviceEvent device, DeviceStateEnum deviceState)
		{
			DbDeviceType deviceType = deviceTypesCache.First(x => string.Equals(x.Key, device.DeviceType, StringComparison.InvariantCultureIgnoreCase)).Value;

			var currentUTC = DateTime.UtcNow;
			var devicePayload = new CreateDevicePayload
			{
				ActionUTC = currentUTC,
				CellModemIMEI = device.CellModemIMEI,
				CellularFirmwarePartnumber = device.CellularFirmwarePartnumber,
				DataLinkType = device.DataLinkType,
				DeviceState = deviceState.ToString(),
				DeregisteredUTC = device.DeregisteredUTC,
				DevicePartNumber = device.DevicePartNumber,
				DeviceSerialNumber = device.DeviceSerialNumber,
				DeviceType = device.DeviceType,
				DeviceUID = device.DeviceUID.Value,
				FirmwarePartNumber = device.FirmwarePartNumber,
				GatewayFirmwarePartNumber = device.GatewayFirmwarePartNumber,
				MainboardSoftwareVersion = device.MainboardSoftwareVersion,
				ModuleType = device.ModuleType,
				NetworkFirmwarePartnumber = device.NetworkFirmwarePartnumber,
				RadioFirmwarePartNumber = device.RadioFirmwarePartNumber,
				ReceivedUTC = device.ReceivedUTC,
				SatelliteFirmwarePartnumber = device.SatelliteFirmwarePartnumber
			};

			var devicePersonality = GetDevicePersonalities(devicePayload, null, deviceType);
			devicePayload =SetNullIfPropertyEmpty(devicePayload);
			//DeviceSerialNumber will be persisited as NULL in masterdata and send it as Empty to Kafka
			devicePayload.DeviceSerialNumber = string.IsNullOrEmpty(devicePayload.DeviceSerialNumber) ? string.Empty : devicePayload.DeviceSerialNumber;
			var deviceModel = new DbDevice
			{
				SerialNumber = devicePayload.DeviceSerialNumber,
				DeregisteredUTC = devicePayload.DeregisteredUTC,
				ModuleType = devicePayload.ModuleType,
				fk_DeviceStatusID = deviceState.GetHashCode(),
				MainboardSoftwareVersion = devicePayload.MainboardSoftwareVersion,
				FirmwarePartNumber = devicePayload.RadioFirmwarePartNumber == null ? devicePayload.FirmwarePartNumber : devicePayload.RadioFirmwarePartNumber,
				GatewayFirmwarePartNumber = devicePayload.GatewayFirmwarePartNumber,
				DataLinkType = devicePayload.DataLinkType,
				InsertUTC = currentUTC,
				UpdateUTC = currentUTC,
				fk_DeviceTypeID = deviceType.DeviceTypeID,
				CellModemIMEI = devicePayload.CellModemIMEI,
				DevicePartNumber = devicePayload.DevicePartNumber,
				CellularFirmwarePartnumber = devicePayload.CellularFirmwarePartnumber,
				NetworkFirmwarePartnumber = devicePayload.NetworkFirmwarePartnumber,
				SatelliteFirmwarePartnumber = devicePayload.SatelliteFirmwarePartnumber,
				DeviceUID = devicePayload.DeviceUID
			};

			var kafkaMessageList = new List<KafkaMessage>();
			kafkaTopicNames.ForEach(topic =>
			{
				var kafkaMessage = new KafkaMessage()
				{
					Key = device.DeviceUID.ToString(),
					Message = new { CreateDeviceEvent = mapper.Map<CreateDevicePayload, CreateDeviceEvent>(devicePayload) },
					Topic = topic
				};
				kafkaMessageList.Add(kafkaMessage);
			});

			kafkaTopicNamesV2.ForEach(topic =>
			{
				var kafkaMessage = new KafkaMessage()
				{
					Key = device.DeviceUID.ToString(),
					Message = ToDevicePayloadV2(devicePayload, devicePersonality),
					Topic = topic
				};
				kafkaMessageList.Add(kafkaMessage);
			});


			var actions = new List<Action>();

			actions.Add(() => transactions.Upsert(deviceModel));
			actions.Add(() => transactions.Upsert<DbDevicePersonality>(devicePersonality));
			actions.Add(() => transactions.Publish(kafkaMessageList));

			return transactions.Execute(actions);
		}
		public bool UpdateDevice(UpdateDeviceEvent device, DeviceStateEnum deviceState)
		{

			var currentUTC = DateTime.UtcNow;
			var devicePayload = new UpdateDevicePayload
			{
				ActionUTC = currentUTC,
				CellModemIMEI = device.CellModemIMEI,
				CellularFirmwarePartnumber = device.CellularFirmwarePartnumber,
				DataLinkType = device.DataLinkType,
				DeregisteredUTC = device.DeregisteredUTC,
				DevicePartNumber = device.DevicePartNumber,
				DeviceSerialNumber = device.DeviceSerialNumber,
				DeviceState = deviceState==DeviceStateEnum.None?null: deviceState.ToString(),
				DeviceType = device.DeviceType,
				DeviceUID = device.DeviceUID.Value,
				FirmwarePartNumber = device.FirmwarePartNumber,
				GatewayFirmwarePartNumber = device.GatewayFirmwarePartNumber,
				MainboardSoftwareVersion = device.MainboardSoftwareVersion,
				ModuleType = device.ModuleType,
				NetworkFirmwarePartnumber = device.NetworkFirmwarePartnumber,
				OwningCustomerUID = device.OwningCustomerUID,
				RadioFirmwarePartNumber = device.RadioFirmwarePartNumber,
				ReceivedUTC = device.ReceivedUTC,
				SatelliteFirmwarePartnumber = device.SatelliteFirmwarePartnumber
			};

			var existingDeviceProp = GetExistingDeviceProperties(device.DeviceUID.Value);
			if (PLDeviceTypes.Contains(device.DeviceType.ToLower(), StringComparer.InvariantCultureIgnoreCase))
			{
				existingDeviceProp.RadioFirmwarePartNumber = existingDeviceProp.FirmwarePartNumber;
				existingDeviceProp.FirmwarePartNumber = null;
			}

			var existingPersonalities = GetExistingDevicePersonalities(device.DeviceUID.Value);
			DbDeviceType deviceType = string.IsNullOrEmpty(devicePayload.DeviceType) ? deviceTypesCache.First(x =>string.Equals(x.Key,existingDeviceProp.DeviceType,StringComparison.InvariantCultureIgnoreCase)).Value : deviceTypesCache.First(x =>string.Equals(x.Key,devicePayload.DeviceType,StringComparison.InvariantCultureIgnoreCase)).Value;
			var devicePersonality = GetDevicePersonalities(devicePayload, existingPersonalities, deviceType);
			devicePayload = AppendExistingDeviceProperties(devicePayload, existingDeviceProp);
			Guid? currentOwningCustomerUID = GetExistingOwningCustomerUid(device.DeviceUID.Value);
			devicePayload.OwningCustomerUID = devicePayload.OwningCustomerUID ?? currentOwningCustomerUID;
			//DeviceSerialNumber will be persisited as NULL in masterdata and send it as Empty to Kafka
			devicePayload.DeviceSerialNumber = string.IsNullOrEmpty(devicePayload.DeviceSerialNumber) ? string.Empty : devicePayload.DeviceSerialNumber;
			bool equal = CheckExistingDevicePropertiesForUpdate(devicePayload, (DeviceStateEnum)Enum.Parse(typeof(DeviceStateEnum), devicePayload.DeviceState), currentOwningCustomerUID, existingDeviceProp);

			if (equal)
			{
				logger.LogInformation("No properties to update");
				return false;
			}
			else
			{
				

				var kafkaMessageList = new List<KafkaMessage>();
				kafkaTopicNames.ForEach(topic =>
				{
					var kafkaMessage = new KafkaMessage()
					{
						Key = device.DeviceUID.ToString(),
						Message = new { UpdateDeviceEvent = mapper.Map<UpdateDevicePayload, UpdateDeviceEvent>(devicePayload) },
						Topic = topic
					};
					kafkaMessageList.Add(kafkaMessage);
				});
				

				kafkaTopicNamesV2.ForEach(topic =>
				{
					var kafkaMessage = new KafkaMessage()
					{
						Key = device.DeviceUID.ToString(),
						Message = ToDevicePayloadV2(devicePayload, devicePersonality),
						Topic = topic
					};
					kafkaMessageList.Add(kafkaMessage);
				});

				List<Action> actions = new List<Action>();

				var deviceModel = new DbDevice
				{
					CellModemIMEI = devicePayload.CellModemIMEI,
					CellularFirmwarePartnumber = devicePayload.CellularFirmwarePartnumber,
					DataLinkType = devicePayload.DataLinkType,
					DeregisteredUTC = devicePayload.DeregisteredUTC,
					DevicePartNumber = devicePayload.DevicePartNumber,
					SerialNumber = devicePayload.DeviceSerialNumber,
					fk_DeviceStatusID = deviceState.GetHashCode(),
					fk_DeviceTypeID = deviceType.DeviceTypeID,
					DeviceUID = devicePayload.DeviceUID,
					FirmwarePartNumber = devicePayload.RadioFirmwarePartNumber == null ? devicePayload.FirmwarePartNumber : devicePayload.RadioFirmwarePartNumber,
					GatewayFirmwarePartNumber = devicePayload.GatewayFirmwarePartNumber,
					MainboardSoftwareVersion = devicePayload.MainboardSoftwareVersion,
					ModuleType = devicePayload.ModuleType,
					NetworkFirmwarePartnumber = devicePayload.NetworkFirmwarePartnumber,
					SatelliteFirmwarePartnumber = devicePayload.SatelliteFirmwarePartnumber,
					UpdateUTC = currentUTC
				};
				var asset = GetOwningCustomerUID(device.DeviceUID.Value);

				if (device.OwningCustomerUID.HasValue && asset != null	&& (asset.OwningCustomerUID == Guid.Empty || !device.OwningCustomerUID.Equals(asset.OwningCustomerUID)))
				{
					var updateAsset = new UpdateAssetEvent
					{
						AssetName = asset.AssetName,
						AssetType = asset.AssetType,
						EquipmentVIN = asset.EquipmentVIN,
						IconKey = asset.IconKey,
						Model = asset.Model,
						ModelYear = asset.ModelYear,
						LegacyAssetID = asset.LegacyAssetID,
						AssetUID = asset.AssetUID,
						OwningCustomerUID = device.OwningCustomerUID.Value,
						ActionUTC = DateTime.UtcNow,
						ReceivedUTC=DateTime.UtcNow
					};

					actions.Add(() => assetServices.UpdateAsset(updateAsset)); 

				}
				actions.Add(() => transactions.Upsert(deviceModel)); 
				actions.Add(() => transactions.Upsert<DbDevicePersonality>(devicePersonality));
				actions.Add(() => transactions.Publish(kafkaMessageList));
				return transactions.Execute(actions);
			}
		}
		public bool UpdateDeviceProperties(UpdateDeviceProperties device, Guid deviceUid)
		{
			DbDeviceType deviceType = deviceTypesCache.First(x => string.Equals(x.Key,device.DeviceType,StringComparison.InvariantCultureIgnoreCase)).Value;
			var currentUTC = DateTime.UtcNow;
			var devicePayload = new UpdateDevicePayload
			{
				ActionUTC = currentUTC,
				CellularFirmwarePartnumber = device.CellularFirmwarePartnumber,
				DataLinkType = device.DataLinkType,
				DeviceSerialNumber = device.DeviceSerialNumber,
				DeviceType = device.DeviceType,
				DeviceUID = deviceUid,
				GatewayFirmwarePartNumber = device.GatewayFirmwarePartNumber,
				MainboardSoftwareVersion = device.MainboardSoftwareVersion,
				ModuleType = device.ModuleType,
				NetworkFirmwarePartnumber = device.NetworkFirmwarePartnumber,
				RadioFirmwarePartNumber = device.RadioFirmwarePartNumber,
				ReceivedUTC = device.ReceivedUTC,
				SatelliteFirmwarePartnumber = device.SatelliteFirmwarePartnumber,
				Description = device.Description
			};

			
			var existingDeviceProp = GetExistingDeviceProperties(deviceUid);
			var existingPersonalities = GetExistingDevicePersonalities(devicePayload.DeviceUID);
			var devicePersonality = GetDevicePersonalities(devicePayload, existingPersonalities, deviceType);
			devicePayload = AppendExistingDeviceProperties(devicePayload, existingDeviceProp);
			devicePayload.OwningCustomerUID = GetExistingOwningCustomerUid(deviceUid);
			var deviceState = (DeviceStateEnum)Enum.Parse(typeof(DeviceStateEnum), existingDeviceProp.DeviceState,true);
			devicePayload.ReceivedUTC = DateTime.UtcNow;

			bool equal = CheckExistingDevicePropertiesForUpdate(devicePayload, deviceState, devicePayload.OwningCustomerUID, existingDeviceProp);

			if (equal)
			{
				logger.LogInformation("No properties to update");
				return false;
			}
			else
			{

				var kafkaMessageList = new List<KafkaMessage>();
				kafkaTopicNames.ForEach(topic =>
				{
					var kafkaMessage = new KafkaMessage()
					{
						Key = deviceUid.ToString(),
						Message = new { UpdateDeviceEvent = mapper.Map<UpdateDevicePayload, UpdateDeviceEvent>(devicePayload) },
						Topic = topic
					};
					kafkaMessageList.Add(kafkaMessage);
				});

				kafkaTopicNamesV2.ForEach(topic =>
				{
					var kafkaMessage = new KafkaMessage()
					{
						Key = deviceUid.ToString(),
						Message = ToDevicePayloadV2(devicePayload, devicePersonality),
						Topic = topic
					};
					kafkaMessageList.Add(kafkaMessage);
				});

				var actions = new List<Action>();
				var deviceModel = new DbDevice
				{
					CellModemIMEI = devicePayload.CellModemIMEI,
					CellularFirmwarePartnumber = devicePayload.CellularFirmwarePartnumber,
					DataLinkType = devicePayload.DataLinkType,
					DeregisteredUTC = devicePayload.DeregisteredUTC,
					DevicePartNumber = devicePayload.DevicePartNumber,
					SerialNumber = devicePayload.DeviceSerialNumber,
					fk_DeviceTypeID = deviceType.DeviceTypeID,
					DeviceUID = devicePayload.DeviceUID,
					FirmwarePartNumber = devicePayload.RadioFirmwarePartNumber == null ? devicePayload.FirmwarePartNumber : devicePayload.RadioFirmwarePartNumber,
					GatewayFirmwarePartNumber = devicePayload.GatewayFirmwarePartNumber,
					MainboardSoftwareVersion = devicePayload.MainboardSoftwareVersion,
					ModuleType = devicePayload.ModuleType,
					NetworkFirmwarePartnumber = devicePayload.NetworkFirmwarePartnumber,
					SatelliteFirmwarePartnumber = devicePayload.SatelliteFirmwarePartnumber,
					UpdateUTC = currentUTC,
					fk_DeviceStatusID = deviceState.GetHashCode()
				};
				actions.Add(() => transactions.Upsert(deviceModel));
				actions.Add(() => transactions.Upsert<DbDevicePersonality>(devicePersonality));
				actions.Add(() => transactions.Publish(kafkaMessageList));
				return transactions.Execute(actions);
			}
		}
		#endregion CUD
		#region  Public Methods
		public DeviceProperties GetExistingDeviceProperties(Guid deviceUID)
		{
			var existingDeviceProp = GetExistingDevice(deviceUID);
			if (existingDeviceProp == null)
			{
				logger.LogInformation($"Device {deviceUID.ToString()} Doesn't Exist");
				return null;
			}
			var device = new DeviceProperties
			{
				DeviceSerialNumber = existingDeviceProp.SerialNumber,
				DeviceState = ((DeviceStateEnum)Enum.Parse(typeof(DeviceStateEnum), existingDeviceProp.fk_DeviceStatusID.ToString())).ToString(),
				DeviceType = deviceTypesCache.Where(x => x.Value.DeviceTypeID == existingDeviceProp.fk_DeviceTypeID).First().Key,
				CellModemIMEI = existingDeviceProp.CellModemIMEI,
				CellularFirmwarePartnumber = existingDeviceProp.CellularFirmwarePartnumber,
				DataLinkType = existingDeviceProp.DataLinkType,
				DeregisteredUTC = existingDeviceProp.DeregisteredUTC,
				DevicePartNumber = existingDeviceProp.DevicePartNumber,
				DeviceUID = existingDeviceProp.DeviceUID.ToString(),
				FirmwarePartNumber = existingDeviceProp.FirmwarePartNumber,
				GatewayFirmwarePartNumber = existingDeviceProp.GatewayFirmwarePartNumber,
				MainboardSoftwareVersion = existingDeviceProp.MainboardSoftwareVersion,
				ModuleType = existingDeviceProp.ModuleType,
				NetworkFirmwarePartnumber = existingDeviceProp.NetworkFirmwarePartnumber,
				SatelliteFirmwarePartnumber = existingDeviceProp.SatelliteFirmwarePartnumber
			};
			return device;
		}
		public bool AssociateAssetDevice(AssociateDeviceAssetEvent associateDeviceAsset)
		{
			associateDeviceAsset.ReceivedUTC = DateTime.UtcNow;

			var assetDevice = new DbAssetDevice
			{
				fk_AssetUID = associateDeviceAsset.AssetUID,
				fk_DeviceUID = associateDeviceAsset.DeviceUID,
				ActionUTC = associateDeviceAsset.ActionUTC.Value,
				RowUpdatedUTC = associateDeviceAsset.ReceivedUTC
			};
			
			var kafkaMessageList = new List<KafkaMessage>();
			kafkaTopicNames.ForEach(topic =>
			{
				var kafkaMessage = new KafkaMessage()
				{
					Key = associateDeviceAsset.DeviceUID.ToString(),
					Message = new { AssociateDeviceAssetEvent = associateDeviceAsset },
					Topic = topic
				};
				kafkaMessageList.Add(kafkaMessage);
			});
			var actions = new List<Action>();
			actions.Add(() => transactions.Upsert(assetDevice));
			actions.Add(() => transactions.Publish(kafkaMessageList));
			return transactions.Execute(actions);
		}
		public bool DissociateAssetDevice(DissociateDeviceAssetEvent dissociateDeviceAsset)
		{
			var kafkaMessageList = new List<KafkaMessage>();
			dissociateDeviceAsset.ReceivedUTC = DateTime.UtcNow;
			kafkaTopicNames.ForEach(topic =>
			{
				KafkaMessage kafkaMessage = new KafkaMessage()
				{
					Key = dissociateDeviceAsset.DeviceUID.ToString(),
					Message = new { DissociateDeviceAssetEvent = dissociateDeviceAsset },
					Topic = topic
				};
				kafkaMessageList.Add(kafkaMessage);
			});

			var actions = new List<Action>();
			actions.Add(() => transactions.Delete($"DELETE FROM md_asset_AssetDevice WHERE fk_AssetUID={dissociateDeviceAsset.AssetUID.ToStringWithoutHyphens().WrapWithUnhex()} AND fk_DeviceUID={dissociateDeviceAsset.DeviceUID.ToStringWithoutHyphens().WrapWithUnhex()}"));
			actions.Add(() => transactions.Publish(kafkaMessageList));
			return transactions.Execute(actions);
		}
		public bool CheckExistingDevice(Guid deviceGuid)
		{
			var readExistingDeviceQuery = string.Format(Queries.CHECK_EXISTING_DEVICE_QUERY, deviceGuid.ToStringWithoutHyphens().WrapWithUnhex());
			return (long)transactions.GetValue(readExistingDeviceQuery) > 0;
		}
		public List<Guid> GetDeviceDetailsBySerialNumberAndType(string serialNumber, string type)
		{
			var deviceQuery = string.Format(Queries.GET_DEVICE_QUERY, serialNumber,deviceTypesCache.First(x => string.Equals(x.Key, type, StringComparison.InvariantCultureIgnoreCase)).Value.DeviceTypeID);

			var deviceUids = transactions.Get<string>(deviceQuery);
			//return !string.IsNullOrEmpty(deviceUid) ? new Guid(deviceUid) : Guid.Empty;

			return deviceUids.Select(x => Guid.Parse(x)).ToList();
		}
	
		public IEnumerable<AssetDevicePropertiesV2> GetDevicePropertiesV2ByAssetGuid(Guid assetGuid)
		{
			IEnumerable<AssetDevicePropertiesV2> deviceProperties;
			var readExistingDevicePropertiesQuery = string.Format(Queries.GET_DEVICE_PROPERTIES_V2_BY_ASSET_UID, assetGuid.ToStringWithoutHyphens().WrapWithUnhex());
			deviceProperties = transactions.Get<AssetDevicePropertiesV2>(readExistingDevicePropertiesQuery);
			return deviceProperties;
		}
		public IEnumerable<DevicePropertiesV2> GetDevicePropertiesV2ByDeviceGuid(Guid deviceGuid)
		{
			IEnumerable<DevicePropertiesV2> deviceProperties;
			var readExistingDevicePropertiesQuery = string.Format(Queries.GET_DEVICE_PROPERTIES_V2_BY_DEVICE_UID, deviceGuid.ToStringWithoutHyphens().WrapWithUnhex());
			deviceProperties = transactions.Get<DevicePropertiesV2>(readExistingDevicePropertiesQuery);
			return deviceProperties;
		}

		public DbAssetDevice GetAssetDevice(Guid assetGuid,Guid deviceGuid)
		{
			var getDevicesQuery = string.Format(Queries.GetAssetDevice, assetGuid.ToStringWithoutHyphens().WrapWithUnhex(), deviceGuid.ToStringWithoutHyphens().WrapWithUnhex());
			return transactions.Get<DbAssetDevice>(getDevicesQuery).FirstOrDefault();
		}

		public bool ValidateAuthorizedCustomerByAsset(Guid userGuid, Guid assetGuid)
		{
			return (long)transactions.GetValue(string.Format(Queries.ValidateAuthorizedCustomerAssetQuery, userGuid.ToStringWithoutHyphens(), assetGuid.ToStringWithoutHyphens())) == 0;
		}
		public bool ValidateAuthorizedCustomerByDevice(Guid userGuid, Guid deviceGuid)
		{
			return (long)transactions.GetValue(string.Format(Queries.ValidateAuthorizedCustomerDeviceQuery, userGuid.ToStringWithoutHyphens(), deviceGuid.ToStringWithoutHyphens())) == 0;
		}
		public List<Guid> GetCustomersForApplication(string applicationName)
		{
			var customerUIDs= transactions.Get<string>($"select HEX(fk_CustomerUID) fk_CustomerUID from md_customer_CustomerApplication where `ApplicationCode` = '{applicationName}'");
			return customerUIDs.Select(x => Guid.Parse(x)).ToList();
		}

		public DeviceDto GetAssociatedDevicesByAsset(Guid assetGuid)
		{
			var getDevicesQuery = string.Format(Queries.GetDevicesByAssetUid, assetGuid.ToStringWithoutHyphens().WrapWithUnhex());
			return transactions.Get<DeviceDto>(getDevicesQuery).FirstOrDefault();
		}

		#endregion Public Methods

		#region Private Methods

		private AssetDto GetOwningCustomerUID(Guid deviceUID)
		{
			AssetDto asset = transactions.Get<AssetDto>(
						$"{Queries.GetOwningCustomerUIDQuery} where AD.fk_DeviceUID = {deviceUID.ToStringWithoutHyphens().WrapWithUnhex()}").FirstOrDefault();
			return asset;
		}
		private DeviceDataModels.DbDeviceType GetDeviceTypeByType(string devicetype)
		{
			return transactions.Get<DbDeviceType>(string.Format("select dt.TypeName,dt.DeviceTypeID,dt.DefaultValueJson,dt.fk_DeviceTypeFamilyID from md_device_DeviceType dt where dt.TypeName='{0}'", devicetype)).FirstOrDefault();
		}

		/// <summary>
		/// Get device personalities
		/// </summary>
		/// <param name="devicePayLoad"></param>
		/// <param name="personalities"></param>        
		/// <returns></returns>
		private List<DbDevicePersonality> GetDevicePersonalities(DevicePayload devicePayLoad, List<DbDevicePersonality> personalities,DbDeviceType devicetype)
		{
			var supportedPersonalities = GetDeviceSupportedPersonality(devicetype.DefaultValueJson);
			bool isCreateEvent = personalities==null; //this will be NULL for create case
			personalities = personalities ?? new List<DbDevicePersonality>();
			var populatedPersonalities =  new List<DbDevicePersonality>();
			if (supportedPersonalities != null)
			{
				var personality = new DbDevicePersonality();
				foreach (var item in supportedPersonalities)
				{
					string desc = null;
					var propertyInfo = typeof(DevicePayload).GetProperty(item.PersonalityTypeName);
					var value = propertyInfo != null ? propertyInfo.GetValue(devicePayLoad, null) : null;

					personality = personalities.FirstOrDefault(x => x.fk_PersonalityTypeID == item.DevicePersonalityTypeID);
					if (personality != null)
					{
						personalities.Remove(personality);
					}
					string personalityValue = null;
					if (value == null)
					{
						personalityValue = personality != null ? personality.PersonalityValue : null;
					}
					else // for create event Empty alone should be treated as NULL
					{
						if (isCreateEvent)
						{
							personalityValue = value.ToString() == string.Empty ? null : value.ToString();
						}
						else
						{
							personalityValue = value?.ToString();
						}
					}
					if (personalityValue != null)
					{
					    desc = !string.IsNullOrEmpty(value?.ToString()) && !string.IsNullOrEmpty(devicePayLoad.Description) ? devicePayLoad.Description : personality?.PersonalityDesc;

						personalities.Add(
							new DbDevicePersonality
							{

								DevicePersonalityUID = personality == null ? Guid.NewGuid() : personality.DevicePersonalityUID,
								fk_DeviceUID = devicePayLoad.DeviceUID,
								fk_PersonalityTypeID = item.DevicePersonalityTypeID,
								PersonalityDesc = desc,
								PersonalityValue = value?.ToString() == string.Empty ? null : personalityValue,
								RowUpdatedUTC = DateTime.UtcNow
							});
					}
				}
			}
			return personalities;
		}

		
		private UpdateDevicePayload AppendExistingDeviceProperties(UpdateDevicePayload updateDevice, DeviceProperties existingDeviceProp)
		{
			foreach (PropertyInfo propertyInfo in updateDevice.GetType().GetProperties().Where(p => p.CanRead))
			{
				if (propertyInfo.Name != "OwningCustomerUID")
				{
					if (propertyInfo.GetValue(updateDevice) == null)
					{
						object value = existingDeviceProp.GetType().GetProperties().Single(p => p.Name.ToString() == propertyInfo.Name.ToString()).GetValue(existingDeviceProp);
						propertyInfo.SetValue(updateDevice, value);
					}
					else if (propertyInfo.GetValue(updateDevice)?.ToString() == string.Empty)
					{
						propertyInfo.SetValue(updateDevice, null);
					}
				}
			}
		
			updateDevice.DeviceUID = Guid.Parse(existingDeviceProp.DeviceUID);
			return updateDevice;
		}

		private CreateDevicePayload SetNullIfPropertyEmpty(CreateDevicePayload createDevice)
		{
			//TODO : Alternative approch
			foreach (PropertyInfo propertyInfo in createDevice.GetType().GetProperties().Where(p => p.CanRead))
			{

				if (propertyInfo.GetValue(createDevice) != null && propertyInfo.GetValue(createDevice)?.ToString() == string.Empty)
				{
					propertyInfo.SetValue(createDevice, null);
				}
			}
			return createDevice;
		}
		private DbDevice GetExistingDevice(Guid deviceGuid)
		{
			var readExistingDevicePropertiesQuery = string.Format(Queries.CHECK_EXISTING_DEVICE_PROPERTIES_QUERY, deviceGuid.ToStringWithoutHyphens().WrapWithUnhex());
			return transactions.Get<DbDevice>(readExistingDevicePropertiesQuery).FirstOrDefault();
		}
		private Guid? GetExistingOwningCustomerUid(Guid deviceGuid)
		{
			var readOwningCustomerUidQuery = string.Format(Queries.CHECK_EXISTING_OWNING_CUSTOMER_UID_QUERY, deviceGuid.ToStringWithoutHyphens().WrapWithUnhex());
			string uid = (string)transactions.GetValue(readOwningCustomerUidQuery);
			return !string.IsNullOrEmpty(uid) ? new Guid(uid) : (Guid?)null;
		}
		private List<DevicePersonalityType> GetDeviceSupportedPersonality(string defaultValueJson)
		{
			return JsonConvert.DeserializeObject<List<DevicePersonalityType>>(defaultValueJson);

		}
		private DevicePayloadV2 ToDevicePayloadV2(DevicePayload deviceEvent, List<DbDevicePersonality> devicePersonality)
		{
			return new DevicePayloadV2
			{
				DeviceUID = deviceEvent.DeviceUID,
				DeviceType = deviceEvent.DeviceType,
				DeviceState = deviceEvent.DeviceState.ToString(),
				DeviceSerialNumber = deviceEvent.DeviceSerialNumber,
				ModuleType = deviceEvent.ModuleType,
				DeregisteredUTC = deviceEvent.DeregisteredUTC,
				DataLinkType = deviceEvent.DataLinkType,
				Personalities = devicePersonality?.Select(personality => new DevicePersonalityPayload
				{
					Name = ((DevicePersonalityTypeEnum)personality.fk_PersonalityTypeID).ToString(),
					Description = personality.PersonalityDesc,
					Value = personality.PersonalityValue
				}).ToList(),
				ActionUTC = deviceEvent.ActionUTC,
				ReceivedUTC = deviceEvent.ReceivedUTC
			};
		}
		
		private bool CheckExistingDevicePropertiesForUpdate(UpdateDevicePayload updateDevice, DeviceStateEnum deviceState, Guid? currentOwningCustomerUID, DeviceProperties existingDeviceProp)
		{
			bool areEqual = false;
			updateDevice.DeviceState = deviceState.ToString();
			if (updateDevice.DeregisteredUTC.HasValue)
				updateDevice.DeregisteredUTC = CompareHelper.ConvertDateTimeForComparison(updateDevice.DeregisteredUTC.Value);
			if (existingDeviceProp.DeregisteredUTC.HasValue)
				existingDeviceProp.DeregisteredUTC = CompareHelper.ConvertDateTimeForComparison(existingDeviceProp.DeregisteredUTC.Value);


			if (updateDevice.OwningCustomerUID == currentOwningCustomerUID &&
				CompareHelper.AreObjectsEqual(updateDevice, existingDeviceProp, "DeviceUID", "DeviceType", "ActionUTC", "ReceivedUTC", "OwningCustomerUID"))
				areEqual = true;
			return areEqual;
		}
		private List<DbDevicePersonality> GetExistingDevicePersonalities(Guid deviceGuid)
		{
			List<DbDevicePersonality> devicePersonalities;
			var readExistingDevicePersonalitiesQuery = $"SELECT DevicePersonalityUID as DevicePersonalityUID, fk_DeviceUid as fk_DeviceUID, fk_PersonalityTypeID,PersonalityDesc, PersonalityValue FROM md_device_DevicePersonality where fk_DeviceUID = {deviceGuid.ToStringWithoutHyphens().WrapWithUnhex()}";

			devicePersonalities = transactions.Get<DbDevicePersonality>(readExistingDevicePersonalitiesQuery).ToList();

			return devicePersonalities;
		}

	
		#endregion Private Methods
	}
}