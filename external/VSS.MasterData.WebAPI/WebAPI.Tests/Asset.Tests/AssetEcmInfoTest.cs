using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.Asset.Controllers.V1;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using Xunit;

namespace Asset.Tests
{
	public class AssetEcmInfoTest
	{
		private readonly IAssetECMInfoServices _assetECMInfoService;
		private readonly IAssetServices _assetServices;
		private readonly ILogger _logger;
		private AssetEcmInfoController _controller;
		ContainerBuilder _builder;
		private const string HeaderTokenJwt = "eyJ0eXAiOiJKV1QiLCJhbGciOiJTSEEyNTZ3aXRoUlNBIiwieDV0IjoiWW1FM016UTRNVFk0TkRVMlpEWm1PRGRtTlRSbU4yWmxZVGt3TVdFelltTmpNVGt6TURFelpnPT0ifQ==.eyJpc3MiOiJ3c28yLm9yZy9wcm9kdWN0cy9hbSIsImV4cCI6MTQ0ODAwNDEyODIyNCwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9zdWJzY3JpYmVyIjoiZGV2LXZzc2FkbWluQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbmlkIjoiNTYwIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbm5hbWUiOiJWU1AtTkRldiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBwbGljYXRpb250aWVyIjoiIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcGljb250ZXh0IjoiL3QvdHJpbWJsZS5jb20vdnNwLXFhLWlkZW50aXR5YXBpIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy92ZXJzaW9uIjoidjEiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3RpZXIiOiJVbmxpbWl0ZWQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2tleXR5cGUiOiJQUk9EVUNUSU9OIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy91c2VydHlwZSI6IkFQUExJQ0FUSU9OX1VTRVIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJkZXYtdmxjbGFzc2ljdXNlckB0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW5kdXNlclRlbmFudElkIjoiMSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW1haWxhZGRyZXNzIjoiQmhvb2JhbGFuX1BhbGFuaXZlbEBUcmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZmlyc3RuYW1lIjoiQmhvb2JhbGFuIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9naXZlbm5hbWUiOiJCaG9vYmFsYW4iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2lkZW50aXR5L2FjY291bnRMb2NrZWQiOiJmYWxzZSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdG5hbWUiOiJQYWxhbml2ZWwiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3JvbGUiOiIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3V1aWQiOiJhYTVkYTYyNy04OWVlLTRlYmUtYTE4Mi1hNzE3NzA1YWZmZDAifQ==.DLa86MVuE2nCuzLGCqgcw20/q5ikODPgDDwLiPO78RQpKwlqOG5Poa5gU2cyEYyDnYMiYW1M3Ffjh2icCBpnediyG2b5b9iNEHvBq+Y3Kiuc3qINva7fqU2Z1hk1afw+NvmQoYO9qVDXr8QXNUQNMEIRheJWQq0PMM+VD2IViEU=";

		public AssetEcmInfoTest()
		{
			_logger = Substitute.For<ILogger>();
			_assetECMInfoService = Substitute.For<IAssetECMInfoServices>();
			_assetServices = Substitute.For<IAssetServices>();
			_builder = new ContainerBuilder();
			var container = _builder.Build();
			_builder.Register(config => new AssetEcmInfoController(_assetECMInfoService, _assetServices, _logger)).As<AssetEcmInfoController>(); 
			_controller = CreateControllerWithHeader(container, new ControllerContext(), _assetECMInfoService, _assetServices, _logger);

		}

		private static AssetEcmInfoController CreateControllerWithHeader(IContainer container, ControllerContext mockHttpContext, IAssetECMInfoServices _assetECMInfoService, IAssetServices _assetServices, ILogger _logger)
		{
			var controller = new AssetEcmInfoController(_assetECMInfoService, _assetServices, _logger);
			controller.ControllerContext = mockHttpContext;
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Request.Headers["X-JWT-Assertion"] = HeaderTokenJwt;

			return controller;
		}

		#region GetAssetECM 
		[Fact]
		public void Get_Valid_AssetECMInfo_ByAssetUID()
		{
			//Arrange
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
			_assetECMInfoService.GetAssetECMInfo(Arg.Any<Guid>()).Returns(x =>
				new List<AssetECM>() { new AssetECM { SerialNumber = "1297S145YD", PartNumber = "5679068-02", Description = "Communication Gateway #1", SyncClockEnabled = true, SyncClockLevel = true } }
			);

			var assetUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0c");

			//Act
			var response = _controller.GetAssetECMInfoByAssetUID(assetUID);

			//Assert
			Assert.Single(((VSS.MasterData.WebAPI.ClientModel.AssetECMInfoResponse)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value).AssetECMInfo);
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
			Assert.Equal("Master", ((VSS.MasterData.WebAPI.ClientModel.AssetECMInfoResponse)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value).AssetECMInfo[0].SyncClockLevel);
			Assert.Equal("Communication Gateway #1", ((VSS.MasterData.WebAPI.ClientModel.AssetECMInfoResponse)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value).AssetECMInfo[0].ECMDescription);
			Assert.Equal("1297S145YD", ((VSS.MasterData.WebAPI.ClientModel.AssetECMInfoResponse)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value).AssetECMInfo[0].ECMSerialNumber);
			Assert.Equal("5679068-02", ((VSS.MasterData.WebAPI.ClientModel.AssetECMInfoResponse)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value).AssetECMInfo[0].FirmwarePartNumber);
		}

		[Fact]
		public void Get_Empty_AssetECMInfo_ByAssetUID()
		{
			//Arrange
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
			_assetECMInfoService.GetAssetECMInfo(Arg.Any<Guid>()).Returns(x =>
				new List<AssetECM>() { }
			);

			//_controller.ControllerContext.HttpContext.Request.Headers["X-JWT-Assertion"] = string.Empty;
			//_controller.ControllerContext.HttpContext.Request.Headers["X-VisionLink-UserUid"] = "a162eb79-0317-11e9-a988-029d68d36a0c";
			var assetUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0c");

			//Act
			var response = _controller.GetAssetECMInfoByAssetUID(assetUID);

			//Assert
			Assert.Empty(((VSS.MasterData.WebAPI.ClientModel.AssetECMInfoResponse)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value).AssetECMInfo);
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void InternalServerError_GetAssetECMInfo_ByAssetUID()
		{
			//Arrange
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
			_assetECMInfoService.GetAssetECMInfo(Arg.Any<Guid>()).Returns(x => throw new Exception());

			var assetUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0c");

			//Act
			var response = _controller.GetAssetECMInfoByAssetUID(assetUID);

			//Assert
			Assert.Equal(500, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void AccesExceptionError_GetAssetECMInfo_ByAssetUID()
		{
			//Arrange
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
			_assetECMInfoService.GetAssetECMInfo(Arg.Any<Guid>()).Returns(x => throw new Exception());

			var assetUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0c");

			//Act
			var response = _controller.GetAssetECMInfoByAssetUID(assetUID);

			//Assert
			Assert.Equal(500, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		//[Fact]
		//public void InternalServerError_InvalidHeader_GetAssetECMInfo_ByAssetUID()
		//{
		//	//Arrange
		//	_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
		//	_controller.ControllerContext.HttpContext.Request.Headers["X-JWT-Assertion"] = "";
		//	_assetECMInfoService.GetAssetECMInfo(Arg.Any<Guid>()).Returns(x =>
		//		new List<AssetECM>() { }
		//	);
		//	var assetUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0c");

		//	var response = _controller.GetAssetECMInfoByAssetUID(assetUID);

		//	//Assert
		//	Assert.Equal(500, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		//}

		[Fact]
		public void UnauthorizedAccess_EmptyHeader_GetAssetECMInfo_ByAssetUID()
		{
			//Arrange
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
			_assetECMInfoService.GetAssetECMInfo(Arg.Any<Guid>()).Returns(x =>
				new List<AssetECM>() { }
			);

			_controller.ControllerContext.HttpContext.Request.Headers["X-JWT-Assertion"] = string.Empty;
			_controller.ControllerContext.HttpContext.Request.Headers["X-VisionLink-UserUid"] = string.Empty;
			var assetUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0c");

			//Act
			var response = _controller.GetAssetECMInfoByAssetUID(assetUID);

			//Assert
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void Unauthorized_Get_AssetECMInfo_ByAssetUID()
		{
			//Arrange
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
			_assetECMInfoService.GetAssetECMInfo(Arg.Any<Guid>()).Returns(x =>
				new List<AssetECM>() { }
			);

			var assetUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0c");

			//Act
			var response = _controller.GetAssetECMInfoByAssetUID(assetUID);

			//Assert
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void InvalidGuid_Get_AssetECMInfo_ByAssetUID()
		{
			//Arrange
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
			_assetECMInfoService.GetAssetECMInfo(Arg.Any<Guid>()).Returns(x =>
				new List<AssetECM>() { }
			);

			var assetUID = new Guid("00000000-0000-0000-0000-000000000000");

			//Act
			var response = _controller.GetAssetECMInfoByAssetUID(assetUID);

			//Assert
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}
		#endregion
	}
}