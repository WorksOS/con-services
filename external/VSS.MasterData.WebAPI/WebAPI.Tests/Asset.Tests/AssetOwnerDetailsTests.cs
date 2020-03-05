using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using VSS.MasterData.WebAPI.Asset.Controllers.V1;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using Xunit;
using static VSS.MasterData.WebAPI.Utilities.Enums.Enums;

namespace Asset.Tests
{
	public class AssetOwnerDetailsTests
	{
		private readonly ILogger _logger;
		private readonly IAssetOwnerServices _assetOwnerService;
		private readonly IAssetServices _assetServices;
		private AssetOwnerDetailsV1Controller _controller;
		ContainerBuilder _builder;
		private const string HeaderTokenJwt = "eyJ0eXAiOiJKV1QiLCJhbGciOiJTSEEyNTZ3aXRoUlNBIiwieDV0IjoiWW1FM016UTRNVFk0TkRVMlpEWm1PRGRtTlRSbU4yWmxZVGt3TVdFelltTmpNVGt6TURFelpnPT0ifQ==.eyJpc3MiOiJ3c28yLm9yZy9wcm9kdWN0cy9hbSIsImV4cCI6MTQ0ODAwNDEyODIyNCwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9zdWJzY3JpYmVyIjoiZGV2LXZzc2FkbWluQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbmlkIjoiNTYwIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbm5hbWUiOiJWU1AtTkRldiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBwbGljYXRpb250aWVyIjoiIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcGljb250ZXh0IjoiL3QvdHJpbWJsZS5jb20vdnNwLXFhLWlkZW50aXR5YXBpIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy92ZXJzaW9uIjoidjEiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3RpZXIiOiJVbmxpbWl0ZWQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2tleXR5cGUiOiJQUk9EVUNUSU9OIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy91c2VydHlwZSI6IkFQUExJQ0FUSU9OX1VTRVIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJkZXYtdmxjbGFzc2ljdXNlckB0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW5kdXNlclRlbmFudElkIjoiMSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW1haWxhZGRyZXNzIjoiQmhvb2JhbGFuX1BhbGFuaXZlbEBUcmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZmlyc3RuYW1lIjoiQmhvb2JhbGFuIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9naXZlbm5hbWUiOiJCaG9vYmFsYW4iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2lkZW50aXR5L2FjY291bnRMb2NrZWQiOiJmYWxzZSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdG5hbWUiOiJQYWxhbml2ZWwiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3JvbGUiOiIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3V1aWQiOiJhYTVkYTYyNy04OWVlLTRlYmUtYTE4Mi1hNzE3NzA1YWZmZDAifQ==.DLa86MVuE2nCuzLGCqgcw20/q5ikODPgDDwLiPO78RQpKwlqOG5Poa5gU2cyEYyDnYMiYW1M3Ffjh2icCBpnediyG2b5b9iNEHvBq+Y3Kiuc3qINva7fqU2Z1hk1afw+NvmQoYO9qVDXr8QXNUQNMEIRheJWQq0PMM+VD2IViEU=";

		public AssetOwnerDetailsTests()
		{
			_logger = Substitute.For<ILogger>();
			_assetOwnerService = Substitute.For<IAssetOwnerServices>();
			_assetServices = Substitute.For<IAssetServices>();

			_builder = new ContainerBuilder();
			var container = _builder.Build();
			_builder.Register(config => new AssetOwnerDetailsV1Controller(_assetOwnerService, _assetServices, _logger)).As<AssetOwnerDetailsV1Controller>();
			_controller = CreateControllerWithHeader(container, new ControllerContext(), _assetOwnerService, _assetServices, _logger);
		}
		private static AssetOwnerDetailsV1Controller CreateControllerWithHeader(IContainer container, ControllerContext mockHttpContext, IAssetOwnerServices _assetOwnerService, IAssetServices _assetServices, ILogger _logger)
		{
			var controller = new AssetOwnerDetailsV1Controller(_assetOwnerService, _assetServices, _logger);
			controller.ControllerContext = mockHttpContext;
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Request.Headers["X-JWT-Assertion"] = HeaderTokenJwt;
			return controller;
		}

		#region Get AssetOwner 
		[Fact]
		public void Get_Valid_GetAssetOwner()
		{
			//Arrange
			_assetOwnerService.CheckExistingAssetOwner(Arg.Any<Guid>()).Returns(true);
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

			_assetOwnerService.GetExistingAssetOwner(Arg.Any<Guid>()).Returns(x =>
				new AssetOwnerInfo { AccountName = "CAT", AccountUID = "a162eb79-0317-11e9-a988-029d68d36a0c", CustomerName = "TDOO", CustomerUID = "a162eb79-0317-11e9-a988-029d68d36a0c", DealerAccountCode = "DEL23", DealerName = "DEL", DealerUID = "a162eb79-0317-11e9-a988-029d68d36a0c", NetworkCustomerCode = "412", NetworkDealerCode = "123" }
			);

			var assetUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0c");

			//Act
			var response = _controller.GetAssetOwner(assetUID);

			//Assert
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
			Assert.Equal("CAT", ((VSS.MasterData.WebAPI.DbModel.AssetOwnerInfo)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value).AccountName);
			Assert.Equal("412", ((VSS.MasterData.WebAPI.DbModel.AssetOwnerInfo)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value).NetworkCustomerCode);
			Assert.Equal("123", ((VSS.MasterData.WebAPI.DbModel.AssetOwnerInfo)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value).NetworkDealerCode);
			Assert.Equal("TDOO", ((VSS.MasterData.WebAPI.DbModel.AssetOwnerInfo)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value).CustomerName);
			Assert.Equal("DEL", ((VSS.MasterData.WebAPI.DbModel.AssetOwnerInfo)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value).DealerName);
			Assert.Equal("a162eb79-0317-11e9-a988-029d68d36a0c", ((VSS.MasterData.WebAPI.DbModel.AssetOwnerInfo)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value).CustomerUID);
			Assert.Equal("a162eb79-0317-11e9-a988-029d68d36a0c", ((VSS.MasterData.WebAPI.DbModel.AssetOwnerInfo)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value).DealerUID);
		}


		[Fact]
		public void InvalidAssetUID_GetAssetOwner()
		{
			//Arrange
			_assetOwnerService.CheckExistingAssetOwner(Arg.Any<Guid>()).Returns(false);
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
			_assetOwnerService.GetExistingAssetOwner(Arg.Any<Guid>()).Returns(x =>
					new AssetOwnerInfo { }
			);

			var assetUID = Guid.Empty;

			//Act
			var response = _controller.GetAssetOwner(assetUID);


			Assert.Equal("AssetUID is mandatory", ((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value);
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void IntrnalServerError_GetAssetOwner()
		{
			//Arrange
			_assetOwnerService.CheckExistingAssetOwner(Arg.Any<Guid>()).Returns(x => throw new Exception());
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
			_assetOwnerService.GetExistingAssetOwner(Arg.Any<Guid>()).Returns(x =>
					new AssetOwnerInfo { }
			);

			var assetUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0c");

			//Act
			var response = _controller.GetAssetOwner(assetUID);
			Assert.Equal(500, ((Microsoft.AspNetCore.Mvc.StatusCodeResult)response).StatusCode);
		}


		[Fact]
		public void Get_InvalidAsset_GetAssetOwner()
		{
			//Arrange
			_assetOwnerService.CheckExistingAssetOwner(Arg.Any<Guid>()).Returns(false);
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
			_assetOwnerService.GetExistingAssetOwner(Arg.Any<Guid>()).Returns(x =>
					new AssetOwnerInfo { }
			);

			var assetUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0c");

			//Act
			var response = _controller.GetAssetOwner(assetUID);

	 
			Assert.Equal("No Such AssetUID exist", ((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value);
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void Unauthorized_GetAssetOwner()
		{
			//Arrange
			_assetOwnerService.CheckExistingAssetOwner(Arg.Any<Guid>()).Returns(true);
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
			_assetOwnerService.GetExistingAssetOwner(Arg.Any<Guid>()).Returns(x =>
					new AssetOwnerInfo { }
			);

			var assetUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0c");

			//Act
			var response = _controller.GetAssetOwner(assetUID);

			//Assert
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			//Arrange
			_controller.ControllerContext.HttpContext.Request.Headers["X-JWT-Assertion"] = "";
			_controller.ControllerContext.HttpContext.Request.Headers["X-VisionLink-UserUid"] = "";
			//Act
			response = _controller.GetAssetOwner(assetUID);

			//Assert
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		#endregion

		#region create/Update AssetOwner	

		[Fact]
		private void CreateAssetOwner()
		{
			_assetOwnerService.CheckExistingAssetOwner(Arg.Any<Guid>()).Returns(false);
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
			_assetOwnerService.CreateAssetOwnerEvent(Arg.Any<AssetOwnerEvent>()).Returns(true);
			var response = _controller.AssetOwner(assetOwnerEvent);
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.StatusCodeResult)response).StatusCode);
		}

		[Fact]
		private void CreateAssetOwner_ExistAsset()
		{
			_assetOwnerService.CheckExistingAssetOwner(Arg.Any<Guid>()).Returns(true);
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
			_assetOwnerService.CreateAssetOwnerEvent(Arg.Any<AssetOwnerEvent>()).Returns(true);
			_assetOwnerService.GetExistingAssetOwner(Arg.Any<Guid>()).Returns(x =>
				new AssetOwnerInfo { AccountName = "CAT", AccountUID = "a162eb79-0317-11e9-a988-029d68d36a0c", CustomerName = "TDOO", CustomerUID = "a162eb79-0317-11e9-a988-029d68d36a0c", DealerAccountCode = "DEL23", DealerName = "DEL", DealerUID = "a162eb79-0317-11e9-a988-029d68d36a0c", NetworkCustomerCode = "412", NetworkDealerCode = "123" }
			);
			_assetOwnerService.UpdateAssetOwnerEvent(Arg.Any<AssetOwnerEvent>()).Returns(true);
			var assetUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0c");


			var response = _controller.AssetOwner(assetOwnerEvent);
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.StatusCodeResult)response).StatusCode);
		}

		[Fact]
		private void UpdateAssetOwner()
		{

			_assetOwnerService.CheckExistingAssetOwner(Arg.Any<Guid>()).Returns(true);
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

			_assetOwnerService.GetExistingAssetOwner(Arg.Any<Guid>()).Returns(x =>
				new AssetOwnerInfo { AccountName = "CAT", AccountUID = "a162eb79-0317-11e9-a988-029d68d36a0c", CustomerName = "TDOO", CustomerUID = "a162eb79-0317-11e9-a988-029d68d36a0c", DealerAccountCode = "DEL23", DealerName = "DEL", DealerUID = "a162eb79-0317-11e9-a988-029d68d36a0c", NetworkCustomerCode = "412", NetworkDealerCode = "123" }
			);
			_assetOwnerService.UpdateAssetOwnerEvent(Arg.Any<AssetOwnerEvent>()).Returns(true);
			assetOwnerEvent.Action = Operation.Update;
			var response = _controller.AssetOwner(assetOwnerEvent);
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.StatusCodeResult)response).StatusCode);

			assetOwnerEvent.AssetOwnerRecord.AccountUID = null;
			assetOwnerEvent.AssetOwnerRecord.CustomerUID = null;
			assetOwnerEvent.AssetOwnerRecord.DealerUID = null;
			response = _controller.AssetOwner(assetOwnerEvent);
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.BadRequestObjectResult)response).StatusCode);
		}

		[Fact]
		private void UpdateAssetOwner_NoAssetExist()
		{

			_assetOwnerService.CheckExistingAssetOwner(Arg.Any<Guid>()).Returns(false);
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

			//_assetOwnerService.GetExistingAssetOwner(Arg.Any<Guid>()).Returns(x =>
			//	new AssetOwnerInfo { AccountName = "CAT", AccountUID = "a162eb79-0317-11e9-a988-029d68d36a0c", CustomerName = "TDOO", CustomerUID = "a162eb79-0317-11e9-a988-029d68d36a0c", DealerAccountCode = "DEL23", DealerName = "DEL", DealerUID = "a162eb79-0317-11e9-a988-029d68d36a0c", NetworkCustomerCode = "412", NetworkDealerCode = "123" }
			//);
			assetOwnerEvent.Action = Operation.Update;
			_assetOwnerService.CreateAssetOwnerEvent(Arg.Any<AssetOwnerEvent>()).Returns(true);
			var response = _controller.AssetOwner(assetOwnerEvent);
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.StatusCodeResult)response).StatusCode);
		}

		[Fact]
		private void UpdateAssetOwner_NewAsset()
		{
			_assetOwnerService.CheckExistingAssetOwner(Arg.Any<Guid>()).Returns(true);
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

			_assetOwnerService.GetExistingAssetOwner(Arg.Any<Guid>()).Returns(x =>
				new AssetOwnerInfo { AccountName = "CAT", AccountUID = "a162eb79-0317-11e9-a988-029d68d36a0c", CustomerName = "TDOO", CustomerUID = "a162eb79-0317-11e9-a988-029d68d36a0c", DealerAccountCode = "DEL23", DealerName = "DEL", DealerUID = "a162eb79-0317-11e9-a988-029d68d36a0c", NetworkCustomerCode = "412", NetworkDealerCode = "123" }
			);
			_assetOwnerService.UpdateAssetOwnerEvent(Arg.Any<AssetOwnerEvent>()).Returns(true);
			var response = _controller.AssetOwner(assetOwnerEvent);
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.StatusCodeResult)response).StatusCode);
		}


		[Fact]
		private void InternalSErverError_CreateAssetOwner()
		{
			_assetOwnerService.CheckExistingAssetOwner(Arg.Any<Guid>()).Returns(x => throw new Exception());
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
			_assetOwnerService.CreateAssetOwnerEvent(Arg.Any<AssetOwnerEvent>()).Returns(false);
			var response = _controller.AssetOwner(assetOwnerEvent);
			Assert.Equal(500, ((Microsoft.AspNetCore.Mvc.StatusCodeResult)response).StatusCode);
		}

		[Fact]
		private void CreateAssetOwner_BadRequest()
		{
			assetOwnerEvent.Action =Operation.Create ;
			assetOwnerEvent.AssetOwnerRecord.DealerUID = Guid.Empty;

			_assetOwnerService.CheckExistingAssetOwner(Arg.Any<Guid>()).Returns(false);
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
			_assetOwnerService.CreateAssetOwnerEvent(Arg.Any<AssetOwnerEvent>()).Returns(true);
			_assetOwnerService.UpdateAssetOwnerEvent(Arg.Any<AssetOwnerEvent>()).Returns(true);
			var response = _controller.AssetOwner(assetOwnerEvent);
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.StatusCodeResult)response).StatusCode);

			assetOwnerEvent.AssetOwnerRecord.AccountUID = Guid.Empty;
			assetOwnerEvent.AssetOwnerRecord.CustomerUID = Guid.Empty;
			assetOwnerEvent.AssetOwnerRecord.DealerUID = Guid.Empty;
			response = _controller.AssetOwner(assetOwnerEvent);
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.BadRequestObjectResult)response).StatusCode);

			assetOwnerEvent.AssetUID = Guid.Empty;
			response = _controller.AssetOwner(assetOwnerEvent);
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.BadRequestObjectResult)response).StatusCode);

		}
		
		[Fact]
		private void DeleteAssetOwner()
		{
			assetOwnerEvent.Action =Operation.Delete;
			_assetOwnerService.CheckExistingAssetOwner(Arg.Any<Guid>()).Returns(true);
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
			_assetOwnerService.DeleteAssetOwnerEvent(Arg.Any<AssetOwnerEvent>()).Returns(true);
			var response = _controller.AssetOwner(assetOwnerEvent);
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.StatusCodeResult)response).StatusCode);
		}

		[Fact]
		private void DeleteAssetOwner_BadRequest()
		{
			assetOwnerEvent.Action = Operation.Delete;
			_assetOwnerService.CheckExistingAssetOwner(Arg.Any<Guid>()).Returns(false);
			_assetServices.ValidateAuthorizedCustomerByAsset(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
			_assetOwnerService.DeleteAssetOwnerEvent(Arg.Any<AssetOwnerEvent>()).Returns(true);
			var response = _controller.AssetOwner(assetOwnerEvent);
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		#endregion

		private AssetOwnerEvent assetOwnerEvent = new AssetOwnerEvent()
		{
			Action = Operation.Create,
			ActionUTC = DateTime.UtcNow,
			AssetUID = Guid.NewGuid(),
			AssetOwnerRecord = new VSS.MasterData.WebAPI.ClientModel.AssetOwner()
			{
				AccountName = "Sam",
				AccountUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0c"),
				CustomerName = "Cat",
				CustomerUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0d"),
				DealerAccountCode = "TD00",
				DealerName = "DemoDeler",
				DealerUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0e"),
				NetworkCustomerCode = "SAP",
				NetworkDealerCode = "TeT",
			}
		};

	}
}
