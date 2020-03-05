using Autofac;
using AutoMapper;
using KafkaModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.ClientModel.Device;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.DbModel.Device;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Interfaces.Device;
using VSS.MasterData.WebAPI.KafkaModel.Device;
using VSS.MasterData.WebAPI.Repository.Device;
using VSS.MasterData.WebAPI.Transactions;
using Xunit;

namespace VSP.MasterData.Device.UnitTests
{
	public class DeviceServiceTests
	{
		private readonly IConfiguration configuration;
		private readonly ILogger logger;
		private readonly IMapper mapper;
		private readonly ITransactions transaction;
		private readonly IDeviceService _deviceService;
		private readonly IDeviceTypeService _deviceTypeService;
		private readonly IAssetServices _assetService;

		private List<DbDeviceType> dbDeviceType = new List<DbDeviceType>();

		public DeviceServiceTests()
		{
			logger = Substitute.For<ILogger>();
			mapper = Substitute.For<IMapper>();
			_deviceTypeService = Substitute.For<IDeviceTypeService>();
			configuration = Substitute.For<IConfiguration>();
			transaction = Substitute.For<ITransactions>();
			_assetService = Substitute.For<IAssetServices>();
			configuration["kafkaTopicNames"] = "VSS.Interfaces.Events.MasterData.IDeviceEvent-Test";
			configuration["kafkaTopicNamesV2"] = "VSS.Interfaces.Events.MasterData.IDeviceEvent-V2-Test";
			string[] topics = configuration["SubscriptionKafkaTopicNames"].Split(',');

			dbDeviceType = GetDeviceTypes();
			transaction.Get<DbDeviceType>(Arg.Any<string>()).Returns(dbDeviceType);
			var deviceTypesDictionary = new Dictionary<string, DbDeviceType>(StringComparer.OrdinalIgnoreCase);
			dbDeviceType.ForEach(x =>
			{
				deviceTypesDictionary.Add(x.TypeName, x);
			});
			_deviceTypeService.GetDeviceType().Returns(deviceTypesDictionary);
			_deviceService = new DeviceService(logger, configuration, transaction, _assetService, mapper, _deviceTypeService);


		}
		private CreateDeviceEvent GetCreateDeviceModel()
		{
			return new CreateDeviceEvent
			{
				DeviceUID = Guid.NewGuid(),
				DeviceSerialNumber = "TestSerialNumber",
				DeviceType = "PLE641PLUSPL631",
				DeviceState = "Installed",
				DeregisteredUTC = DateTime.UtcNow,
				ModuleType = "TestModuleType",
				MainboardSoftwareVersion = "TestMainboardSoftwareVersion",
				RadioFirmwarePartNumber = "TestRadioSoftwareVersion",
				GatewayFirmwarePartNumber = "TestGatewaySoftwareVersion",
				FirmwarePartNumber = "TestFirmwarePartNumber",
				DataLinkType = "J1939",
				CellModemIMEI = "TestCellModemIMEI",
				DevicePartNumber = "TestDevicePartNumber",
				CellularFirmwarePartnumber = "TestCellularFirmwarePartnumber",
				NetworkFirmwarePartnumber = "TestNetworkFirmwarePartnumber",
				SatelliteFirmwarePartnumber = "TestSatelliteFirmwarePartnumber",
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
		}
		private UpdateDeviceEvent GetUpdateDeviceModel()
		{
			return new UpdateDeviceEvent
			{
				DeviceUID = Guid.NewGuid(),
				DeviceSerialNumber = "TestSerialNumber",
				DeviceType = "PLE641PLUSPL631",
				DeviceState = "Installed",
				DeregisteredUTC = DateTime.UtcNow,
				ModuleType = "TestModuleType",
				MainboardSoftwareVersion = "TestMainboardSoftwareVersion",
				RadioFirmwarePartNumber = null,
				GatewayFirmwarePartNumber = "TestGatewaySoftwareVersion",
				DataLinkType = "J1939",
				FirmwarePartNumber = "TestFirmwarePartNumber",
				CellModemIMEI = "TestCellModemIMEI",
				DevicePartNumber = "TestDevicePartNumber",
				CellularFirmwarePartnumber = "TestCellularFirmwarePartnumber",
				NetworkFirmwarePartnumber = "TestNetworkFirmwarePartnumber",
				SatelliteFirmwarePartnumber = "TestSatelliteFirmwarePartnumber",
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow,
				OwningCustomerUID = Guid.NewGuid()
			};
		}

		private UpdateDeviceProperties GetUpdateDevicePropertiesModel()
		{
			return new UpdateDeviceProperties
			{
				DeviceSerialNumber = "TestSerialNumber",
				DeviceType = "PLE641PLUSPL631",
				ModuleType = "TestModuleType",
				MainboardSoftwareVersion = "TestMainboardSoftwareVersion",
				RadioFirmwarePartNumber = null,
				GatewayFirmwarePartNumber = "TestGatewaySoftwareVersion",
				DataLinkType = "J1939",
				CellularFirmwarePartnumber = "TestCellularFirmwarePartnumber",
				NetworkFirmwarePartnumber = "TestNetworkFirmwarePartnumber",
				SatelliteFirmwarePartnumber = "TestSatelliteFirmwarePartnumber",
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
		}
		[Fact]
		public void TestCreateDevice_ValidInput_Success()
		{
			//Arrange
			var input = GetCreateDeviceModel();


			transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				foreach (var action in x.Arg<List<Action>>())
				{
					action();
				}
				return true;
			});

			//Act
			var result = _deviceService.CreateDevice(input, Arg.Any<DeviceStateEnum>());

			//Assert

			transaction.Received(1).Upsert(Arg.Any<DbDevice>());
			transaction.Received(1).Upsert<DbDevicePersonality>(Arg.Any<List<DbDevicePersonality>>());
			transaction.Received(1).Publish(Arg.Any<List<KafkaMessage>>());
			//transaction.Received(1).Execute(Arg.Any<List<Action>>());
			Assert.True(result);
		}
		[Fact]
		public void TestCreateDevice_DbFailure_NoCalltoPublish()
		{
			//Arrange
			var input = GetCreateDeviceModel();

			transaction.When(fake => fake.Upsert<DbDevicePersonality>(Arg.Any<List<DbDevicePersonality>>())).Throw(new Exception());
			transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				try
				{
					foreach (var action in x.Arg<List<Action>>())
					{
						action();
					}
					return true;
				}
				catch
				{
					return false;
				}

			});
			//Act
			var result = _deviceService.CreateDevice(input, Arg.Any<DeviceStateEnum>());

			//Assert
			//transaction.Received(1).Execute(Arg.Any<List<Action>>());
			transaction.Received(1).Upsert(Arg.Any<DbDevice>());
			transaction.Received(1).Upsert<DbDevicePersonality>(Arg.Any<List<DbDevicePersonality>>());
			transaction.Received(0).Publish(Arg.Any<List<KafkaMessage>>());
			Assert.False(result);

		}

		[Fact]
		public void TestCreateDevice_KafkaFailure_Fail()
		{
			//Arrange
			var input = GetCreateDeviceModel();

			transaction.When(fake => fake.Publish(Arg.Any<List<KafkaMessage>>())).Throw(new Exception());
			transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				try
				{
					foreach (var action in x.Arg<List<Action>>())
					{
						action();
					}
					return true;
				}
				catch (Exception)
				{
					return false;
				}

			});
			//Act
			var result = _deviceService.CreateDevice(input, Arg.Any<DeviceStateEnum>());

			//Assert
			//transaction.Received(1).Execute(Arg.Any<List<Action>>());
			transaction.Received(1).Upsert(Arg.Any<DbDevice>());
			transaction.Received(1).Upsert<DbDevicePersonality>(Arg.Any<List<DbDevicePersonality>>());
			transaction.Received(1).Publish(Arg.Any<List<KafkaMessage>>());
			Assert.False(result);

		}

		[Theory]
		[InlineData("PLE641PLUSPL631")]
		[InlineData("PL121")]
		[InlineData("PL321")]
		public void TestUpdateDevice_ValidInput_SameDeviceProperties_NoUpdateCall_Success(string deviceTypeInput)
		{
			//Arrange
			var input = GetUpdateDeviceModel();
			input.DeviceType = deviceTypeInput;

			var owningCustomerUID = input.OwningCustomerUID;
			var deviceType = dbDeviceType.Where(x => x.TypeName == deviceTypeInput);

			var existingDevice = new DbDevice
			{
				DeviceUID = input.DeviceUID.Value,
				CellModemIMEI = input.CellModemIMEI,
				CellularFirmwarePartnumber = input.CellularFirmwarePartnumber,
				DataLinkType = input.DataLinkType,
				DeregisteredUTC = input.DeregisteredUTC,
				DevicePartNumber = input.DevicePartNumber,
				FirmwarePartNumber = input.FirmwarePartNumber,
				GatewayFirmwarePartNumber = input.GatewayFirmwarePartNumber,
				MainboardSoftwareVersion = input.MainboardSoftwareVersion,
				NetworkFirmwarePartnumber = input.NetworkFirmwarePartnumber,
				SatelliteFirmwarePartnumber = input.SatelliteFirmwarePartnumber,
				ModuleType = input.ModuleType,
				SerialNumber = input.DeviceSerialNumber,
				fk_DeviceStatusID = (int)(DeviceStateEnum)Enum.Parse(typeof(DeviceStateEnum), input.DeviceState).GetHashCode(),
				fk_DeviceTypeID = deviceType.First().DeviceTypeID
			};

			transaction.Get<DbDevice>(Arg.Any<string>()).Returns(new List<DbDevice> { existingDevice });
			transaction.GetValue(Arg.Any<string>()).Returns(owningCustomerUID.ToString());

			//Act
			var result = _deviceService.UpdateDevice(input, (DeviceStateEnum)Enum.Parse(typeof(DeviceStateEnum), input.DeviceState));

			//Assert

			if (deviceTypeInput == "PL121" || deviceTypeInput == "PL321")
			{
				transaction.Received(1).Execute(Arg.Any<List<Action>>());
				_assetService.Received(0).UpdateAsset(Arg.Any<UpdateAssetEvent>()); // same owner, no update
				transaction.Received(0).Upsert(Arg.Any<DbDevice>());
				transaction.Received(0).Upsert<DbDevicePersonality>(Arg.Any<List<DbDevicePersonality>>());
				transaction.Received(0).Publish(Arg.Any<List<KafkaMessage>>());
			}
			else
			{
				transaction.Received(0).Execute(Arg.Any<List<Action>>());
				_assetService.Received(0).UpdateAsset(Arg.Any<UpdateAssetEvent>()); // same owner, no update
				transaction.Received(0).Upsert(Arg.Any<DbDevice>());
				transaction.Received(0).Upsert<DbDevicePersonality>(Arg.Any<List<DbDevicePersonality>>());
				transaction.Received(0).Publish(Arg.Any<List<KafkaMessage>>());
			}
			Assert.False(result);
		}


		[Theory]
		[InlineData("PLE641PLUSPL631", "MainboardSoftwareVersion")]
		[InlineData("PLE641PLUSPL631", "GatewayFirmwarePartNumber")]
		[InlineData("PLE641PLUSPL631", "DataLinkType")]
		[InlineData("PLE641PLUSPL631", "CellModemIMEI")]
		[InlineData("PLE641PLUSPL631", "DevicePartNumber")]
		[InlineData("PLE641PLUSPL631", "CellularFirmwarePartnumber")]
		[InlineData("PLE641PLUSPL631", "NetworkFirmwarePartnumber")]
		[InlineData("PLE641PLUSPL631", "SatelliteFirmwarePartnumber")]
		[InlineData("PLE641PLUSPL631", "ModuleType")]
		[InlineData("PL121", "MainboardSoftwareVersion")]
		[InlineData("PL121", "GatewayFirmwarePartNumber")]
		[InlineData("PL121", "DataLinkType")]
		[InlineData("PL121", "CellModemIMEI")]
		[InlineData("PL121", "DevicePartNumber")]
		[InlineData("PL121", "CellularFirmwarePartnumber")]
		[InlineData("PL121", "NetworkFirmwarePartnumber")]
		[InlineData("PL121", "SatelliteFirmwarePartnumber")]
		[InlineData("PL121", "ModuleType")]
		public void TestUpdateDevice_ValidInput_DiffrentDeviceProperties_UpdateCall_SameOwner_Success(string deviceTypeInput, string propertyNameToChangeValue)
		{
			//Arrange
			var input = GetUpdateDeviceModel();
			input.DeviceType = deviceTypeInput;

			var owningCustomerUID = input.OwningCustomerUID; // same owner
			var deviceType = dbDeviceType.Where(x => x.TypeName == deviceTypeInput);

			var existingDevice = new DbDevice
			{
				DeviceUID = input.DeviceUID.Value,
				CellModemIMEI = input.CellModemIMEI,
				CellularFirmwarePartnumber = input.CellularFirmwarePartnumber,
				DataLinkType = input.DataLinkType,
				DeregisteredUTC = input.DeregisteredUTC,
				DevicePartNumber = input.DevicePartNumber,
				FirmwarePartNumber = input.FirmwarePartNumber,
				GatewayFirmwarePartNumber = input.GatewayFirmwarePartNumber,
				MainboardSoftwareVersion = input.MainboardSoftwareVersion,
				NetworkFirmwarePartnumber = input.NetworkFirmwarePartnumber,
				SatelliteFirmwarePartnumber = input.SatelliteFirmwarePartnumber,
				ModuleType = input.ModuleType,
				SerialNumber = input.DeviceSerialNumber,
				fk_DeviceStatusID = (int)(DeviceStateEnum)Enum.Parse(typeof(DeviceStateEnum), input.DeviceState).GetHashCode(),
				fk_DeviceTypeID = deviceType.First().DeviceTypeID
			};

			Type myType = typeof(UpdateDeviceEvent);
			PropertyInfo myPropInfo = myType.GetProperty(propertyNameToChangeValue);
			myPropInfo.SetValue(input, "Changed", null);// it should call update & publish

			transaction.Get<DbDevice>(Arg.Any<string>()).Returns(new List<DbDevice> { existingDevice });
			transaction.GetValue(Arg.Any<string>()).Returns(owningCustomerUID.ToString());

			var devicePersonality = new List<DbDevicePersonality> { new DbDevicePersonality{
				fk_DeviceUID=input.DeviceUID.Value,fk_PersonalityTypeID= 0,PersonalityValue= "Test",PersonalityDesc= "Test"
				}, new DbDevicePersonality{
					fk_DeviceUID=input.DeviceUID.Value,fk_PersonalityTypeID= 1,PersonalityValue= "Test1",PersonalityDesc= "Test1" }
			};
			transaction.Get<DbDevicePersonality>(Arg.Any<string>()).Returns(devicePersonality);
			//var deviceType = dbDeviceType.Where(x => x.TypeName == "PLE641PLUSPL631");
			transaction.Get<DbDeviceType>(Arg.Any<string>()).Returns(deviceType);
			var asset = new AssetDto { AssetUID = Guid.NewGuid(), OwningCustomerUID = owningCustomerUID.Value };
			transaction.Get<AssetDto>(Arg.Any<string>()).Returns(new List<AssetDto> { asset });

			transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				foreach (var action in x.Arg<List<Action>>())
				{
					action();
				}
				return true;
			});

			//Act
			var result = _deviceService.UpdateDevice(input, Arg.Any<DeviceStateEnum>());

			//Assert
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
			_assetService.Received(0).UpdateAsset(Arg.Any<UpdateAssetEvent>()); // same owner, no update
			transaction.Received(1).Upsert(Arg.Any<DbDevice>());
			transaction.Received(1).Upsert<DbDevicePersonality>(Arg.Any<List<DbDevicePersonality>>());
			transaction.Received(1).Publish(Arg.Any<List<KafkaMessage>>());
			Assert.True(result);
		}

		[Theory]
		[InlineData("PLE641PLUSPL631", "MainboardSoftwareVersion")]
		[InlineData("PLE641PLUSPL631", "ModuleType")]
		[InlineData("PL121", "MainboardSoftwareVersion")]
		public void TestUpdateDevice_ValidInput_DiffrentDeviceProperties_UpdateCall_DiffrentOwner_Success(string deviceTypeInput, string propertyNameToChangeValue)
		{
			//Arrange
			var input = GetUpdateDeviceModel();
			input.DeviceType = deviceTypeInput;

			var owningCustomerUID = Guid.NewGuid(); // diffrent owner
			var deviceType = dbDeviceType.Where(x => x.TypeName == deviceTypeInput);

			var existingDevice = new DbDevice
			{
				DeviceUID = input.DeviceUID.Value,
				CellModemIMEI = input.CellModemIMEI,
				CellularFirmwarePartnumber = input.CellularFirmwarePartnumber,
				DataLinkType = input.DataLinkType,
				DeregisteredUTC = input.DeregisteredUTC,
				DevicePartNumber = input.DevicePartNumber,
				FirmwarePartNumber = input.FirmwarePartNumber,
				GatewayFirmwarePartNumber = input.GatewayFirmwarePartNumber,
				MainboardSoftwareVersion = input.MainboardSoftwareVersion,
				NetworkFirmwarePartnumber = input.NetworkFirmwarePartnumber,
				SatelliteFirmwarePartnumber = input.SatelliteFirmwarePartnumber,
				ModuleType = input.ModuleType,
				SerialNumber = input.DeviceSerialNumber,
				fk_DeviceStatusID = (int)(DeviceStateEnum)Enum.Parse(typeof(DeviceStateEnum), input.DeviceState).GetHashCode(),
				fk_DeviceTypeID = deviceType.First().DeviceTypeID
			};

			Type myType = typeof(UpdateDeviceEvent);
			PropertyInfo myPropInfo = myType.GetProperty(propertyNameToChangeValue);
			myPropInfo.SetValue(input, "Changed", null);// it should call update & publish

			transaction.Get<DbDevice>(Arg.Any<string>()).Returns(new List<DbDevice> { existingDevice });
			transaction.GetValue(Arg.Any<string>()).Returns(owningCustomerUID.ToString());

			var devicePersonality = new List<DbDevicePersonality> { new DbDevicePersonality{
				fk_DeviceUID=input.DeviceUID.Value,fk_PersonalityTypeID= 0,PersonalityValue= "Test",PersonalityDesc= "Test"
				}, new DbDevicePersonality{
					fk_DeviceUID=input.DeviceUID.Value,fk_PersonalityTypeID= 1,PersonalityValue= "Test1",PersonalityDesc= "Test1" }
			};
			transaction.Get<DbDevicePersonality>(Arg.Any<string>()).Returns(devicePersonality);
			//var deviceType = dbDeviceType.Where(x => x.TypeName == "PLE641PLUSPL631");
			transaction.Get<DbDeviceType>(Arg.Any<string>()).Returns(deviceType);
			var asset = new AssetDto { AssetUID = Guid.NewGuid(), OwningCustomerUID = owningCustomerUID };
			transaction.Get<AssetDto>(Arg.Any<string>()).Returns(new List<AssetDto> { asset });

			transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				foreach (var action in x.Arg<List<Action>>())
				{
					action();
				}
				return true;
			});

			//Act
			var result = _deviceService.UpdateDevice(input, Arg.Any<DeviceStateEnum>());

			//Assert
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
			_assetService.Received(1).UpdateAsset(Arg.Any<UpdateAssetEvent>()); // same owner, update call
			transaction.Received(1).Upsert(Arg.Any<DbDevice>());
			transaction.Received(1).Upsert<DbDevicePersonality>(Arg.Any<List<DbDevicePersonality>>());
			transaction.Received(1).Publish(Arg.Any<List<KafkaMessage>>());
			Assert.True(result);
		}

		[Theory]
		[InlineData("PLE641PLUSPL631")]
		[InlineData("PL121")]
		[InlineData("PL321")]
		public void TestUpdateDevice_DbFailure_NoCalltoPublish(string deviceTypeInput)
		{
			//Arrange
			var input = GetUpdateDeviceModel();
			input.DeviceType = deviceTypeInput;

			var owningCustomerUID = Guid.NewGuid();
			var deviceType = dbDeviceType.Where(x => x.TypeName == deviceTypeInput);

			var existingDevice = new DbDevice
			{
				DeviceUID = input.DeviceUID.Value,
				CellModemIMEI = input.CellModemIMEI,
				CellularFirmwarePartnumber = input.CellularFirmwarePartnumber,
				DataLinkType = input.DataLinkType,
				DeregisteredUTC = input.DeregisteredUTC,
				DevicePartNumber = input.DevicePartNumber,
				FirmwarePartNumber = input.FirmwarePartNumber,
				GatewayFirmwarePartNumber = input.GatewayFirmwarePartNumber,
				MainboardSoftwareVersion = input.MainboardSoftwareVersion,
				NetworkFirmwarePartnumber = input.NetworkFirmwarePartnumber,
				SatelliteFirmwarePartnumber = input.SatelliteFirmwarePartnumber,
				ModuleType = input.ModuleType,
				SerialNumber = input.DeviceSerialNumber,
				fk_DeviceStatusID = (int)(DeviceStateEnum)Enum.Parse(typeof(DeviceStateEnum), input.DeviceState).GetHashCode(),
				fk_DeviceTypeID = deviceType.First().DeviceTypeID
			};
			input.MainboardSoftwareVersion = "Changed";

			transaction.Get<DbDevice>(Arg.Any<string>()).Returns(new List<DbDevice> { existingDevice });
			transaction.GetValue(Arg.Any<string>()).Returns(owningCustomerUID.ToString());

			var devicePersonality = new List<DbDevicePersonality> { new DbDevicePersonality{
				fk_DeviceUID=input.DeviceUID.Value,fk_PersonalityTypeID= 0,PersonalityValue= "Test",PersonalityDesc= "Test"
				}, new DbDevicePersonality{
					fk_DeviceUID=input.DeviceUID.Value,fk_PersonalityTypeID= 1,PersonalityValue= "Test1",PersonalityDesc= "Test1" }
			};
			transaction.Get<DbDevicePersonality>(Arg.Any<string>()).Returns(devicePersonality);

			transaction.Get<DbDeviceType>(Arg.Any<string>()).Returns(deviceType);


			transaction.When(fake => fake.Upsert<DbDevicePersonality>(Arg.Any<List<DbDevicePersonality>>())).Throw(new Exception());
			transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				try
				{
					foreach (var action in x.Arg<List<Action>>())
					{
						action();
					}
					return true;
				}
				catch
				{
					return false;
				}

			});
			//Act
			var result = _deviceService.UpdateDevice(input, Arg.Any<DeviceStateEnum>());

			//Assert
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
			transaction.Received(1).Upsert(Arg.Any<DbDevice>());
			transaction.Received(1).Upsert<DbDevicePersonality>(Arg.Any<List<DbDevicePersonality>>());
			transaction.Received(0).Publish(Arg.Any<List<KafkaMessage>>());
			Assert.False(result);

		}

		[Theory]
		[InlineData("PLE641PLUSPL631")]
		[InlineData("PL121")]
		[InlineData("PL321")]
		public void TestUpdateDevice_KafkaFailure_Fail(string deviceTypeInput)
		{
			//Arrange
			var input = GetUpdateDeviceModel();
			input.DeviceType = deviceTypeInput;

			var owningCustomerUID = Guid.NewGuid();
			var deviceType = dbDeviceType.Where(x => x.TypeName == deviceTypeInput);

			var existingDevice = new DbDevice
			{
				DeviceUID = input.DeviceUID.Value,
				CellModemIMEI = input.CellModemIMEI,
				CellularFirmwarePartnumber = input.CellularFirmwarePartnumber,
				DataLinkType = input.DataLinkType,
				DeregisteredUTC = input.DeregisteredUTC,
				DevicePartNumber = input.DevicePartNumber,
				FirmwarePartNumber = input.FirmwarePartNumber,
				GatewayFirmwarePartNumber = input.GatewayFirmwarePartNumber,
				MainboardSoftwareVersion = input.MainboardSoftwareVersion,
				NetworkFirmwarePartnumber = input.NetworkFirmwarePartnumber,
				SatelliteFirmwarePartnumber = input.SatelliteFirmwarePartnumber,
				ModuleType = input.ModuleType,
				SerialNumber = input.DeviceSerialNumber,
				fk_DeviceStatusID = (int)(DeviceStateEnum)Enum.Parse(typeof(DeviceStateEnum), input.DeviceState).GetHashCode(),
				fk_DeviceTypeID = deviceType.First().DeviceTypeID
			};
			input.MainboardSoftwareVersion = "Changed";

			transaction.Get<DbDevice>(Arg.Any<string>()).Returns(new List<DbDevice> { existingDevice });
			transaction.GetValue(Arg.Any<string>()).Returns(owningCustomerUID.ToString());

			var devicePersonality = new List<DbDevicePersonality> { new DbDevicePersonality{
				fk_DeviceUID=input.DeviceUID.Value,fk_PersonalityTypeID= 0,PersonalityValue= "Test",PersonalityDesc= "Test"
				}, new DbDevicePersonality{
					fk_DeviceUID=input.DeviceUID.Value,fk_PersonalityTypeID= 1,PersonalityValue= "Test1",PersonalityDesc= "Test1" }
			};
			transaction.Get<DbDevicePersonality>(Arg.Any<string>()).Returns(devicePersonality);

			transaction.Get<DbDeviceType>(Arg.Any<string>()).Returns(deviceType);


			transaction.When(fake => fake.Publish(Arg.Any<List<KafkaMessage>>())).Throw(new Exception());
			transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				try
				{
					foreach (var action in x.Arg<List<Action>>())
					{
						action();
					}
					return true;
				}
				catch
				{
					return false;
				}

			});
			//Act
			var result = _deviceService.UpdateDevice(input, Arg.Any<DeviceStateEnum>());

			//Assert
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
			transaction.Received(1).Upsert(Arg.Any<DbDevice>());
			transaction.Received(1).Upsert<DbDevicePersonality>(Arg.Any<List<DbDevicePersonality>>());
			transaction.Received(1).Publish(Arg.Any<List<KafkaMessage>>());
			Assert.False(result);

		}

		[Theory]
		[InlineData("PLE641PLUSPL631")]
		[InlineData("PL121")]
		[InlineData("PL321")]
		public void TestUpdateDeviceProperties_ValidInput_SameDeviceProperties_NoUpdateCall_Success(string deviceTypeInput)
		{
			//Arrange
			var input = GetUpdateDevicePropertiesModel();
			var deviceGuid = Guid.NewGuid();

			input.DeviceType = deviceTypeInput;

			var owningCustomerUID = Guid.NewGuid();
			var deviceType = dbDeviceType.Where(x => x.TypeName == deviceTypeInput);

			var existingDevice = new DbDevice
			{
				DeviceUID = deviceGuid,
				CellModemIMEI = "TestCellModemIMEI",
				CellularFirmwarePartnumber = input.CellularFirmwarePartnumber,
				DataLinkType = input.DataLinkType,
				DeregisteredUTC = DateTime.Now,
				DevicePartNumber = "TestDevicePartNumber",
				FirmwarePartNumber = "TestFirmwarePartNumber",
				GatewayFirmwarePartNumber = input.GatewayFirmwarePartNumber,
				MainboardSoftwareVersion = input.MainboardSoftwareVersion,
				NetworkFirmwarePartnumber = input.NetworkFirmwarePartnumber,
				SatelliteFirmwarePartnumber = input.SatelliteFirmwarePartnumber,
				ModuleType = input.ModuleType,
				SerialNumber = input.DeviceSerialNumber,
				fk_DeviceStatusID = (int)(DeviceStateEnum)Enum.Parse(typeof(DeviceStateEnum), "Installed").GetHashCode(),
				fk_DeviceTypeID = deviceType.First().DeviceTypeID
			};

			transaction.Get<DbDevice>(Arg.Any<string>()).Returns(new List<DbDevice> { existingDevice });

			transaction.GetValue(Arg.Any<string>()).Returns(owningCustomerUID.ToString());

			//Act
			var result = _deviceService.UpdateDeviceProperties(input, deviceGuid);

			//Assert
			transaction.Received(0).Execute(Arg.Any<List<Action>>());
			transaction.Received(0).Upsert(Arg.Any<DbDevice>());
			transaction.Received(0).Upsert<DbDevicePersonality>(Arg.Any<List<DbDevicePersonality>>());
			transaction.Received(0).Publish(Arg.Any<List<KafkaMessage>>());
			Assert.False(result);
		}


		[Theory]
		[InlineData("PLE641PLUSPL631", "MainboardSoftwareVersion")]
		[InlineData("PLE641PLUSPL631", "GatewayFirmwarePartNumber")]
		[InlineData("PLE641PLUSPL631", "DataLinkType")]
		[InlineData("PLE641PLUSPL631", "CellularFirmwarePartnumber")]
		[InlineData("PLE641PLUSPL631", "NetworkFirmwarePartnumber")]
		[InlineData("PLE641PLUSPL631", "SatelliteFirmwarePartnumber")]
		[InlineData("PLE641PLUSPL631", "ModuleType")]
		[InlineData("PLE641PLUSPL631", "Description")]
		[InlineData("PL121", "MainboardSoftwareVersion")]
		[InlineData("PL121", "GatewayFirmwarePartNumber")]
		[InlineData("PL121", "DataLinkType")]
		[InlineData("PL121", "NetworkFirmwarePartnumber")]
		[InlineData("PL121", "SatelliteFirmwarePartnumber")]
		[InlineData("PL121", "ModuleType")]
		[InlineData("PL121", "Description")]
		public void TestUpdateDeviceProperties_ValidInput_DiffrentDeviceProperties_UpdateCall_Success(string deviceTypeInput, string propertyNameToChangeValue)
		{
			//Arrange
			var input = GetUpdateDevicePropertiesModel();
			input.DeviceType = deviceTypeInput;
			var deviceGuid = Guid.NewGuid();

			var owningCustomerUID = Guid.NewGuid();
			var deviceType = dbDeviceType.Where(x => x.TypeName == deviceTypeInput);

			var existingDevice = new DbDevice
			{
				DeviceUID = deviceGuid,
				CellModemIMEI = "TestCellModemIMEI",
				CellularFirmwarePartnumber = input.CellularFirmwarePartnumber,
				DataLinkType = input.DataLinkType,
				DeregisteredUTC = DateTime.Now,
				DevicePartNumber = "TestDevicePartNumber",
				FirmwarePartNumber = "TestFirmwarePartNumber",
				GatewayFirmwarePartNumber = input.GatewayFirmwarePartNumber,
				MainboardSoftwareVersion = input.MainboardSoftwareVersion,
				NetworkFirmwarePartnumber = input.NetworkFirmwarePartnumber,
				SatelliteFirmwarePartnumber = input.SatelliteFirmwarePartnumber,
				ModuleType = input.ModuleType,
				SerialNumber = input.DeviceSerialNumber,
				fk_DeviceStatusID = (int)(DeviceStateEnum)Enum.Parse(typeof(DeviceStateEnum), "Installed").GetHashCode(),
				fk_DeviceTypeID = deviceType.First().DeviceTypeID
			};

			Type myType = typeof(UpdateDeviceProperties);
			PropertyInfo myPropInfo = myType.GetProperty(propertyNameToChangeValue);
			myPropInfo.SetValue(input, "Changed", null);// it should call update & publish

			transaction.Get<DbDevice>(Arg.Any<string>()).Returns(new List<DbDevice> { existingDevice });
			transaction.GetValue(Arg.Any<string>()).Returns(owningCustomerUID.ToString());

			var devicePersonality = new List<DbDevicePersonality> { new DbDevicePersonality{
				fk_DeviceUID=deviceGuid,fk_PersonalityTypeID= 0,PersonalityValue= "Test",PersonalityDesc= "Test"
				}, new DbDevicePersonality{
					fk_DeviceUID=deviceGuid,fk_PersonalityTypeID= 1,PersonalityValue= "Test1",PersonalityDesc= "Test1" }
			};
			transaction.Get<DbDevicePersonality>(Arg.Any<string>()).Returns(devicePersonality);
			//var deviceType = dbDeviceType.Where(x => x.TypeName == "PLE641PLUSPL631");
			transaction.Get<DbDeviceType>(Arg.Any<string>()).Returns(deviceType);
			var asset = new AssetDto { AssetUID = Guid.NewGuid(), OwningCustomerUID = owningCustomerUID };
			transaction.Get<AssetDto>(Arg.Any<string>()).Returns(new List<AssetDto> { asset });

			transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				foreach (var action in x.Arg<List<Action>>())
				{
					action();
				}
				return true;
			});

			//Act
			var result = _deviceService.UpdateDeviceProperties(input, deviceGuid);

			//Assert
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
			transaction.Received(1).Upsert(Arg.Any<DbDevice>());
			transaction.Received(1).Upsert<DbDevicePersonality>(Arg.Any<List<DbDevicePersonality>>());
			transaction.Received(1).Publish(Arg.Any<List<KafkaMessage>>());
			Assert.True(result);
		}

		[Theory]
		[InlineData("PLE641PLUSPL631")]
		[InlineData("PL121")]
		[InlineData("PL321")]
		public void TestUpdateDeviceProperties_DbFailure_NoCalltoPublish(string deviceTypeInput)
		{
			//Arrange
			var input = GetUpdateDevicePropertiesModel();
			var deviceGuid = Guid.NewGuid();

			input.DeviceType = deviceTypeInput;

			var owningCustomerUID = Guid.NewGuid();
			var deviceType = dbDeviceType.Where(x => x.TypeName == deviceTypeInput);

			var existingDevice = new DbDevice
			{
				DeviceUID = deviceGuid,
				CellModemIMEI = "TestCellModemIMEI",
				CellularFirmwarePartnumber = input.CellularFirmwarePartnumber,
				DataLinkType = input.DataLinkType,
				DeregisteredUTC = DateTime.Now,
				DevicePartNumber = "TestDevicePartNumber",
				FirmwarePartNumber = "TestFirmwarePartNumber",
				GatewayFirmwarePartNumber = input.GatewayFirmwarePartNumber,
				MainboardSoftwareVersion = input.MainboardSoftwareVersion,
				NetworkFirmwarePartnumber = input.NetworkFirmwarePartnumber,
				SatelliteFirmwarePartnumber = input.SatelliteFirmwarePartnumber,
				ModuleType = input.ModuleType,
				SerialNumber = input.DeviceSerialNumber,
				fk_DeviceStatusID = (int)(DeviceStateEnum)Enum.Parse(typeof(DeviceStateEnum), "Installed").GetHashCode(),
				fk_DeviceTypeID = deviceType.First().DeviceTypeID
			};

			input.MainboardSoftwareVersion = "Changed";

			transaction.Get<DbDevice>(Arg.Any<string>()).Returns(new List<DbDevice> { existingDevice });
			transaction.GetValue(Arg.Any<string>()).Returns(owningCustomerUID.ToString());

			var devicePersonality = new List<DbDevicePersonality> { new DbDevicePersonality{
				fk_DeviceUID=deviceGuid,fk_PersonalityTypeID= 0,PersonalityValue= "Test",PersonalityDesc= "Test"
				}, new DbDevicePersonality{
					fk_DeviceUID=deviceGuid,fk_PersonalityTypeID= 1,PersonalityValue= "Test1",PersonalityDesc= "Test1" }
			};
			transaction.Get<DbDevicePersonality>(Arg.Any<string>()).Returns(devicePersonality);

			transaction.Get<DbDeviceType>(Arg.Any<string>()).Returns(deviceType);


			transaction.When(fake => fake.Upsert<DbDevicePersonality>(Arg.Any<List<DbDevicePersonality>>())).Throw(new Exception());
			transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				try
				{
					foreach (var action in x.Arg<List<Action>>())
					{
						action();
					}
					return true;
				}
				catch
				{
					return false;
				}

			});
			//Act
			var result = _deviceService.UpdateDeviceProperties(input, deviceGuid);

			//Assert
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
			transaction.Received(1).Upsert(Arg.Any<DbDevice>());
			transaction.Received(1).Upsert<DbDevicePersonality>(Arg.Any<List<DbDevicePersonality>>());
			transaction.Received(0).Publish(Arg.Any<List<KafkaMessage>>());
			Assert.False(result);

		}

		[Theory]
		[InlineData("PLE641PLUSPL631")]
		[InlineData("PL121")]
		[InlineData("PL321")]
		public void TestUpdateDeviceProperties_KafkaFailure_Fail(string deviceTypeInput)
		{
			//Arrange
			var input = GetUpdateDevicePropertiesModel();
			var deviceGuid = Guid.NewGuid();

			input.DeviceType = deviceTypeInput;

			var owningCustomerUID = Guid.NewGuid();
			var deviceType = dbDeviceType.Where(x => x.TypeName == deviceTypeInput);

			var existingDevice = new DbDevice
			{
				DeviceUID = deviceGuid,
				CellModemIMEI = "TestCellModemIMEI",
				CellularFirmwarePartnumber = input.CellularFirmwarePartnumber,
				DataLinkType = input.DataLinkType,
				DeregisteredUTC = DateTime.Now,
				DevicePartNumber = "TestDevicePartNumber",
				FirmwarePartNumber = "TestFirmwarePartNumber",
				GatewayFirmwarePartNumber = input.GatewayFirmwarePartNumber,
				MainboardSoftwareVersion = input.MainboardSoftwareVersion,
				NetworkFirmwarePartnumber = input.NetworkFirmwarePartnumber,
				SatelliteFirmwarePartnumber = input.SatelliteFirmwarePartnumber,
				ModuleType = input.ModuleType,
				SerialNumber = input.DeviceSerialNumber,
				fk_DeviceStatusID = (int)(DeviceStateEnum)Enum.Parse(typeof(DeviceStateEnum), "Installed").GetHashCode(),
				fk_DeviceTypeID = deviceType.First().DeviceTypeID
			};

			input.MainboardSoftwareVersion = "Changed";

			transaction.Get<DbDevice>(Arg.Any<string>()).Returns(new List<DbDevice> { existingDevice });
			transaction.GetValue(Arg.Any<string>()).Returns(owningCustomerUID.ToString());

			var devicePersonality = new List<DbDevicePersonality> { new DbDevicePersonality{
				fk_DeviceUID=deviceGuid,fk_PersonalityTypeID= 0,PersonalityValue= "Test",PersonalityDesc= "Test"
				}, new DbDevicePersonality{
					fk_DeviceUID=deviceGuid,fk_PersonalityTypeID= 1,PersonalityValue= "Test1",PersonalityDesc= "Test1" }
			};
			transaction.Get<DbDevicePersonality>(Arg.Any<string>()).Returns(devicePersonality);

			transaction.Get<DbDeviceType>(Arg.Any<string>()).Returns(deviceType);


			transaction.When(fake => fake.Publish(Arg.Any<List<KafkaMessage>>())).Throw(new Exception());
			transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				try
				{
					foreach (var action in x.Arg<List<Action>>())
					{
						action();
					}
					return true;
				}
				catch
				{
					return false;
				}

			});
			//Act
			var result = _deviceService.UpdateDeviceProperties(input, deviceGuid);

			//Assert
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
			transaction.Received(1).Upsert(Arg.Any<DbDevice>());
			transaction.Received(1).Upsert<DbDevicePersonality>(Arg.Any<List<DbDevicePersonality>>());
			transaction.Received(1).Publish(Arg.Any<List<KafkaMessage>>());
			Assert.False(result);

		}
		[Theory]
		[InlineData("PLE641PLUSPL631")]
		[InlineData("PL121")]
		[InlineData("PL321")]
		public void TestGetExistingDeviceProperties_Success(string deviceTypeInput)
		{
			//Arrange
			var input = GetUpdateDevicePropertiesModel();
			var deviceGuid = Guid.NewGuid();

			input.DeviceType = deviceTypeInput;
			var deviceType = dbDeviceType.Where(x => x.TypeName == deviceTypeInput);

			var existingDevice = new DbDevice
			{
				DeviceUID = deviceGuid,
				CellModemIMEI = "TestCellModemIMEI",
				CellularFirmwarePartnumber = input.CellularFirmwarePartnumber,
				DataLinkType = input.DataLinkType,
				DeregisteredUTC = DateTime.Now,
				DevicePartNumber = "TestDevicePartNumber",
				FirmwarePartNumber = "TestFirmwarePartNumber",
				GatewayFirmwarePartNumber = input.GatewayFirmwarePartNumber,
				MainboardSoftwareVersion = input.MainboardSoftwareVersion,
				NetworkFirmwarePartnumber = input.NetworkFirmwarePartnumber,
				SatelliteFirmwarePartnumber = input.SatelliteFirmwarePartnumber,
				ModuleType = input.ModuleType,
				SerialNumber = input.DeviceSerialNumber,
				fk_DeviceStatusID = (int)(DeviceStateEnum)Enum.Parse(typeof(DeviceStateEnum), "Installed").GetHashCode(),
				fk_DeviceTypeID = deviceType.First().DeviceTypeID
			};

			transaction.Get<DbDevice>(Arg.Any<string>()).Returns(new List<DbDevice> { existingDevice });

			var result = _deviceService.GetExistingDeviceProperties(deviceGuid);

			//Assert
			Assert.Equal(deviceType.First().TypeName, result.DeviceType);
			Assert.Equal("Installed", result.DeviceState);
			Assert.Equal(deviceType.First().TypeName, result.DeviceType);
			//if (!deviceType.First().TypeName.Equals("PLE641PLUSPL631"))
			//{
			//	Assert.Null(result.FirmwarePartNumber);
			//}

			transaction.Received(1).Get<DbDevice>(Arg.Any<string>());
		}

		[Theory]
		[InlineData("PLE641PLUSPL631")]
		[InlineData("PL121")]
		[InlineData("PL321")]
		public void TestGetExistingDeviceProperties_NoDevice(string deviceTypeInput)
		{
			//Arrange
			var input = GetUpdateDevicePropertiesModel();
			var deviceGuid = Guid.NewGuid();

			input.DeviceType = deviceTypeInput;
			var deviceType = dbDeviceType.Where(x => x.TypeName == deviceTypeInput);

			transaction.Get<DbDevice>(Arg.Any<string>()).Returns(new List<DbDevice> { null });

			var result = _deviceService.GetExistingDeviceProperties(deviceGuid);

			//Assert
			Assert.Null(result);
			transaction.Received(1).Get<DbDevice>(Arg.Any<string>());
		}

		[Fact]
		public void TestAssociateDeviceAsset_NewAssetDeviceAssociation()
		{
			//Arrange
			AssociateDeviceAssetEvent input = new AssociateDeviceAssetEvent
			{
				DeviceUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			transaction.Get<DeviceDto>(Arg.Any<string>()).Returns(new List<DeviceDto> { null });
			transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				try
				{
					foreach (var action in x.Arg<List<Action>>())
					{
						action();
					}
					return true;
				}
				catch
				{
					return false;
				}

			});

			//Act
			var result = _deviceService.AssociateAssetDevice(input);

			//Assert
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
			transaction.Received(1).Publish(Arg.Any<List<KafkaMessage>>());
			transaction.Received(1).Publish(Arg.Is<List<KafkaMessage>>(x => x.Count == 1)); // only association event
			Assert.True(result);
		}



		[Fact]
		public void TestAssociateDeviceAsset_ThrowDBException()
		{
			//Arrange
			AssociateDeviceAssetEvent input = new AssociateDeviceAssetEvent
			{
				DeviceUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var deviceFromDB = new DeviceDto { DeviceUID = input.DeviceUID, DeviceStatusID = DeviceStateEnum.Installed.GetHashCode() };
			transaction.Get<DeviceDto>(Arg.Any<string>()).Returns(new List<DeviceDto> { deviceFromDB });
			transaction.When(fake => fake.Publish(Arg.Any<List<KafkaMessage>>())).Throw(new Exception());
			transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				try
				{
					foreach (var action in x.Arg<List<Action>>())
					{
						action();
					}
					return true;
				}
				catch
				{
					return false;
				}

			});

			//Act
			var result = _deviceService.AssociateAssetDevice(input);

			//Assert
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
			transaction.Received(1).Publish(Arg.Any<List<KafkaMessage>>());
			Assert.False(result);
		}

		[Fact]
		public void TestDissociateDeviceAsset_Valid()
		{
			//Arrange
			DissociateDeviceAssetEvent input = new DissociateDeviceAssetEvent
			{
				DeviceUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				try
				{
					foreach (var action in x.Arg<List<Action>>())
					{
						action();
					}
					return true;
				}
				catch
				{
					return false;
				}

			});

			//Act
			var result = _deviceService.DissociateAssetDevice(input);

			//Assert
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
			transaction.Received(1).Publish(Arg.Any<List<KafkaMessage>>());
			transaction.Received(1).Publish(Arg.Is<List<KafkaMessage>>(x => x.Count == 1)); // only dissociation event
			Assert.True(result);
		}



		[Fact]
		public void TestDissociateDeviceAssetEvent_ThrowDBException()
		{
			//Arrange
			DissociateDeviceAssetEvent input = new DissociateDeviceAssetEvent
			{
				DeviceUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};


			transaction.When(fake => fake.Publish(Arg.Any<List<KafkaMessage>>())).Throw(new Exception());
			transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				try
				{
					foreach (var action in x.Arg<List<Action>>())
					{
						action();
					}
					return true;
				}
				catch
				{
					return false;
				}

			});

			//Act
			var result = _deviceService.DissociateAssetDevice(input);

			//Assert
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
			transaction.Received(1).Publish(Arg.Any<List<KafkaMessage>>());
			Assert.False(result);
		}

		private List<DbDeviceType> GetDeviceTypes()
		{
			var deviceTypes = new List<DbDeviceType>{
				new DbDeviceType { TypeName = "MANUALDEVICE", DeviceTypeID = 0, fk_DeviceTypeFamilyID = 4, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""}]" },
new DbDeviceType { TypeName = "PL121", DeviceTypeID = 1, fk_DeviceTypeFamilyID = 1, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""RadioFirmwarePartNumber"",""DevicePersonalityTypeID"": ""8""}]" },
new DbDeviceType { TypeName = "PL321", DeviceTypeID = 2, fk_DeviceTypeFamilyID = 1, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""GatewayFirmwarePartNumber"",""DevicePersonalityTypeID"": ""3""},{""PersonalityTypeName"": ""RadioFirmwarePartNumber"",""DevicePersonalityTypeID"": ""8""}]" },
new DbDeviceType { TypeName = "Series522", DeviceTypeID = 3, fk_DeviceTypeFamilyID = 2, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""GatewayFirmwarePartNumber"",""DevicePersonalityTypeID"": ""3""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""},{""PersonalityTypeName"": ""DevicePartNumber"",""DevicePersonalityTypeID"": ""0""}]" },
new DbDeviceType { TypeName = "Series523", DeviceTypeID = 4, fk_DeviceTypeFamilyID = 2, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""GatewayFirmwarePartNumber"",""DevicePersonalityTypeID"": ""3""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""},{""PersonalityTypeName"": ""DevicePartNumber"",""DevicePersonalityTypeID"": ""0""}]" },
new DbDeviceType { TypeName = "Series521", DeviceTypeID = 5, fk_DeviceTypeFamilyID = 2, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""GatewayFirmwarePartNumber"",""DevicePersonalityTypeID"": ""3""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""},{""PersonalityTypeName"": ""DevicePartNumber"",""DevicePersonalityTypeID"": ""0""}]" },
new DbDeviceType { TypeName = "SNM940", DeviceTypeID = 6, fk_DeviceTypeFamilyID = 2, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""}]" },
new DbDeviceType { TypeName = "CrossCheck", DeviceTypeID = 7, fk_DeviceTypeFamilyID = 2, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""}]" },
new DbDeviceType { TypeName = "TrimTrac", DeviceTypeID = 8, fk_DeviceTypeFamilyID = 2, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""}]" },
new DbDeviceType { TypeName = "PL420", DeviceTypeID = 9, fk_DeviceTypeFamilyID = 2, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""}]" },
new DbDeviceType { TypeName = "PL421", DeviceTypeID = 10, fk_DeviceTypeFamilyID = 2, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""}]" },
new DbDeviceType { TypeName = "TM3000", DeviceTypeID = 11, fk_DeviceTypeFamilyID = 2, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""}]" },
new DbDeviceType { TypeName = "TAP66", DeviceTypeID = 12, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""}]" },
new DbDeviceType { TypeName = "SNM451", DeviceTypeID = 13, fk_DeviceTypeFamilyID = 2, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""}]" },
new DbDeviceType { TypeName = "PL431", DeviceTypeID = 14, fk_DeviceTypeFamilyID = 2, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""}]" },
new DbDeviceType { TypeName = "DCM300", DeviceTypeID = 15, fk_DeviceTypeFamilyID = 2, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""}]" },
new DbDeviceType { TypeName = "PL641", DeviceTypeID = 16, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "PLE641", DeviceTypeID = 17, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "PLE641PLUSPL631", DeviceTypeID = 18, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "PLE631", DeviceTypeID = 19, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "PL631", DeviceTypeID = 20, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "PL241", DeviceTypeID = 21, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "PL231", DeviceTypeID = 22, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "BasicVirtualDevice", DeviceTypeID = 23, fk_DeviceTypeFamilyID = 4, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""}]" },
new DbDeviceType { TypeName = "MTHYPHEN10", DeviceTypeID = 24, fk_DeviceTypeFamilyID = 4, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""}]" },
new DbDeviceType { TypeName = "XT5060", DeviceTypeID = 25, fk_DeviceTypeFamilyID = 4, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""}]" },
new DbDeviceType { TypeName = "XT4860", DeviceTypeID = 26, fk_DeviceTypeFamilyID = 4, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""}]" },
new DbDeviceType { TypeName = "TTUSeries", DeviceTypeID = 27, fk_DeviceTypeFamilyID = 4, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""}]" },
new DbDeviceType { TypeName = "XT2000", DeviceTypeID = 28, fk_DeviceTypeFamilyID = 4, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""}]" },
new DbDeviceType { TypeName = "MTGModularGatewayHYPHENMotorEngine", DeviceTypeID = 29, fk_DeviceTypeFamilyID = 4, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""}]" },
new DbDeviceType { TypeName = "MTGModularGatewayHYPHENElectricEngine", DeviceTypeID = 30, fk_DeviceTypeFamilyID = 4, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""}]" },
new DbDeviceType { TypeName = "MCHYPHEN3", DeviceTypeID = 31, fk_DeviceTypeFamilyID = 4, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""}]" },
new DbDeviceType { TypeName = "Dummy", DeviceTypeID = 32, fk_DeviceTypeFamilyID = 4, DefaultValueJson = @"-" },
new DbDeviceType { TypeName = "XT6540", DeviceTypeID = 33, fk_DeviceTypeFamilyID = 4, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""}]" },
new DbDeviceType { TypeName = "XT65401", DeviceTypeID = 34, fk_DeviceTypeFamilyID = 4, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""}]" },
new DbDeviceType { TypeName = "XT65402", DeviceTypeID = 35, fk_DeviceTypeFamilyID = 4, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""}]" },
new DbDeviceType { TypeName = "THREEPDATA", DeviceTypeID = 36, fk_DeviceTypeFamilyID = 4, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""}]" },
new DbDeviceType { TypeName = "PL131", DeviceTypeID = 37, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""}]" },
new DbDeviceType { TypeName = "PL141", DeviceTypeID = 38, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""}]" },
new DbDeviceType { TypeName = "PL440", DeviceTypeID = 39, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""}]" },
new DbDeviceType { TypeName = "PLE601", DeviceTypeID = 40, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""}]" },
new DbDeviceType { TypeName = "PL161", DeviceTypeID = 41, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""}]" },
new DbDeviceType { TypeName = "PL240", DeviceTypeID = 42, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "PL542", DeviceTypeID = 43, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""}]" },
new DbDeviceType { TypeName = "PLE642", DeviceTypeID = 44, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "PLE742", DeviceTypeID = 45, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "SNM941", DeviceTypeID = 46, fk_DeviceTypeFamilyID = 2, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion""},{""PersonalityTypeName"": ""CellModemIMEI""}]" },
new DbDeviceType { TypeName = "PL240B", DeviceTypeID = 47, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "TAP76", DeviceTypeID = 48, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""}]" },
new DbDeviceType { TypeName = "PLE732", DeviceTypeID = 49, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "PLE782", DeviceTypeID = 50, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "PL243", DeviceTypeID = 51, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "PLE643", DeviceTypeID = 52, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "PLE783", DeviceTypeID = 53, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "PLE743", DeviceTypeID = 54, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "PLE683", DeviceTypeID = 55, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""NetworkFirmwarePartnumber"",""DevicePersonalityTypeID"": ""5""},{""PersonalityTypeName"": ""CellularFirmwarePartnumber"",""DevicePersonalityTypeID"": ""6""},{""PersonalityTypeName"": ""SatelliteFirmwarePartnumber"",""DevicePersonalityTypeID"": ""7""}]" },
new DbDeviceType { TypeName = "EC520", DeviceTypeID = 56, fk_DeviceTypeFamilyID = 3, DefaultValueJson = @"[{""PersonalityTypeName"": ""FirmwarePartNumber"",""DevicePersonalityTypeID"": ""1""},{""PersonalityTypeName"": ""MainboardSoftwareVersion"",""DevicePersonalityTypeID"": ""2""},{""PersonalityTypeName"": ""CellModemIMEI"",""DevicePersonalityTypeID"": ""4""}]" },

	};
			return deviceTypes;
		}
	}
}