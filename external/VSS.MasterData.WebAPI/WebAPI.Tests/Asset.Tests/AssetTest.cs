using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using VSS.MasterData.WebAPI.Asset.Controllers.V1;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.VisionLink.SearchAndFilter.Client.v1_6.Interfaces;
using VSS.VisionLink.SearchAndFilter.Interfaces.v1_6.DataContracts;
using Xunit;

namespace Asset.Tests
{
	public class AssetTest
	{
		private readonly ILogger _logger;
		private readonly IConfiguration _configuration;
		private readonly IControllerUtilities _controllerUtilities;
		private readonly IAssetServices _assetServices;
		private readonly ISearchAndFilter _searchAndFilterClient;
		private AssetV1Controller _controller;
		ContainerBuilder _builder;
		private const string HeaderTokenJwt = "eyJ0eXAiOiJKV1QiLCJhbGciOiJTSEEyNTZ3aXRoUlNBIiwieDV0IjoiWW1FM016UTRNVFk0TkRVMlpEWm1PRGRtTlRSbU4yWmxZVGt3TVdFelltTmpNVGt6TURFelpnPT0ifQ==.eyJpc3MiOiJ3c28yLm9yZy9wcm9kdWN0cy9hbSIsImV4cCI6MTQ0ODAwNDEyODIyNCwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9zdWJzY3JpYmVyIjoiZGV2LXZzc2FkbWluQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbmlkIjoiNTYwIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbm5hbWUiOiJWU1AtTkRldiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBwbGljYXRpb250aWVyIjoiIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcGljb250ZXh0IjoiL3QvdHJpbWJsZS5jb20vdnNwLXFhLWlkZW50aXR5YXBpIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy92ZXJzaW9uIjoidjEiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3RpZXIiOiJVbmxpbWl0ZWQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2tleXR5cGUiOiJQUk9EVUNUSU9OIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy91c2VydHlwZSI6IkFQUExJQ0FUSU9OX1VTRVIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJkZXYtdmxjbGFzc2ljdXNlckB0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW5kdXNlclRlbmFudElkIjoiMSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW1haWxhZGRyZXNzIjoiQmhvb2JhbGFuX1BhbGFuaXZlbEBUcmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZmlyc3RuYW1lIjoiQmhvb2JhbGFuIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9naXZlbm5hbWUiOiJCaG9vYmFsYW4iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2lkZW50aXR5L2FjY291bnRMb2NrZWQiOiJmYWxzZSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdG5hbWUiOiJQYWxhbml2ZWwiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3JvbGUiOiIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3V1aWQiOiJhYTVkYTYyNy04OWVlLTRlYmUtYTE4Mi1hNzE3NzA1YWZmZDAifQ==.DLa86MVuE2nCuzLGCqgcw20/q5ikODPgDDwLiPO78RQpKwlqOG5Poa5gU2cyEYyDnYMiYW1M3Ffjh2icCBpnediyG2b5b9iNEHvBq+Y3Kiuc3qINva7fqU2Z1hk1afw+NvmQoYO9qVDXr8QXNUQNMEIRheJWQq0PMM+VD2IViEU=";
		
		public AssetTest()
		{
			_logger = Substitute.For<ILogger>();
			_assetServices = Substitute.For<IAssetServices>();
			_controllerUtilities = Substitute.For<IControllerUtilities>();
			_searchAndFilterClient = Substitute.For<ISearchAndFilter>();
			string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			_configuration = new ConfigurationBuilder().SetBasePath(currentDirectory)
													.AddXmlFile("app.config.xml", true)
													.AddJsonFile("appsettings.json", true)
													.AddEnvironmentVariables()
													.Build();
			_builder = new ContainerBuilder();
			var container = _builder.Build();
			_builder.Register(config => new AssetV1Controller(_assetServices, _controllerUtilities, _configuration, _logger, _searchAndFilterClient)).As<AssetOwnerDetailsV1Controller>();
			_controller = CreateControllerWithHeader(container, new ControllerContext(), _assetServices, _controllerUtilities, _configuration, _logger, _searchAndFilterClient);
		}
		private static AssetV1Controller CreateControllerWithHeader(IContainer container, ControllerContext mockHttpContext, IAssetServices _assetServices, IControllerUtilities _controllerUtilities, IConfiguration _configuration, ILogger _logger, ISearchAndFilter _searchAndFilterClient)
		{
			var controller = new AssetV1Controller(_assetServices, _controllerUtilities, _configuration, _logger, _searchAndFilterClient);
			controller.ControllerContext = mockHttpContext;
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			controller.ControllerContext.HttpContext.Request.Headers["X-JWT-Assertion"] = HeaderTokenJwt;
			return controller;
		}
		#region Get

    //		 GetAssetByLegacyAssetID
		[Fact]
		public void GetAssetByLegacyAssetID()
		{
			_assetServices.GetAssetByAssetLegacyID(Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<Guid?>(), Arg.Any<long?>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<string>()).Returns(x =>
			new List<LegacyAssetData>() { new LegacyAssetData { AssetName = "TA210801", AssetUID = "d97b84ed-35f7-4de7-8fe5-2e4f4dfca18e", DeviceSerialNumber = "Dev", DeviceType = "PL121", EquipmentVIN = "Eevent", LegacyAssetID = 132, MakeCode = "CAT", MakeName = "TDOO", Model = "2019", ModelYear = "2012", ProductFamily = "CAT", SerialNumber = "Ser123" } }
			);
			var response = _controller.GetAssetByLegacyAssetID(Guid.NewGuid().ToString(), Guid.NewGuid(), false, false, "fdf", 1, "wer", "were", false);
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			_controller.ControllerContext.HttpContext.Request.Headers["X-JWT-Assertion"] = HeaderTokenJwt;
		}

		[Fact]
		public void GetAssetByLegacyAssetID_NotFound()
		{
			//_assetServices.GetAssetByAssetLegacyID(Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<Guid?>(), Arg.Any<long?>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<string>()).Returns(x =>
			//	new List<LegacyAssetData>() { new LegacyAssetData { AssetName = "TA210801", AssetUID = "d97b84ed-35f7-4de7-8fe5-2e4f4dfca18e", DeviceSerialNumber = "Dev", DeviceType = "PL121", EquipmentVIN = "Eevent", LegacyAssetID = 132, MakeCode = "CAT", MakeName = "TDOO", Model = "2019", ModelYear = "2012", ProductFamily = "CAT", SerialNumber = "Ser123" } }
			//);

			_assetServices.GetAssetByAssetLegacyID(Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<Guid?>(), Arg.Any<long?>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<string>()).Returns(x =>
			new List<LegacyAssetData>() );
			var response = _controller.GetAssetByLegacyAssetID(Guid.NewGuid().ToString(), Guid.NewGuid(), false, false, "fdf", 1, "wer", "were", false);
			Assert.Equal(404, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			// GetAssetByAssetLegacyID Exception
			_assetServices.GetAssetByAssetLegacyID(Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<Guid?>(), Arg.Any<long?>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<string>()).Returns(x =>throw new Exception());
			 response = _controller.GetAssetByLegacyAssetID(Guid.NewGuid().ToString(), Guid.NewGuid(), false, false, "fdf", 1, "wer", "were", false);
			Assert.Equal(500, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void GetAssetByLegacyAssetID_BadReqest()
		{
			_assetServices.GetAssetByAssetLegacyID(Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<Guid?>(), Arg.Any<long?>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<string>()).Returns(x =>
			null
			);
			var response = _controller.GetAssetByLegacyAssetID(Guid.NewGuid().ToString(), Guid.NewGuid(), false, false, "fdf", 1, "wer", "were", false);
			Assert.Equal(404, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			//UserID Not Provide
			response = _controller.GetAssetByLegacyAssetID(Guid.NewGuid().ToString(), null, false, false, "fdf", 1, "wer", "were", false);
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			//Invalid assetIDorAssetUID
			response = _controller.GetAssetByLegacyAssetID("dfgdg", Guid.NewGuid(), false, false, "fdf", 1, "wer", "were", false);
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

	//		 GetAssetByLegacyAssetID Ends

		[Fact]
		public void GetAsset_CustomerNotMapped()
		{
			string[] assetUID = { Guid.NewGuid().ToString() };
			_assetServices.GetCustomersForApplication(Arg.Any<string>()).Returns(x =>
			new List<Guid>() { new Guid("07D4A55244C5E311AA7700505688274D") }
			);
			var response = _controller.GetAssets(assetUID, Guid.NewGuid().ToString(), new string[] { "Type" }, new string[] { "Active" }, new string[] { "CAT" }, new string[] { "Model" }, "SN", "10", "1", Guid.NewGuid(), Guid.NewGuid());
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
			Assert.Equal("Application does not have this customer mapped. Please contact your API administrator.", ((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value);

		}

		[Fact]
		public void GetAssets()
		{

			_assetServices.GetCustomersForApplication(Arg.Any<string>()).Returns(x =>
			new List<Guid> { new Guid("7a0d9d17-b55c-4a59-af43-579e2588d9ba") }
			);

			_assetServices.GetAssetsForCustomer(Arg.Any<List<Guid>>(), Arg.Any<int>(), Arg.Any<int>()).Returns(x =>
		  new CustomerAssetsListData
		  {
			  PageNumber = 2,
			  TotalNumberOfPages = 2,
			  TotalRowsCount = 10,
			  CustomerAssets = new List<CustomerAsset>() { new CustomerAsset { AssetName = "AN", AssetTypeName = "Type", AssetUID = Guid.NewGuid(), EquipmentVIN = "VIN", IconKey = 1, LegacyAssetID = 10, MakeCode = "Make", Model = "2011", ModelYear = 2019, OwningCustomerUID = Guid.NewGuid(), SerialNumber = "SN", StatusInd = true },
			  new CustomerAsset { AssetName = "AN", AssetTypeName = "Type", AssetUID = Guid.NewGuid(), EquipmentVIN = "VIN", IconKey = 1, LegacyAssetID = 10, MakeCode = "Make", Model = "2011", ModelYear = 2019, OwningCustomerUID = Guid.NewGuid(), SerialNumber = "SN", StatusInd = true }
			  }
		  });

			_searchAndFilterClient.QueryFleet(Arg.Any<AssetFleetQueryParameters>()).Returns(x =>
			new AssetFleetResponseParameters { AssetUIDs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() } }
			);

			_assetServices.GetAssets(Arg.Any<Guid[]>(), Arg.Any<Guid>()).Returns(x =>
			new List<VSS.MasterData.WebAPI.DbModel.Asset>() {
				new VSS.MasterData.WebAPI.DbModel.Asset {AssetUID="a162eb79-0317-11e9-a988-029d68d36a0c", SerialNumber="wer", MakeCode="et"},
				new VSS.MasterData.WebAPI.DbModel.Asset {AssetUID="a162eb79-0317-11e9-a988-029d68d36a0q", SerialNumber="rew", MakeCode="ert"}
			});

			string[] assetUID = { Guid.NewGuid().ToString() };
			var response = _controller.GetAssets(assetUID, "7a0d9d17-b55c-4a59-af43-579e2588d9ba", new string[] { "Type" }, new string[] { "Active" }, new string[] { "CAT" }, new string[] { "Model" }, "SN", "10", "1", Guid.NewGuid(), Guid.NewGuid());
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
			Assert.Equal(2, ((CustomerAssetsListData)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value).CustomerAssets.Count);
			Assert.Equal(2, ((VSS.MasterData.WebAPI.DbModel.CustomerAssetsListData)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value).PageNumber);
			Assert.Equal(10, ((VSS.MasterData.WebAPI.DbModel.CustomerAssetsListData)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value).TotalRowsCount);
		}

		[Fact]
		public void GetAssets_UsingSearchandFilter()
		{
			int pageSizeInt;
			int pageNumberInt;
			_controllerUtilities.ValidatePageParameters(Arg.Any<string>(), Arg.Any<string>(), out pageSizeInt, out pageNumberInt);
			_assetServices.GetCustomersForApplication(Arg.Any<string>()).Returns(x => null);

			_controllerUtilities.ValidateAssetUIDParameters(Arg.Any<string[]>()).Returns(x => new Guid[] { Guid.NewGuid() });
			_searchAndFilterClient.QueryFleet(Arg.Any<AssetFleetQueryParameters>()).Returns(x => new AssetFleetResponseParameters() { AssetUIDs = new Guid[] { Guid.NewGuid() } });

			_assetServices.GetAssets(Arg.Any<Guid[]>(), Arg.Any<Guid>()).Returns(x =>
			new List<VSS.MasterData.WebAPI.DbModel.Asset>() {
				new VSS.MasterData.WebAPI.DbModel.Asset {AssetUID="ceea76f5-84de-49c2-b3fb-7463b9697f9c", SerialNumber="2FMPK4J96HBB29959", MakeCode="FA1", AssetName="", AssetTypeName="qwe", EquipmentVIN="45", IconKey=1, LegacyAssetID=0, Model="rt", ModelYear=2017, StatusInd=true },
				new VSS.MasterData.WebAPI.DbModel.Asset {AssetUID="f29ec652-7e35-464a-a246-d869ad233e69", SerialNumber="2FMPK4J96HBB29950", MakeCode="FA2", AssetName="", AssetTypeName="erw", EquipmentVIN="345", IconKey=1, LegacyAssetID=0, Model="rtt", ModelYear=2017, StatusInd=true }
			});

			var response = _controller.GetAssets(null, "7a0d9d17-b55c-4a59-af43-579e2588d9ba", null, null, null, null, "57d", "10", "1", null, null);
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			
			_controllerUtilities.ValidateAssetUIDParameters(Arg.Any<string[]>()).Returns(x => new Guid[] { } );
			response = _controller.GetAssets(null, "7a0d9d17-b55c-4a59-af43-579e2588d9ba", null, null, null, null, "57d", "10", "1", null, null);
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}


		[Fact]
		public void GetAssets_BadRequest()
		{
			_assetServices.GetCustomersForApplication(Arg.Any<string>()).Returns(x => new List<Guid>() { new Guid("8abcf851-44c5-e311-aa77-00505688274d") });

			string[] assetUID = { Guid.NewGuid().ToString() };
			var response = _controller.GetAssets(assetUID, "7a0d9d17-b55c-4a59-af43-579e2588d9ba", new string[] { "Type" }, new string[] { "Active" }, new string[] { "CAT" }, new string[] { "Model" }, "SN", "10", "1", Guid.NewGuid(), Guid.NewGuid());
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			_assetServices.GetCustomersForApplication(Arg.Any<string>()).Returns(x => null);
			_controllerUtilities.ValidateAssetUIDParameters(Arg.Any<string[]>()).Returns(x => throw new Exception());
			 response = _controller.GetAssets(assetUID, "7a0d9d17-b55c-4a59-af43-579e2588d9ba", new string[] { "Type" }, new string[] { "Active" }, new string[] { "CAT" }, new string[] { "Model" }, "SN", "10", "1", Guid.NewGuid(), Guid.NewGuid());
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void GetAssets_SearchandFilter_Exception()
		{
			int pageSizeInt;
			int pageNumberInt;
			_controllerUtilities.ValidatePageParameters(Arg.Any<string>(), Arg.Any<string>(), out pageSizeInt, out pageNumberInt);
			_assetServices.GetCustomersForApplication(Arg.Any<string>()).Returns(x => null);
			
			_searchAndFilterClient.QueryFleet(Arg.Any<AssetFleetQueryParameters>()).Returns(x => throw new Exception());

			_assetServices.GetAssets(Arg.Any<Guid[]>(), Arg.Any<Guid>()).Returns(x =>
			new List<VSS.MasterData.WebAPI.DbModel.Asset>() {
				new VSS.MasterData.WebAPI.DbModel.Asset {AssetUID="ceea76f5-84de-49c2-b3fb-7463b9697f9c", SerialNumber="2FMPK4J96HBB29959", MakeCode="FA1", AssetName="", AssetTypeName="qwe", EquipmentVIN="45", IconKey=1, LegacyAssetID=0, Model="rt", ModelYear=2017, StatusInd=true },
				new VSS.MasterData.WebAPI.DbModel.Asset {AssetUID="f29ec652-7e35-464a-a246-d869ad233e69", SerialNumber="2FMPK4J96HBB29950", MakeCode="FA2", AssetName="", AssetTypeName="erw", EquipmentVIN="345", IconKey=1, LegacyAssetID=0, Model="rtt", ModelYear=2017, StatusInd=true }
			});

			_controllerUtilities.ValidateAssetUIDParameters(Arg.Any<string[]>()).Returns(x => new Guid[] { });
			var response = _controller.GetAssets(null, "7a0d9d17-b55c-4a59-af43-579e2588d9ba", null, null, null, null, "57d", "10", "1", null, null);
			Assert.Equal(500, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}


		[Fact]
		public void Get3PDataAssets()
		{

			_assetServices.GetHarvesterAssets().Returns(x =>
					   new List<object>() { new object(), new object() }
			);

			var response = _controller.Get3PDataAssets(Guid.NewGuid());

			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
			Assert.Equal(2, ((System.Collections.Generic.List<object>)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value).Count);


			response = _controller.Get3PDataAssets();
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			_assetServices.GetHarvesterAssets().Returns(x => throw new Exception());
			response = _controller.Get3PDataAssets(Guid.NewGuid());
			Assert.Equal(500, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}


		[Fact]
		public void HttpPost_GetAssets()
		{
			_assetServices.GetAssets(Arg.Any<Guid[]>(), Arg.Any<Guid>()).Returns(x =>
			new List<VSS.MasterData.WebAPI.DbModel.Asset>() {
				new VSS.MasterData.WebAPI.DbModel.Asset {AssetUID="ceea76f5-84de-49c2-b3fb-7463b9697f9c", SerialNumber="2FMPK4J96HBB29959", MakeCode="FA1", AssetName="", AssetTypeName="qwe", EquipmentVIN="45", IconKey=1, LegacyAssetID=0, Model="rt", ModelYear=2017, StatusInd=true },
				new VSS.MasterData.WebAPI.DbModel.Asset {AssetUID="f29ec652-7e35-464a-a246-d869ad233e69", SerialNumber="2FMPK4J96HBB29950", MakeCode="FA2", AssetName="", AssetTypeName="erw", EquipmentVIN="345", IconKey=1, LegacyAssetID=0, Model="rtt", ModelYear=2017, StatusInd=true }
			});

			// Valid Responce
			var assetParam = new AssetListParam() { AssetUIDs = new List<string>() { "ceea76f5-84de-49c2-b3fb-7463b9697f9c", "f29ec652-7e35-464a-a246-d869ad233e69" } };
			_controllerUtilities.ValidateAssetUIDParameters(Arg.Any<string[]>()).Returns(x => new Guid[] { Guid.NewGuid() });

			var response = _controller.GetAssets(assetParam, Guid.NewGuid());
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			_controllerUtilities.ValidateAssetUIDParameters(Arg.Any<string[]>()).Returns(x=> null);

			response = _controller.GetAssets(assetParam, Guid.NewGuid());
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
			
			//Bad Request for UserGuid null
 			response = _controller.GetAssets(assetParam);
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			//Internal Server Error
			_controllerUtilities.ValidateAssetUIDParameters(Arg.Any<string[]>()).Returns(x => new Guid[] { Guid.NewGuid() });

			_assetServices.GetAssets(Arg.Any<Guid[]>(), Arg.Any<Guid>()).Returns(x => throw new Exception());
			response = _controller.GetAssets(assetParam, Guid.NewGuid());
			Assert.Equal(500, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			_controllerUtilities.ValidateAssetUIDParameters(Arg.Any<string[]>()).Returns(x =>  throw new Exception());
			response = _controller.GetAssets(assetParam, Guid.NewGuid());
			Assert.Equal(500, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			


		}

		
		#endregion

		#region Create/Update/Delete
		[Fact]
		public void CreateAsset()
		{
			_assetServices.GetAssetUid(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(x => null);
			_assetServices.GetCustomersForApplication(Arg.Any<string>()).Returns(x => new List<Guid>() { new Guid("8abcf851-44c5-e311-aa77-00505688274d") });
			_assetServices.CreateAsset(Arg.Any<CreateAssetEvent>()).Returns(x => true);
			_assetServices.IsValidMakeCode(Arg.Any<String>()).Returns(x => true);
			var response = _controller.CreateAsset(GetCreateAssetEvent());
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void CreateAsset_InvalidToken()
		{
			_assetServices.GetAssetUid(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(x => null);
			_assetServices.GetCustomersForApplication(Arg.Any<string>()).Returns(x => new List<Guid>() { new Guid("8abcf851-44c5-e311-aa77-00505688274d") });
			_assetServices.CreateAsset(Arg.Any<CreateAssetEvent>()).Returns(x => true);
			_assetServices.IsValidMakeCode(Arg.Any<String>()).Returns(x => true);
			_controller.ControllerContext.HttpContext.Request.Headers["X-JWT-Assertion"] = "";
			var response = _controller.CreateAsset(GetCreateAssetEvent());
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}
				 

		[Fact]
		public void CreateAssetWithInvalidMake()
		{
			_assetServices.GetAssetUid(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(x => null);
			_assetServices.GetCustomersForApplication(Arg.Any<string>()).Returns(x => new List<Guid>() { new Guid("8abcf851-44c5-e311-aa77-00505688274d") });
			_assetServices.CreateAsset(Arg.Any<CreateAssetEvent>()).Returns(x => true);
			_assetServices.IsValidMakeCode(Arg.Any<String>()).Returns(x => false);
			var response = _controller.CreateAsset(GetCreateAssetEvent());
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void CreateAsset_Already_Exists()
		{
			_assetServices.GetAssetUid(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(x => Guid.NewGuid());
			_assetServices.GetCustomersForApplication(Arg.Any<string>()).Returns(x => new List<Guid>() { new Guid("8abcf851-44c5-e311-aa77-00505688274d") });
			_assetServices.CreateAsset(Arg.Any<CreateAssetEvent>()).Returns(x => true);
			var response = _controller.CreateAsset(GetCreateAssetEvent());
			Assert.Equal(409, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void CreateAsset_NotMappedCustomer()
		{
			_assetServices.GetCustomersForApplication(Arg.Any<string>()).Returns(x => null);
			var response = _controller.CreateAsset(GetCreateAssetEvent());
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			_assetServices.GetCustomersForApplication(Arg.Any<string>()).Returns(x => throw new Exception());
			response = _controller.CreateAsset(GetCreateAssetEvent());
			Assert.Equal(500, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void UpdateAsset()
		{
			_assetServices.GetAssetUid(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(x => null);
			_assetServices.GetCustomersForApplication(Arg.Any<string>()).Returns(x => new List<Guid>() { new Guid("8abcf851-44c5-e311-aa77-00505688274d") });
			_assetServices.UpdateAsset(Arg.Any<UpdateAssetEvent>()).Returns(x => true);
			_assetServices.GetAsset(Arg.Any<Guid>()).Returns(x => new VSS.MasterData.WebAPI.ClientModel.Asset { StatusInd = 1, AssetName = "EWR" });
			var request = GetUpdateAssetEvent();
			request.LegacyAssetID = 0;
			var response = _controller.UpdateAssetInfo(request);
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

		}

		[Fact]
		public void UpdateAsset_BadRequest()
		{
			_assetServices.GetAssetUid(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(x => null);
			_assetServices.GetCustomersForApplication(Arg.Any<string>()).Returns(x => new List<Guid>() { new Guid("8abcf851-44c5-e311-aa77-00505688274d") });
			_assetServices.UpdateAsset(Arg.Any<UpdateAssetEvent>()).Returns(x => true);
			_assetServices.GetAsset(Arg.Any<Guid>()).Returns(x => null);

			var response = _controller.UpdateAssetInfo(GetNoUpdateAssetEvent());
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			response = _controller.UpdateAssetInfo(GetUpdateAssetEvent());
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			_assetServices.GetAsset(Arg.Any<Guid>()).Returns(x => new VSS.MasterData.WebAPI.ClientModel.Asset { StatusInd = 1, AssetName = "EWR" });
			_assetServices.UpdateAsset(Arg.Any<UpdateAssetEvent>()).Returns(x => false);
			response = _controller.UpdateAssetInfo(GetUpdateAssetEvent());
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			_controller.ControllerContext.HttpContext.Request.Headers["X-JWT-Assertion"] = "";
			response = _controller.UpdateAssetInfo(GetUpdateAssetEvent());
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);


		}

		[Fact]
		public void UpdateAsset_InternalServerError()
		{
			_assetServices.GetAssetUid(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(x => null);
			_assetServices.GetCustomersForApplication(Arg.Any<string>()).Returns(x => new List<Guid>() { new Guid("8abcf851-44c5-e311-aa77-00505688274d") });
			_assetServices.UpdateAsset(Arg.Any<UpdateAssetEvent>()).Returns(x => true);
			_assetServices.GetAsset(Arg.Any<Guid>()).Returns(x => throw new Exception());
			var response = _controller.UpdateAssetInfo(GetUpdateAssetEvent());
			Assert.Equal(500, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}
		[Fact]
		public void UpdateAsset_NotMappedCustomer()
		{
			_assetServices.GetCustomersForApplication(Arg.Any<string>()).Returns(x => null);
			var response = _controller.UpdateAssetInfo(GetUpdateAssetEvent());
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void UpdateAsset_AssetDoesNotExist()
		{
			_assetServices.GetCustomersForApplication(Arg.Any<string>()).Returns(x => new List<Guid>() { new Guid("8abcf851-44c5-e311-aa77-00505688274d") });
			_assetServices.GetAsset(Arg.Any<Guid>()).Returns(x => null);
			var response = _controller.UpdateAssetInfo(GetUpdateAssetEvent());
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void DeleteAsset()
		{
			_assetServices.GetAsset(Arg.Any<Guid>()).Returns(x => new VSS.MasterData.WebAPI.ClientModel.Asset { StatusInd = 1, AssetName = "EWR" });
			_assetServices.DeleteAsset(Arg.Any<DeleteAssetPayload>()).Returns(true);
			var response = _controller.DeleteAsset(Guid.NewGuid(), DateTime.Now);
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}
		[Fact]
		public void DeleteAsset_AssetDoesNotExists()
		{
			_assetServices.GetAsset(Arg.Any<Guid>()).Returns(x => new VSS.MasterData.WebAPI.ClientModel.Asset { StatusInd = 0, AssetName = "EWR" });
			var response = _controller.DeleteAsset(Guid.NewGuid(), DateTime.Now);
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			response = _controller.DeleteAsset(null, DateTime.Now);
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);	 
		}

		[Fact]
		public void DeleteAsset_BadRequest()
		{
			_assetServices.GetAsset(Arg.Any<Guid>()).Returns(x => new VSS.MasterData.WebAPI.ClientModel.Asset { StatusInd = 0, AssetName = "EWR" });
			var response = _controller.DeleteAsset(Guid.Empty, DateTime.Now);
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			_assetServices.DeleteAsset(Arg.Any<DeleteAssetPayload>()).Returns(false);
			response = _controller.DeleteAsset(Guid.NewGuid(), DateTime.Now);
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void InternalServer_DeleteAsset()
		{
			_assetServices.GetAsset(Arg.Any<Guid>()).Returns(x => new VSS.MasterData.WebAPI.ClientModel.Asset { StatusInd = 1, AssetName = "EWR" });
			_assetServices.DeleteAsset(Arg.Any<DeleteAssetPayload>()).Returns(x => throw new Exception());
			var response = _controller.DeleteAsset(Guid.NewGuid(), DateTime.Now);
			Assert.Equal(500, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		#endregion

		private CreateAssetEvent GetCreateAssetEvent()
		{
			return new CreateAssetEvent
			{
				AssetUID = Guid.NewGuid(),
				LegacyAssetID = 11111111,
				AssetName = "testAsset",
				SerialNumber = "testSNO",
				EquipmentVIN = "testEquipmentVIN",
				AssetType = "testFamily",
				MakeCode = "CAT",
				Model = "testModel",
				ModelYear = 5,
				IconKey = 1,
				ActionUTC = DateTime.UtcNow
			};
		}

		private UpdateAssetEvent GetUpdateAssetEvent()
		{
			return new UpdateAssetEvent
			{
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				LegacyAssetID = 11111111,
				AssetName = "testAsset",
				EquipmentVIN = "testEquipmentVIN",
				AssetType = "testFamily",
				Model = "testModel",
				ModelYear = 5,
				IconKey = 1
			};
		}

		private UpdateAssetEvent GetNoUpdateAssetEvent()
		{
			return new UpdateAssetEvent
			{
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				LegacyAssetID = -9999999,
				AssetName = "$#$#$",
				EquipmentVIN = "$#$#$",
				AssetType = "$#$#$",
				Model = "$#$#$",
				ModelYear = -9999999,
				IconKey = -9999999,
				OwningCustomerUID = Guid.Empty,
				Category = "$#$#$",
				ProjectStatus = "$#$#$",
				Classification = "$#$#$",
				ObjectType= "$#$#$",
				PlanningGroup= "$#$#$",
				SortField= "$#$#$",
				 Source= "$#$#$",
				 UserEnteredRuntimeHours= "$#$#$"
			};

		}

		private UpdateAssetSourceEvent GetUpdateAssetSourceEvent()
		{
			return new UpdateAssetSourceEvent
			{
				ActionUTC = DateTime.Now,
				ReceivedUTC = DateTime.Now,
				AssetSources = new List<AssetSource> {
					 new AssetSource{ AssetUID=Guid.NewGuid(), Source="MasterData"},
					 new AssetSource{ AssetUID=Guid.NewGuid(), Source="MasterData"},
					 new AssetSource{ AssetUID=Guid.NewGuid(), Source="MasterData"}
				 }
			};
		}
		 
	}
}