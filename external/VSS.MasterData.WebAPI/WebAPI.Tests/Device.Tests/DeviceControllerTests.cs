using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.ClientModel.Device;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.DbModel.Device;
using VSS.MasterData.WebAPI.Device.Controllers.V1;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Interfaces.Device;
using VSS.MasterData.WebAPI.KafkaModel.Device;
using Xunit;

namespace VSP.MasterData.Device.UnitTests
{
	public class DeviceControllerTests
	{
		private readonly DeviceV1Controller _target;
		private readonly IDeviceService _deviceService;
		private readonly ILogger logger;
		private readonly IDeviceTypeService _deviceTypeService;
		private List<DbDeviceType> dbDeviceType = new List<DbDeviceType>();

		ContainerBuilder _builder;
		private const string HeaderTokenJwt =
			"eyJ0eXAiOiJKV1QiLCJhbGciOiJTSEEyNTZ3aXRoUlNBIiwieDV0IjoiWW1FM016UTRNVFk0TkRVMlpEWm1PRGRtTlRSbU4yWmxZVGt3TVdFelltTmpNVGt6TURFelpnPT0ifQ==.eyJpc3MiOiJ3c28yLm9yZy9wcm9kdWN0cy9hbSIsImV4cCI6MTQ0ODAwNDEyODIyNCwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9zdWJzY3JpYmVyIjoiZGV2LXZzc2FkbWluQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbmlkIjoiNTYwIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbm5hbWUiOiJWU1AtTkRldiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBwbGljYXRpb250aWVyIjoiIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcGljb250ZXh0IjoiL3QvdHJpbWJsZS5jb20vdnNwLXFhLWlkZW50aXR5YXBpIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy92ZXJzaW9uIjoidjEiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3RpZXIiOiJVbmxpbWl0ZWQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2tleXR5cGUiOiJQUk9EVUNUSU9OIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy91c2VydHlwZSI6IkFQUExJQ0FUSU9OX1VTRVIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJkZXYtdmxjbGFzc2ljdXNlckB0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW5kdXNlclRlbmFudElkIjoiMSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW1haWxhZGRyZXNzIjoiQmhvb2JhbGFuX1BhbGFuaXZlbEBUcmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZmlyc3RuYW1lIjoiQmhvb2JhbGFuIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9naXZlbm5hbWUiOiJCaG9vYmFsYW4iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2lkZW50aXR5L2FjY291bnRMb2NrZWQiOiJmYWxzZSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdG5hbWUiOiJQYWxhbml2ZWwiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3JvbGUiOiIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3V1aWQiOiJhYTVkYTYyNy04OWVlLTRlYmUtYTE4Mi1hNzE3NzA1YWZmZDAifQ==.DLa86MVuE2nCuzLGCqgcw20/q5ikODPgDDwLiPO78RQpKwlqOG5Poa5gU2cyEYyDnYMiYW1M3Ffjh2icCBpnediyG2b5b9iNEHvBq+Y3Kiuc3qINva7fqU2Z1hk1afw+NvmQoYO9qVDXr8QXNUQNMEIRheJWQq0PMM+VD2IViEU=";

		public DeviceControllerTests()
		{
			_deviceTypeService = Substitute.For<IDeviceTypeService>();

			logger = Substitute.For<ILogger>();
			_deviceService = Substitute.For<IDeviceService>();
			_builder = new ContainerBuilder();
			var container = _builder.Build();
			var deviceTypesDictionary = new Dictionary<string, DbDeviceType>(StringComparer.OrdinalIgnoreCase);
			dbDeviceType = GetDeviceTypes();
			dbDeviceType.ForEach(x =>
			{
				deviceTypesDictionary.Add(x.TypeName, x);
			});
			_deviceTypeService.GetDeviceType().Returns(deviceTypesDictionary);
			_builder.Register(config => new DeviceV1Controller(_deviceService, logger, _deviceTypeService)).As<DeviceV1Controller>();
			_target = CreateControllerWithHeader(container, new ControllerContext(), logger, _deviceService, _deviceTypeService); // new DeviceV1Controller(_deviceService, logger);

		}



		private static DeviceV1Controller CreateControllerWithHeader(IContainer container, ControllerContext mockHttpContext, ILogger logger, IDeviceService _deviceService,IDeviceTypeService deviceTypeService)
		{
			var controller = new DeviceV1Controller(_deviceService, logger, deviceTypeService);
			controller.ControllerContext = mockHttpContext;
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Request.Headers["X-JWT-Assertion"] = HeaderTokenJwt;
			return controller;
		}

		[Fact]
		public void TestCreateDevice_ValidInput_Success()
		{
			//Arrange
			var input = new CreateDeviceEvent
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

			_deviceService.CheckExistingDevice(input.DeviceUID.Value).Returns(false);
			_deviceService.CreateDevice(input, Arg.Any<DeviceStateEnum>()).Returns(true);

			//Act
			var result = _target.CreateDevice(input);

			//Assert
			Assert.Equal(typeof(OkResult), result.GetType());
			_deviceService.Received(1).CreateDevice(ValidateDevicePayload(input.DeviceUID.Value, input.DeviceSerialNumber, input.DeviceType), Enum.Parse<DeviceStateEnum>(input.DeviceState.Trim(), true));

		}

		[Fact]
		public void TestCreateDevice_ValidInput_ExistingDevice_Fail()
		{
			//Arrange
			var input = new CreateDeviceEvent
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

			_deviceService.CheckExistingDevice(input.DeviceUID.Value).Returns(true);
			_deviceService.CreateDevice(input, Arg.Any<DeviceStateEnum>()).Returns(false);

			//Act
			var result = _target.CreateDevice(input);

			//Assert
			Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
			_deviceService.Received(0).CreateDevice(ValidateDevicePayload(input.DeviceUID.Value, input.DeviceSerialNumber, input.DeviceType), Enum.Parse<DeviceStateEnum>(input.DeviceState.Trim(), true));

		}

		[Theory]
		[InlineData("")]
		public void TestCreateDevice_InvalidInput_EmptyDeviceSerialNumber_Allow(string deviceSerialNumber)
		{
			//Arrange
			var input = new CreateDeviceEvent
			{
				DeviceUID = Guid.NewGuid(),
				DeviceType = "PLE641PLUSPL631",
				DeviceSerialNumber = "TestSerialNumber",
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
			input.DeviceSerialNumber = deviceSerialNumber;

			_deviceService.CheckExistingDevice(input.DeviceUID.Value).Returns(false);
			_deviceService.CreateDevice(input, Arg.Any<DeviceStateEnum>()).Returns(false);

			//Act
			var result = _target.CreateDevice(input);

			//Assert
			Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
			_deviceService.Received(1).CreateDevice(ValidateDevicePayload(input.DeviceUID.Value, input.DeviceSerialNumber, input.DeviceType), Enum.Parse<DeviceStateEnum>(input.DeviceState.Trim(), true));

		}

		[Theory]
		[InlineData(null)]
		public void TestCreateDevice_InvalidInput_NULLDeviceSerialNumber_Fail(string deviceSerialNumber)
		{
			//Arrange
			var input = new CreateDeviceEvent
			{
				DeviceUID = Guid.NewGuid(),
				DeviceType = "PLE641PLUSPL631",
				DeviceSerialNumber = "TestSerialNumber",
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
			input.DeviceSerialNumber = deviceSerialNumber;

			_deviceService.CheckExistingDevice(input.DeviceUID.Value).Returns(false);
			_deviceService.CreateDevice(input, Arg.Any<DeviceStateEnum>()).Returns(false);

			//Act
			var result = _target.CreateDevice(input);

			//Assert
			Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
			_deviceService.Received(0).CreateDevice(ValidateDevicePayload(input.DeviceUID.Value, input.DeviceSerialNumber, input.DeviceType), Enum.Parse<DeviceStateEnum>(input.DeviceState.Trim(), true));

		}

		[Theory]
		[InlineData("")]
		[InlineData(null)]
		[InlineData("Dummy")]
		public void TestCreateDevice_InvalidInput_InvalidDeviceState_Fail(string deviceState)
		{
			//Arrange
			var input = new CreateDeviceEvent
			{
				DeviceUID = Guid.NewGuid(),
				DeviceType = "PLE641PLUSPL631",
				DeviceState = deviceState,
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
				ReceivedUTC = DateTime.UtcNow,
				DeviceSerialNumber = "Test"
			};

			_deviceService.CheckExistingDevice(input.DeviceUID.Value).Returns(false);
			_deviceService.CreateDevice(input, Arg.Any<DeviceStateEnum>()).Returns(false);

			//Act
			var result = _target.CreateDevice(input);

			//Assert
			Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
			_deviceService.Received(0).CreateDevice(ValidateDevicePayload(input.DeviceUID.Value, input.DeviceSerialNumber, input.DeviceType), Arg.Any<DeviceStateEnum>());
		}

		[Fact]
		public void TestCreateDevice_ExceptionFailure_ReturnsServerError()
		{
			//Arrange
			var input = new CreateDeviceEvent
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
			_deviceService.CheckExistingDevice(input.DeviceUID.Value).Returns(false);
			_deviceService.CreateDevice(input, Arg.Any<DeviceStateEnum>()).Returns(x => { throw new Exception(); });

			//Act
			var result = _target.CreateDevice(input);

			//Assert
			_deviceService.Received(1).CreateDevice(ValidateDevicePayload(input.DeviceUID.Value, input.DeviceSerialNumber, input.DeviceType), Enum.Parse<DeviceStateEnum>(input.DeviceState.Trim(), true));
			Assert.Equal(500, ((ObjectResult)result).StatusCode);
		}

		[Fact]
		public void TestUpdateDevice_ValidInput_Success()
		{
			//Arrange
			var input = new UpdateDeviceEvent
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
				DataLinkType = "J1939",
				FirmwarePartNumber = "TestFirmwarePartNumber",
				CellModemIMEI = "TestCellModemIMEI",
				DevicePartNumber = "TestDevicePartNumber",
				CellularFirmwarePartnumber = "TestCellularFirmwarePartnumber",
				NetworkFirmwarePartnumber = "TestNetworkFirmwarePartnumber",
				SatelliteFirmwarePartnumber = "TestSatelliteFirmwarePartnumber",
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			_deviceService.CheckExistingDevice(input.DeviceUID.Value).Returns(true);
			_deviceService.UpdateDevice(input, Arg.Any<DeviceStateEnum>()).Returns(true);

			//Act
			var result = _target.UpdateDevice(input);

			//Assert
			_deviceService.Received(1).UpdateDevice(Arg.Any<UpdateDeviceEvent>(), Enum.Parse<DeviceStateEnum>(input.DeviceState.Trim(), true));
			Assert.Equal(typeof(OkObjectResult), result.GetType());

		}

		[Fact]
		public void TestUpdateDevice_ValidInput_NotExistsDevice_Fail()
		{
			//Arrange
			var input = new UpdateDeviceEvent
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
				DataLinkType = "J1939",
				FirmwarePartNumber = "TestFirmwarePartNumber",
				CellModemIMEI = "TestCellModemIMEI",
				DevicePartNumber = "TestDevicePartNumber",
				CellularFirmwarePartnumber = "TestCellularFirmwarePartnumber",
				NetworkFirmwarePartnumber = "TestNetworkFirmwarePartnumber",
				SatelliteFirmwarePartnumber = "TestSatelliteFirmwarePartnumber",
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			_deviceService.CheckExistingDevice(input.DeviceUID.Value).Returns(false);
			_deviceService.UpdateDevice(input, Arg.Any<DeviceStateEnum>()).Returns(false);

			//Act
			var result = _target.UpdateDevice(input);

			//Assert
			_deviceService.Received(0).UpdateDevice(Arg.Any<UpdateDeviceEvent>(), Enum.Parse<DeviceStateEnum>(input.DeviceState.Trim(), true));
			Assert.Equal(typeof(BadRequestObjectResult), result.GetType());

		}

		[Theory]
		[InlineData("")]
		[InlineData("$#$#$")]
		[InlineData(null)]
		[InlineData("Dummy")]
		public void TestUpdateDevice_InvalidDeviceState_NotExistsDevice_Fail(string deviceState)
		{
			//Arrange
			var input = new UpdateDeviceEvent
			{
				DeviceUID = Guid.NewGuid(),
				DeviceSerialNumber = "TestSerialNumber",
				DeviceType = "PLE641PLUSPL631",
				DeviceState = deviceState,
				DeregisteredUTC = DateTime.UtcNow,
				ModuleType = "TestModuleType",
				MainboardSoftwareVersion = "TestMainboardSoftwareVersion",
				RadioFirmwarePartNumber = "TestRadioSoftwareVersion",
				GatewayFirmwarePartNumber = "TestGatewaySoftwareVersion",
				DataLinkType = "J1939",
				FirmwarePartNumber = "TestFirmwarePartNumber",
				CellModemIMEI = "TestCellModemIMEI",
				DevicePartNumber = "TestDevicePartNumber",
				CellularFirmwarePartnumber = "TestCellularFirmwarePartnumber",
				NetworkFirmwarePartnumber = "TestNetworkFirmwarePartnumber",
				SatelliteFirmwarePartnumber = "TestSatelliteFirmwarePartnumber",
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			_deviceService.CheckExistingDevice(input.DeviceUID.Value).Returns(false);
			_deviceService.UpdateDevice(input, Arg.Any<DeviceStateEnum>()).Returns(false);

			//Act
			var result = _target.UpdateDevice(input);

			//Assert
			_deviceService.Received(0).UpdateDevice(Arg.Any<UpdateDeviceEvent>(), Arg.Any<DeviceStateEnum>());
			Assert.Equal(typeof(BadRequestObjectResult), result.GetType());

		}


		[Fact]
		public void TestUpdateDevice_ExceptionOnUpdate_Fail()
		{
			//Arrange
			var input = new UpdateDeviceEvent
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
				DataLinkType = "J1939",
				FirmwarePartNumber = "TestFirmwarePartNumber",
				CellModemIMEI = "TestCellModemIMEI",
				DevicePartNumber = "TestDevicePartNumber",
				CellularFirmwarePartnumber = "TestCellularFirmwarePartnumber",
				NetworkFirmwarePartnumber = "TestNetworkFirmwarePartnumber",
				SatelliteFirmwarePartnumber = "TestSatelliteFirmwarePartnumber",
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			_deviceService.CheckExistingDevice(input.DeviceUID.Value).Returns(true);
			_deviceService.UpdateDevice(input, Enum.Parse<DeviceStateEnum>(input.DeviceState.Trim(), true)).Returns(x => { throw new Exception(); });


			//Act
			var result = _target.UpdateDevice(input);

			//Assert
			_deviceService.Received(1).UpdateDevice(Arg.Any<UpdateDeviceEvent>(), Enum.Parse<DeviceStateEnum>(input.DeviceState.Trim(), true));
			Assert.Equal(500, ((ObjectResult)result).StatusCode);

		}


		[Fact]
		public void GetDeviceDetailsByDeviceUID_Success()
		{
			var deviceUid = Guid.NewGuid();
			DeviceProperties retList = new DeviceProperties { DeviceUID = deviceUid.ToString(), DeviceType = "PL641", DeviceState = "Subscribed" };
			_deviceService.GetExistingDeviceProperties(Arg.Any<Guid>()).Returns(retList);

			_target.ControllerContext.HttpContext.Request.Headers["UserUID_APIRequest"] = Guid.NewGuid().ToString();


			var result = _target.GetDeviceDetailsByDeviceUID(deviceUid);

			//Assert
			Assert.Equal(typeof(OkObjectResult), result.GetType());
			Assert.IsType<OkObjectResult>(result);
			Assert.Equal(retList, ((DeviceProperties)((OkObjectResult)result).Value));
			_deviceService.Received(1).GetExistingDeviceProperties(Arg.Any<Guid>());
		}

		[Fact]
		public void GetDeviceDetailsByDeviceUID_UserCustomerNotMappedwithAssetDevice_ReturnsErrorMessage()
		{
			var deviceUid = Guid.NewGuid();
			DeviceProperties retList = new DeviceProperties { DeviceUID = deviceUid.ToString(), DeviceType = "PL641", DeviceState = "Subscribed" };
			_deviceService.GetExistingDeviceProperties(Arg.Any<Guid>()).Returns(retList);
			_deviceService.ValidateAuthorizedCustomerByDevice(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

			var result = _target.GetDeviceDetailsByDeviceUID(deviceUid);

			//Assert
			Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
			Assert.Equal("Unauthorized User Access", ((BadRequestObjectResult)result).Value);
		}

		[Fact]
		public void GetDeviceDetailsByDeviceUID_DeviceDoestExist_ReturnsErrorMessage()
		{
			var deviceUid = Guid.NewGuid();
			DeviceProperties retList = null;
			_deviceService.GetExistingDeviceProperties(Arg.Any<Guid>()).Returns(retList);
			_deviceService.ValidateAuthorizedCustomerByDevice(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
			_target.ControllerContext.HttpContext.Request.Headers["UserUID_APIRequest"] = Guid.NewGuid().ToString();

			var result = _target.GetDeviceDetailsByDeviceUID(deviceUid);

			//Assert
			Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
			Assert.Equal("Device Doesn't Exist", ((BadRequestObjectResult)result).Value);
		}
		[Fact]
		public void GetDeviceDetailsByDeviceUID_Exception_ReturnsErrorMessage()
		{
			var deviceUid = Guid.NewGuid();
			DeviceProperties retList = new DeviceProperties { DeviceUID = deviceUid.ToString(), DeviceType = "PL641", DeviceState = "Subscribed" };
			_deviceService.GetExistingDeviceProperties(Arg.Any<Guid>()).Returns(x => { throw new Exception(); });
			_deviceService.ValidateAuthorizedCustomerByDevice(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
			_target.ControllerContext.HttpContext.Request.Headers["UserUID_APIRequest"] = Guid.NewGuid().ToString();

			var result = _target.GetDeviceDetailsByDeviceUID(deviceUid);

			//Assert
			_deviceService.Received(1).GetExistingDeviceProperties(Arg.Any<Guid>());
			Assert.Equal(500, ((ObjectResult)result).StatusCode);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void TestAssociateDeviceAsset_ValidInput_Success(bool isAssociateAssetDevice)
		{
			//Arrange
			AssociateDeviceAssetEvent input = new AssociateDeviceAssetEvent
			{
				DeviceUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			_deviceService.AssociateAssetDevice(Arg.Any<AssociateDeviceAssetEvent>()).Returns(isAssociateAssetDevice);
			//Act
			var result = _target.AssociateDeviceAsset(input);

			//Assert
			if (isAssociateAssetDevice)
			{
				Assert.Equal(typeof(OkResult), result.GetType());
				_deviceService.Received(1).AssociateAssetDevice(Arg.Any<AssociateDeviceAssetEvent>());
			}
			else
			{
				Assert.Equal(typeof(ObjectResult), result.GetType());
				Assert.Equal(500, ((ObjectResult)result).StatusCode);
				_deviceService.Received(1).AssociateAssetDevice(Arg.Any<AssociateDeviceAssetEvent>());
				Assert.Equal("AssociateDeviceAsset is not published.", ((ObjectResult)result).Value);
			}
		}


		[Fact]
		public void TestAssociateDeviceAsset_PublishToQueueFailure_ReturnsServerError()
		{
			//Arrange
			AssociateDeviceAssetEvent input = new AssociateDeviceAssetEvent
			{
				DeviceUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			_deviceService.AssociateAssetDevice(Arg.Any<AssociateDeviceAssetEvent>()).Returns(x => { throw new Exception(); });

			//Act
			var result = _target.AssociateDeviceAsset(input);

			//Assert
			Assert.Equal(500, ((ObjectResult)result).StatusCode);
			_deviceService.Received(1).AssociateAssetDevice(Arg.Any<AssociateDeviceAssetEvent>());
		}



		[Fact]
		public void TestAssociateDeviceAsset_AssetDeviceAssociationExistsAlready_NewActionUTC_BadRequest()
		{

			//Arrange
			AssociateDeviceAssetEvent input = new AssociateDeviceAssetEvent
			{
				DeviceUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var assetDevice = new DbAssetDevice { fk_AssetUID = input.AssetUID, fk_DeviceUID = input.DeviceUID, ActionUTC = DateTime.UtcNow.AddMinutes(-10) };
			_deviceService.GetAssetDevice(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(assetDevice);
			//Act
			var result = _target.AssociateDeviceAsset(input);


			Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
			Assert.Equal(400, ((BadRequestObjectResult)result).StatusCode);
			_deviceService.Received(0).AssociateAssetDevice(Arg.Any<AssociateDeviceAssetEvent>());
			_deviceService.Received(0).GetAssociatedDevicesByAsset(input.AssetUID);
			Assert.Equal("The Device is already Associated with this Asset.", ((BadRequestObjectResult)result).Value);
		}
		[Fact]
		public void TestAssociateDeviceAsset_AssetDeviceAssociationExistsAlready_OldActionUTC_BadRequest()
		{

			//Arrange
			AssociateDeviceAssetEvent input = new AssociateDeviceAssetEvent
			{
				DeviceUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var assetDevice = new DbAssetDevice { fk_AssetUID = input.AssetUID, fk_DeviceUID = input.DeviceUID, ActionUTC = DateTime.UtcNow.AddMinutes(10) };
			_deviceService.GetAssetDevice(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(assetDevice);
			//Act
			var result = _target.AssociateDeviceAsset(input);


			Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
			Assert.Equal(400, ((BadRequestObjectResult)result).StatusCode);
			_deviceService.Received(0).AssociateAssetDevice(Arg.Any<AssociateDeviceAssetEvent>());
			_deviceService.Received(0).GetAssociatedDevicesByAsset(input.AssetUID);
			Assert.Equal("The AssociateDeviceAsset does not have the latest data to be updated.", ((BadRequestObjectResult)result).Value);
		}

		[Theory]
		[InlineData(null)]
		public void TestAssociateDeviceAsset_NoAssetDeviceAssociationExists_SubscribedStatusAssetAlready_BadRequest(DbAssetDevice dbAssetDevice)
		{

			//Arrange
			AssociateDeviceAssetEvent input = new AssociateDeviceAssetEvent
			{
				DeviceUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var deviceDTo = new DeviceDto { DeviceUID = input.DeviceUID, DeviceStatusID = 3 };
			_deviceService.GetAssetDevice(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(dbAssetDevice);
			_deviceService.GetAssociatedDevicesByAsset(Arg.Any<Guid>()).Returns(deviceDTo);
			//Act
			var result = _target.AssociateDeviceAsset(input);


			Assert.Equal(typeof(OkObjectResult), result.GetType());
			Assert.Equal(200, ((OkObjectResult)result).StatusCode);
			_deviceService.Received(0).AssociateAssetDevice(Arg.Any<AssociateDeviceAssetEvent>());
			Assert.Equal("AssociateDeviceAsset ignored as another device is subscribed already", ((OkObjectResult)result).Value);
		}

		[Theory]
		[InlineData(null)]
		public void TestAssociateDeviceAsset_NoAssetDeviceAssociationExists_NotInSubscribedStatusAssetAlready_Success(DbAssetDevice dbAssetDevice)
		{

			//Arrange
			AssociateDeviceAssetEvent input = new AssociateDeviceAssetEvent
			{
				DeviceUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var deviceDTo = new DeviceDto { DeviceUID = input.DeviceUID, DeviceStatusID = 2 };
			_deviceService.GetAssetDevice(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(dbAssetDevice);
			_deviceService.GetAssociatedDevicesByAsset(Arg.Any<Guid>()).Returns(deviceDTo);
			_deviceService.AssociateAssetDevice(Arg.Any<AssociateDeviceAssetEvent>()).Returns(true);

			//Act
			var result = _target.AssociateDeviceAsset(input);


			Assert.Equal(typeof(OkResult), result.GetType());
			Assert.Equal(200, ((OkResult)result).StatusCode);
			_deviceService.Received(1).DissociateAssetDevice(Arg.Any<DissociateDeviceAssetEvent>());
			_deviceService.Received(1).AssociateAssetDevice(Arg.Any<AssociateDeviceAssetEvent>());
		}

		[Theory]
		[InlineData(null)]
		public void TestAssociateDeviceAsset_NoAssetDeviceAssociationExists_NotInSubscribedStatusAssetAlready_Fail(DbAssetDevice dbAssetDevice)
		{

			//Arrange
			AssociateDeviceAssetEvent input = new AssociateDeviceAssetEvent
			{
				DeviceUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var deviceDTo = new DeviceDto { DeviceUID = input.DeviceUID, DeviceStatusID = 2 };
			_deviceService.GetAssetDevice(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(dbAssetDevice);
			_deviceService.GetAssociatedDevicesByAsset(Arg.Any<Guid>()).Returns(deviceDTo);
			_deviceService.AssociateAssetDevice(Arg.Any<AssociateDeviceAssetEvent>()).Returns(false);

			//Act
			var result = _target.AssociateDeviceAsset(input);


			Assert.Equal(typeof(ObjectResult), result.GetType());
			Assert.Equal(500, ((ObjectResult)result).StatusCode);
			_deviceService.Received(1).DissociateAssetDevice(Arg.Any<DissociateDeviceAssetEvent>());
			_deviceService.Received(1).AssociateAssetDevice(Arg.Any<AssociateDeviceAssetEvent>());
			Assert.Equal("AssociateDeviceAsset is not published.", ((ObjectResult)result).Value);

		}

		[Theory]
		[InlineData(null)]
		public void TestDisssociateDeviceAsset_DissociatedAlready_Fail(DbAssetDevice dbAssetDevice)
		{
			//Arrange
			DissociateDeviceAssetEvent input = new DissociateDeviceAssetEvent
			{
				DeviceUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			_deviceService.GetAssetDevice(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(dbAssetDevice);
			_deviceService.DissociateAssetDevice(Arg.Any<DissociateDeviceAssetEvent>()).Returns(true);
			//Act
			var result = _target.DissociateDeviceAsset(input);

			//Assert
			Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
			Assert.Equal(400, ((BadRequestObjectResult)result).StatusCode);
			_deviceService.Received(0).DissociateAssetDevice(Arg.Any<DissociateDeviceAssetEvent>());
			Assert.Equal("No AssetDevice Association exists for the given Asset-Device.", ((BadRequestObjectResult)result).Value);

		}

		[Fact]
		public void TestDisssociateDeviceAsset_DissociatedAlready__NewActionUTcInTable_Fail()
		{
			//Arrange
			DissociateDeviceAssetEvent input = new DissociateDeviceAssetEvent
			{
				DeviceUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var assetDevice = new DbAssetDevice { fk_AssetUID = input.AssetUID, fk_DeviceUID = input.DeviceUID, ActionUTC = DateTime.UtcNow.AddMinutes(10) };

			_deviceService.GetAssetDevice(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(assetDevice);
			_deviceService.DissociateAssetDevice(Arg.Any<DissociateDeviceAssetEvent>()).Returns(true);
			//Act
			var result = _target.DissociateDeviceAsset(input);

			//Assert
			Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
			Assert.Equal(400, ((BadRequestObjectResult)result).StatusCode);
			_deviceService.Received(0).DissociateAssetDevice(Arg.Any<DissociateDeviceAssetEvent>());
			Assert.Equal("The DissociateDeviceAssetEvent does not have the latest data to be updated.", ((BadRequestObjectResult)result).Value);

		}

		[Fact]
		public void TestDisssociateDeviceAsset_DissociatedAlready__OldActionUTcInTable_Success()
		{
			//Arrange
			DissociateDeviceAssetEvent input = new DissociateDeviceAssetEvent
			{
				DeviceUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var assetDevice = new DbAssetDevice { fk_AssetUID = input.AssetUID, fk_DeviceUID = input.DeviceUID, ActionUTC = DateTime.UtcNow.AddMinutes(-10) };

			_deviceService.GetAssetDevice(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(assetDevice);
			_deviceService.DissociateAssetDevice(Arg.Any<DissociateDeviceAssetEvent>()).Returns(true);
			//Act
			var result = _target.DissociateDeviceAsset(input);

			//Assert
			Assert.Equal(typeof(OkResult), result.GetType());
			Assert.Equal(200, ((OkResult)result).StatusCode);
			_deviceService.Received(1).DissociateAssetDevice(Arg.Any<DissociateDeviceAssetEvent>());

		}

		[Fact]
		public void TestDisssociateDeviceAsset_DissociatedAlready__OldActionUTcInTable_Fail()
		{
			//Arrange
			DissociateDeviceAssetEvent input = new DissociateDeviceAssetEvent
			{
				DeviceUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var assetDevice = new DbAssetDevice { fk_AssetUID = input.AssetUID, fk_DeviceUID = input.DeviceUID, ActionUTC = DateTime.UtcNow.AddMinutes(-10) };

			_deviceService.GetAssetDevice(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(assetDevice);
			_deviceService.DissociateAssetDevice(Arg.Any<DissociateDeviceAssetEvent>()).Returns(false);
			//Act
			var result = _target.DissociateDeviceAsset(input);

			//Assert
			Assert.Equal(typeof(ObjectResult), result.GetType());
			Assert.Equal(500, ((ObjectResult)result).StatusCode);
			_deviceService.Received(1).DissociateAssetDevice(Arg.Any<DissociateDeviceAssetEvent>());
			Assert.Equal("DissociateDeviceAsset is not processed.", ((ObjectResult)result).Value);
		}


		[Fact]
		public void TestDissociateDeviceAsset_PublishToQueueFailure_ReturnsServerError()
		{
			//Arrange
			DissociateDeviceAssetEvent input = new DissociateDeviceAssetEvent
			{
				DeviceUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var assetDevice = new DbAssetDevice { fk_AssetUID = input.AssetUID, fk_DeviceUID = input.DeviceUID, ActionUTC = DateTime.UtcNow.AddMinutes(-10) };

			_deviceService.GetAssetDevice(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(assetDevice);
			_deviceService.DissociateAssetDevice(Arg.Any<DissociateDeviceAssetEvent>()).Returns(x => { throw new Exception(); });
			//Act
			var result = _target.DissociateDeviceAsset(input);

			//Assert
			Assert.Equal(500, ((ObjectResult)result).StatusCode);
			_deviceService.Received(1).DissociateAssetDevice(Arg.Any<DissociateDeviceAssetEvent>());
		}

		private CreateDeviceEvent ValidateDevicePayload(Guid deviceGuid, string deviceSerialNumber, string deviceType)
		{
			return Arg.Is<CreateDeviceEvent>(devicePayload => devicePayload.DeviceUID == deviceGuid && devicePayload.DeviceSerialNumber == deviceSerialNumber && devicePayload.DeviceType == deviceType);
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
