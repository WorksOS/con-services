using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.Asset.Controllers.V1;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using Xunit;

namespace Asset.Tests
{
	public class SupportAssetTests
	{
		private readonly ILogger _logger;
		private readonly ISupportAssetServices _supportAssetServices;
		private SupportAssetV1Controller _controller;
		private readonly IAssetServices _assetRepository;
		private readonly IControllerUtilities _controllerUtilities;

		public SupportAssetTests()
		{
			_logger = Substitute.For<ILogger>();
			_supportAssetServices = Substitute.For<ISupportAssetServices>();
			_controllerUtilities = Substitute.For<IControllerUtilities>();
			_assetRepository = Substitute.For<IAssetServices>();
			_controller = new SupportAssetV1Controller(_assetRepository, _controllerUtilities, _logger, _supportAssetServices);//_supportAssetServices, _assetRepository, _controllerUtilities, _logger);

		}

		#region Get
		[Fact]
		public void GetAssetsForSupportUser()
		{
			_assetRepository.GetAssetsforSupportUser(Arg.Any<String>(), Arg.Any<int>(), Arg.Any<int>()).Returns(x =>
		   new object()
			);
			var response = _controller.GetAssetsForSupportUser("Test");
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}
		
		[Fact]
		public void GetAssetsForSupportUser_EmptySearchString()
		{
			_assetRepository.GetAssetsforSupportUser(Arg.Any<String>(), Arg.Any<int>(), Arg.Any<int>()).Returns(x =>
		   new object()
			);
			var response = _controller.GetAssetsForSupportUser();
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void InternalServerError_GetAssetsForSupportUser()
		{
			_assetRepository.GetAssetsforSupportUser(Arg.Any<String>(), Arg.Any<int>(), Arg.Any<int>()).Returns(x => throw new Exception());
			var response = _controller.GetAssetsForSupportUser("Test");
			Assert.Equal(500, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}


		[Fact]
		public void GetAssetDetail()
		{
			_assetRepository.GetAssetDetail(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(x =>
			new List<AssetDetail>() {
				new AssetDetail { AssetCustomerUIDs = "a162eb79-0317-11e9-a988-029d68d36a0i", AssetName = "tesr", AssetTypeName = "sa", AssetUID = "a162eb79-0317-11e9-a988-029d68d36a0e", DeviceSerialNumber = "Awr", DeviceState = "Subscribed", DeviceType = "PL121", DeviceUID = "a162eb79-0317-11e9-a988-029d68d36a0d", MakeCode = "CAT", Model = "TT", ModelYear = 2019, OwningCustomerUID = "a162eb79-0317-11e9-a988-029d68d36a0c", SerialNumber = "qww", TimestampOfModification = DateTime.Now}
			});

			_supportAssetServices.GetAssetCustomerByAssetGuid(Arg.Any<Guid>()).Returns(x =>
			new List<VSS.MasterData.WebAPI.ClientModel.AssetCustomer>() {
				new VSS.MasterData.WebAPI.ClientModel.AssetCustomer { CustomerName="AT", CustomerType="Delear", CustomerUID=Guid.NewGuid(), ParentCustomerType="Dealer", ParentCustomerUID=Guid.NewGuid(), ParentName="CAT" } }
			);

			_supportAssetServices.GetSubscriptionForAsset(Arg.Any<Guid>()).Returns(x =>
			new AssetSubscriptionModel() { AssetUID = Guid.NewGuid(), SubscriptionStatus = "Subscribed", OwnersVisibility = new List<OwnerVisibility> { new OwnerVisibility { CustomerName = "AT", CustomerType = "Delear", CustomerUID = Guid.NewGuid(), SubscriptionName = "Dealer", SubscriptionUID = Guid.NewGuid(), SubscriptionStatus = "Active", SubscriptionEndDate = DateTime.Now, SubscriptionStartDate = DateTime.Now } } }
			);
			var response = _controller.GetAssetDetail(Guid.NewGuid(), Guid.NewGuid());
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			// Null Responce
			_assetRepository.GetAssetDetail(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(x =>new List<AssetDetail>() {});
			response = _controller.GetAssetDetail(Guid.NewGuid(), Guid.NewGuid());
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
			Assert.Empty(((List<AssetDeviceDetail>)((Microsoft.AspNetCore.Mvc.ObjectResult)response).Value));
		
		}

		[Fact]
		public void GetAssetDetail_NoContent()
		{
			_assetRepository.GetAssetDetail(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(x =>
			new List<AssetDetail>() {
				new AssetDetail { AssetCustomerUIDs = "a162eb79-0317-11e9-a988-029d68d36a0i", AssetName = "tesr", AssetTypeName = "sa", AssetUID = "a162eb79-0317-11e9-a988-029d68d36a0e", DeviceSerialNumber = "Awr", DeviceState = "Subscribed", DeviceType = "PL121", DeviceUID = "a162eb79-0317-11e9-a988-029d68d36a0d", MakeCode = "CAT", Model = "TT", ModelYear = 2019, OwningCustomerUID = "a162eb79-0317-11e9-a988-029d68d36a0c", SerialNumber = "qww", TimestampOfModification = DateTime.Now}
			});

			_supportAssetServices.GetAssetCustomerByAssetGuid(Arg.Any<Guid>()).Returns(x =>
			new List<VSS.MasterData.WebAPI.ClientModel.AssetCustomer>() {
				new VSS.MasterData.WebAPI.ClientModel.AssetCustomer { CustomerName="AT", CustomerType="Delear", CustomerUID=Guid.NewGuid(), ParentCustomerType="Dealer", ParentCustomerUID=Guid.NewGuid(), ParentName="CAT" } }
			);

			_supportAssetServices.GetSubscriptionForAsset(Arg.Any<Guid>()).Returns(x =>
			new AssetSubscriptionModel() { AssetUID = Guid.NewGuid(), SubscriptionStatus = "Subscribed", OwnersVisibility = new List<OwnerVisibility> { new OwnerVisibility { CustomerName = "AT", CustomerType = "Delear", CustomerUID = Guid.NewGuid(), SubscriptionName = "Dealer", SubscriptionUID = Guid.NewGuid(), SubscriptionStatus = "Active", SubscriptionEndDate = DateTime.Now, SubscriptionStartDate = DateTime.Now } } }
			);
			var response = _controller.GetAssetDetail(Guid.NewGuid());
			Assert.Equal(204, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			response = _controller.GetAssetDetail(null,Guid.NewGuid());
			Assert.Equal(204, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);		 
		}

		[Fact]
		public void GetAssetDetail_NoAssetCustomer_No()
		{
			_assetRepository.GetAssetDetail(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(x =>
			new List<AssetDetail>() {
				new AssetDetail { AssetCustomerUIDs = "a162eb79-0317-11e9-a988-029d68d36a0i", AssetName = "tesr", AssetTypeName = "sa", AssetUID = "a162eb79-0317-11e9-a988-029d68d36a0e", DeviceSerialNumber = "Awr", DeviceState = "Subscribed", DeviceType = "PL121", DeviceUID = "a162eb79-0317-11e9-a988-029d68d36a0d", MakeCode = "CAT", Model = "TT", ModelYear = 2019, OwningCustomerUID = "a162eb79-0317-11e9-a988-029d68d36a0c", SerialNumber = "qww", TimestampOfModification = DateTime.Now}
			});

			_supportAssetServices.GetAssetCustomerByAssetGuid(Arg.Any<Guid>()).Returns(x =>
			new List<VSS.MasterData.WebAPI.ClientModel.AssetCustomer>() {
				new VSS.MasterData.WebAPI.ClientModel.AssetCustomer { CustomerName="AT", CustomerType="Delear", CustomerUID=Guid.NewGuid(), ParentCustomerType="Dealer", ParentCustomerUID=Guid.NewGuid(), ParentName="CAT" } }
			);

			_supportAssetServices.GetSubscriptionForAsset(Arg.Any<Guid>()).Returns(x =>
			new AssetSubscriptionModel() { AssetUID = Guid.NewGuid(), SubscriptionStatus = "Subscribed", OwnersVisibility = new List<OwnerVisibility> { new OwnerVisibility { CustomerName = "AT", CustomerType = "Delear", CustomerUID = Guid.NewGuid(), SubscriptionName = "Dealer", SubscriptionUID = Guid.NewGuid(), SubscriptionStatus = "Active", SubscriptionEndDate = DateTime.Now, SubscriptionStartDate = DateTime.Now } } }
			);
			var response = _controller.GetAssetDetail(Guid.NewGuid());
			Assert.Equal(204, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			response = _controller.GetAssetDetail(null, Guid.NewGuid());
			Assert.Equal(204, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);

			_supportAssetServices.GetAssetCustomerByAssetGuid(Arg.Any<Guid>()).Returns(x => throw new Exception());
			_supportAssetServices.GetSubscriptionForAsset(Arg.Any<Guid>()).Returns(x => throw new Exception());

			response = _controller.GetAssetDetail(Guid.NewGuid(), Guid.NewGuid());
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}


		[Fact]
		public void GetAssetDetail_InternalServerError()
		{
			_assetRepository.GetAssetDetail(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(x => throw new Exception());
			_supportAssetServices.GetAssetCustomerByAssetGuid(Arg.Any<Guid>()).Returns(x => throw new Exception());
			_supportAssetServices.GetSubscriptionForAsset(Arg.Any<Guid>()).Returns(x => throw new Exception());

			var response = _controller.GetAssetDetail(Guid.NewGuid(), Guid.NewGuid());
			Assert.Equal(500, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);


		}

		[Fact]
		public void GetAssetDetail_BadRequest()
		{
			var response = _controller.GetAssetDetail();
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void GetAssetDeviceDetails_BadRequest()
		{
			var assetUIDs = new AssetDeviceRequest() { AssetUIDs = new List<string>() { "a162eb79-0317-11e9-a988-029d68d36a0i", "a162eb79-0317-11e9-a988-029d68d36a0g" } };			 
			var response = _controller.GetAssetDeviceDetails(assetUIDs);
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		
			_controllerUtilities.ValidateAssetUIDParameters(Arg.Any<string[]>()).Returns(x => throw new Exception());

			 response = _controller.GetAssetDeviceDetails(assetUIDs);
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void GetAssetDeviceDetails_InternalServerError()
		{
			var assetUIDs = new AssetDeviceRequest() { AssetUIDs = new List<string>() { "a162eb79-0317-11e9-a988-029d68d36a0i", "a162eb79-0317-11e9-a988-029d68d36a0g" } };
			_controllerUtilities.ValidateAssetUIDParameters(Arg.Any<string[]>()).Returns( x => throw new Exception());
		 
			var response = _controller.GetAssetDeviceDetails(assetUIDs);
			Assert.Equal(400, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);
		}

		[Fact]
		public void GetAssetDeviceDetails()
		{
			var assetUIDs = new AssetDeviceRequest() { AssetUIDs = new List<string>() {Guid.NewGuid().ToString(), Guid.NewGuid().ToString() } };
			_supportAssetServices.GetAssetDetailFromAssetGuids(Arg.Any<List<Guid>>()).Returns(x => new List<AssetDetail>() {
				new AssetDetail { AssetCustomerUIDs = "a162eb79-0317-11e9-a988-029d68d36a0i", AssetName = "tesr", AssetTypeName = "sa", AssetUID = "a162eb79-0317-11e9-a988-029d68d36a0e", DeviceSerialNumber = "Awr", DeviceState = "Subscribed", DeviceType = "PL121", DeviceUID = "a162eb79-0317-11e9-a988-029d68d36a0d", MakeCode = "CAT", Model = "TT", ModelYear = 2019, OwningCustomerUID = "a162eb79-0317-11e9-a988-029d68d36a0c", SerialNumber = "qww", TimestampOfModification = DateTime.Now }
			});
			_controllerUtilities.ValidateAssetUIDParameters(Arg.Any<string[]>()).Returns(x => new Guid[] { Guid.NewGuid(), Guid.NewGuid() });
			var response = _controller.GetAssetDeviceDetails(assetUIDs);
			Assert.Equal(200, ((Microsoft.AspNetCore.Mvc.ObjectResult)response).StatusCode);			 
		}
		#endregion
	}
}
