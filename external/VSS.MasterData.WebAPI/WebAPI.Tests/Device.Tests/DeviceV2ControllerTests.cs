using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using VSS.Authentication.JWT;
using VSS.MasterData.WebAPI.ClientModel.Device;
using VSS.MasterData.WebAPI.Device.Controllers.V2;
using VSS.MasterData.WebAPI.Interfaces.Device;
using Xunit;

namespace VSP.MasterData.Device.UnitTests
{
	public class DeviceV2ControllerTests
	{
		private readonly DeviceV2Controller _target;
		private readonly IDeviceService _deviceService;
		private readonly ILogger logger;
		private readonly IConfiguration configuration;

		ContainerBuilder _builder;
		private const string HeaderTokenJwt =
			"eyJ0eXAiOiJKV1QiLCJhbGciOiJTSEEyNTZ3aXRoUlNBIiwieDV0IjoiWW1FM016UTRNVFk0TkRVMlpEWm1PRGRtTlRSbU4yWmxZVGt3TVdFelltTmpNVGt6TURFelpnPT0ifQ==.eyJpc3MiOiJ3c28yLm9yZy9wcm9kdWN0cy9hbSIsImV4cCI6MTQ0ODAwNDEyODIyNCwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9zdWJzY3JpYmVyIjoiZGV2LXZzc2FkbWluQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbmlkIjoiNTYwIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbm5hbWUiOiJWU1AtTkRldiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBwbGljYXRpb250aWVyIjoiIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcGljb250ZXh0IjoiL3QvdHJpbWJsZS5jb20vdnNwLXFhLWlkZW50aXR5YXBpIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy92ZXJzaW9uIjoidjEiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3RpZXIiOiJVbmxpbWl0ZWQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2tleXR5cGUiOiJQUk9EVUNUSU9OIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy91c2VydHlwZSI6IkFQUExJQ0FUSU9OX1VTRVIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJkZXYtdmxjbGFzc2ljdXNlckB0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW5kdXNlclRlbmFudElkIjoiMSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW1haWxhZGRyZXNzIjoiQmhvb2JhbGFuX1BhbGFuaXZlbEBUcmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZmlyc3RuYW1lIjoiQmhvb2JhbGFuIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9naXZlbm5hbWUiOiJCaG9vYmFsYW4iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2lkZW50aXR5L2FjY291bnRMb2NrZWQiOiJmYWxzZSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdG5hbWUiOiJQYWxhbml2ZWwiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3JvbGUiOiIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3V1aWQiOiJhYTVkYTYyNy04OWVlLTRlYmUtYTE4Mi1hNzE3NzA1YWZmZDAifQ==.DLa86MVuE2nCuzLGCqgcw20/q5ikODPgDDwLiPO78RQpKwlqOG5Poa5gU2cyEYyDnYMiYW1M3Ffjh2icCBpnediyG2b5b9iNEHvBq+Y3Kiuc3qINva7fqU2Z1hk1afw+NvmQoYO9qVDXr8QXNUQNMEIRheJWQq0PMM+VD2IViEU=";

		public DeviceV2ControllerTests()
		{
			logger = Substitute.For<ILogger>();
			configuration = Substitute.For<IConfiguration>();
			_deviceService = Substitute.For<IDeviceService>();
			_builder = new ContainerBuilder();
			var container = _builder.Build();
			_builder.Register(config => new DeviceV2Controller(_deviceService, logger, configuration)).As<DeviceV2Controller>();
			_target = CreateControllerWithHeader(container,new ControllerContext(), logger, _deviceService, configuration); 

		}

		private static DeviceV2Controller CreateControllerWithHeader(IContainer container, ControllerContext mockHttpContext, ILogger logger, IDeviceService _deviceService,IConfiguration configuration)
		{
			configuration["KiewitAMPCustomerUID"] = "E6E2F851-44C5-E311-AA77-00505688274D";
			var controller = new DeviceV2Controller(_deviceService, logger, configuration);
			controller.ControllerContext = mockHttpContext;
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Request.Headers["X-JWT-Assertion"] = HeaderTokenJwt;
			return controller;
		}

		[Fact]
		public void GetDeviceDetailsByDeviceUID_Success()
		{
			var deviceUid = Guid.NewGuid();
			List<DevicePropertiesV2> retList = new List<DevicePropertiesV2>();
			retList.Add(new DevicePropertiesV2{ DeviceUID = deviceUid, DeviceType = "PL641",DeviceState=1 });
			_deviceService.GetDevicePropertiesV2ByDeviceGuid(Arg.Any<Guid>()).Returns( retList);

			_target.ControllerContext.HttpContext.Request.Headers["UserUID_APIRequest"] = Guid.NewGuid().ToString();


			var result = _target.GetDeviceDetailsByDeviceUID(deviceUid);

			//Assert
			Assert.Equal(typeof(OkObjectResult), result.GetType());
			Assert.IsType<OkObjectResult>(result);
			_deviceService.Received(1).GetDevicePropertiesV2ByDeviceGuid(Arg.Any<Guid>());
		}

		[Fact]
		public void GetDeviceDetailsByDeviceUID_UserCustomerNotMappedwithAssetDevice_ReturnsErrorMessage()
		{
			var deviceUid = Guid.NewGuid();
			List<DevicePropertiesV2> retList = new List<DevicePropertiesV2>();
			retList.Add(new DevicePropertiesV2 { DeviceUID = deviceUid, DeviceType = "PL641", DeviceState = 1 });
			_deviceService.GetDevicePropertiesV2ByDeviceGuid(Arg.Any<Guid>()).Returns(retList);
			_deviceService.ValidateAuthorizedCustomerByDevice(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

			_target.ControllerContext.HttpContext.Request.Headers["UserUID_APIRequest"] = Guid.NewGuid().ToString();


			var result = _target.GetDeviceDetailsByDeviceUID(deviceUid);

			//Assert
			Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
			Assert.Equal("Unauthorized User Access", ((BadRequestObjectResult)result).Value);
			_deviceService.Received(0).GetDevicePropertiesV2ByDeviceGuid(Arg.Any<Guid>());

		}

		[Fact]
		public void GetDeviceDetailsByDeviceUID_DeviceDoestExist_ReturnsErrorMessage()
		{
			var deviceUid = Guid.NewGuid();
			List<DevicePropertiesV2> retList = null;
			_deviceService.GetDevicePropertiesV2ByDeviceGuid(Arg.Any<Guid>()).Returns(retList);

			_target.ControllerContext.HttpContext.Request.Headers["UserUID_APIRequest"] = Guid.NewGuid().ToString();


			var result = _target.GetDeviceDetailsByDeviceUID(deviceUid);

			//Assert
			Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
			_deviceService.Received(1).GetDevicePropertiesV2ByDeviceGuid(Arg.Any<Guid>());
			Assert.Equal("Device Doesn't Exist", ((BadRequestObjectResult)result).Value);
		}
		[Fact]
		public void GetDeviceDetailsByDeviceUID_Exception_ReturnsErrorMessage()
		{
			var deviceUid = Guid.NewGuid();
			List<DevicePropertiesV2> retList = new List<DevicePropertiesV2>();
			retList.Add(new DevicePropertiesV2 { DeviceUID = deviceUid, DeviceType = "PL641", DeviceState = 1 });
			_deviceService.GetDevicePropertiesV2ByDeviceGuid(Arg.Any<Guid>()).Returns(x=> { throw new Exception(); });

			_target.ControllerContext.HttpContext.Request.Headers["UserUID_APIRequest"] = Guid.NewGuid().ToString();


			var result = _target.GetDeviceDetailsByDeviceUID(deviceUid);

			//Assert
			_deviceService.Received(1).GetDevicePropertiesV2ByDeviceGuid(Arg.Any<Guid>());
			Assert.Equal(500, ((ObjectResult)result).StatusCode);
		}


	

		[Fact]
		public void GetDeviceDetailsByAssetUID__ApplicationUserToken_UserCustomerNotMappedwithAssetDevice_ReturnsErrorMessage()
		{
			var deviceUid = Guid.NewGuid();
			List<AssetDevicePropertiesV2> retList = new List<AssetDevicePropertiesV2>();
			retList.Add(new AssetDevicePropertiesV2 { DeviceUID = deviceUid, DeviceType = "PL641", DeviceState = 1 });
			_deviceService.GetDevicePropertiesV2ByAssetGuid(Arg.Any<Guid>()).Returns(retList);
			_deviceService.ValidateAuthorizedCustomerByDevice(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);


			var result = _target.GetDeviceDetailsByAssetUID(deviceUid, _target.ControllerContext.HttpContext.Request.Headers["X-JWT-Assertion"]);

			//Assert
			Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
			Assert.Equal("Unauthorized User Access", ((BadRequestObjectResult)result).Value);
			_deviceService.Received(0).GetDevicePropertiesV2ByAssetGuid(Arg.Any<Guid>());

		}

		[Fact]
		public void GetDeviceDetailsByAssetUID_ApplicationToken_Kiewit_Success()
		{
			var assetUID = Guid.NewGuid();
			List<AssetDevicePropertiesV2> retList = new List<AssetDevicePropertiesV2>();
			retList.Add(new AssetDevicePropertiesV2 { DeviceUID = assetUID, DeviceType = "PL641", DeviceState = 1 });
			_deviceService.GetDevicePropertiesV2ByAssetGuid(Arg.Any<Guid>()).Returns(retList);
			_deviceService.GetCustomersForApplication(Arg.Any<string>()).Returns(new List<Guid> { new Guid("E6E2F851-44C5-E311-AA77-00505688274D") }); 

			_target.ControllerContext.HttpContext.Request.Headers["UserUID_APIRequest"] = Guid.NewGuid().ToString();

			var aa = TPaaSJWT.GenerateFakeApplicationJWT("MasterDataManagement");

			var result = _target.GetDeviceDetailsByAssetUID(assetUID, aa.EncodedJWT);


			//Assert
			Assert.Equal(typeof(OkObjectResult), result.GetType());
			Assert.IsType<OkObjectResult>(result);
			_deviceService.Received(1).GetDevicePropertiesV2ByAssetGuid(Arg.Any<Guid>());
		}

		[Fact]
		public void GetDeviceDetailsByAssetUID_ApplicationToken_OtherThanKiewit_NoAccess()
		{
			var deviceUid = Guid.NewGuid();
			List<AssetDevicePropertiesV2> retList = new List<AssetDevicePropertiesV2>();
			retList.Add(new AssetDevicePropertiesV2 { DeviceUID = deviceUid, DeviceType = "PL641", DeviceState = 1 });
			_deviceService.GetDevicePropertiesV2ByAssetGuid(Arg.Any<Guid>()).Returns(retList);
			_deviceService.GetCustomersForApplication(Arg.Any<string>()).Returns(new List<Guid> { Guid.NewGuid() }); ;

			_target.ControllerContext.HttpContext.Request.Headers["UserUID_APIRequest"] = Guid.NewGuid().ToString();

			var aa = TPaaSJWT.GenerateFakeApplicationJWT("MasterDataManagement");

			var result = _target.GetDeviceDetailsByAssetUID(deviceUid, aa.EncodedJWT);


			//Assert
			Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
			_deviceService.Received(0).GetDevicePropertiesV2ByAssetGuid(Arg.Any<Guid>());
			Assert.Equal("Application does not have Access. Please contact your API administrator.", ((BadRequestObjectResult)result).Value);
		}

		[Fact]
		public void GetDeviceDetailsByAssetUID_DeviceDoestExist_ReturnsErrorMessage()
		{
			var deviceUid = Guid.NewGuid();
			List<AssetDevicePropertiesV2> retList = null;
			_deviceService.GetDevicePropertiesV2ByAssetGuid(Arg.Any<Guid>()).Returns(retList);

			_target.ControllerContext.HttpContext.Request.Headers["UserUID_APIRequest"] = Guid.NewGuid().ToString();


			var result = _target.GetDeviceDetailsByAssetUID(deviceUid, _target.ControllerContext.HttpContext.Request.Headers["X-JWT-Assertion"]);

			//Assert
			Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
			_deviceService.Received(1).GetDevicePropertiesV2ByAssetGuid(Arg.Any<Guid>());
			Assert.Equal("Device Doesn't Exist", ((BadRequestObjectResult)result).Value);
		}
		[Fact]
		public void GetDeviceDetailsByAssetUID_Exception_ReturnsErrorMessage()
		{
			var deviceUid = Guid.NewGuid();
			List<AssetDevicePropertiesV2> retList = new List<AssetDevicePropertiesV2>();
			retList.Add(new AssetDevicePropertiesV2 { DeviceUID = deviceUid, DeviceType = "PL641", DeviceState = 1 });
			_deviceService.GetDevicePropertiesV2ByAssetGuid(Arg.Any<Guid>()).Returns(x => { throw new Exception(); });

			_target.ControllerContext.HttpContext.Request.Headers["UserUID_APIRequest"] = Guid.NewGuid().ToString();


			var result = _target.GetDeviceDetailsByAssetUID(deviceUid, _target.ControllerContext.HttpContext.Request.Headers["X-JWT-Assertion"]);

			//Assert
			_deviceService.Received(1).GetDevicePropertiesV2ByAssetGuid(Arg.Any<Guid>());
			Assert.Equal(500, ((ObjectResult)result).StatusCode);
		}
	}
}
