using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.AssetRepository;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Transactions;
using Xunit;

namespace AssetRepository.Tests
{
	public class SupportAssetServicesTests
	{
		private readonly ILogger _logger;
		private readonly ITransactions _transaction;
		private ISupportAssetServices _supportAssetServices;
		public SupportAssetServicesTests()
		{
			_logger = Substitute.For<ILogger>();
			_transaction = Substitute.For<ITransactions>();
			_supportAssetServices = new SupportAssetServices(_transaction, _logger);
		}

		[Fact]
		public void GetNullAssetCustomerByAssetGuid()
		{
			//Arrange
			_transaction.Get<AssetCustomer>(Arg.Any<string>()).Returns(x => null);
			//Act
			var assetCustomer = _supportAssetServices.GetAssetCustomerByAssetGuid(new Guid());
			//Assert
			Assert.Null(assetCustomer);
		}
		[Fact]
		public void GetValidAssetCustomerByAssetGuid()
		{
			//Arrange
			_transaction.Get<AssetCustomer>(Arg.Any<string>()).Returns(x =>
			{
				return new List<AssetCustomer>() { new AssetCustomer() {
					CustomerName ="TestCustomer4220719",
					CustomerType ="0",
					CustomerUID= new Guid("004B97E3AC4D11E9812B061307E34288"),
					ParentCustomerType ="1",
					ParentCustomerUID =new Guid("EFB56414AC4C11E9812B061307E34288"),
					ParentName ="TestDealer4220719"
				},
				new AssetCustomer() {
					CustomerName ="JhonDeer",
					CustomerType ="1",
					CustomerUID= new Guid("EFB56414AC4C11E9812B061307E34288"),
					ParentCustomerType ="1",
					ParentCustomerUID =new Guid("EFB56414AC4C11E9812B061307E34288"),
					ParentName ="TestDealer4220719"
				}
				};
			});

			//Act 
			var assetCustomer = _supportAssetServices.GetAssetCustomerByAssetGuid(new Guid("B34DBEF6AC4E11E9812B061307E34288"));
			Assert.Equal("TestCustomer4220719", assetCustomer[0].CustomerName);
			Assert.Equal("Customer", assetCustomer[0].CustomerType);
			Assert.Equal("Customer", assetCustomer[0].ParentCustomerType);
			Assert.Equal("TestDealer4220719", assetCustomer[0].ParentName);
		}

		[Fact]
		public void GetSubscriptionForAsset_Null()
		{
			//Arrange
			_transaction.Get<OwnerVisibility>(Arg.Any<string>()).Returns(x => new List<OwnerVisibility>() { null});

			//Act 
			var subscription = _supportAssetServices.GetSubscriptionForAsset(new Guid());
			//Assert
			Assert.Null(subscription.AssetUID);
		}
		[Fact]
		public void GetValidGetSubscriptionForAsset()
		{
			//Arrange
			var assetUID= Guid.NewGuid();
			var SubscriptionUID = Guid.NewGuid();
			var CustomerUID = Guid.NewGuid();
			_transaction.Get<OwnerVisibility>(Arg.Any<string>()).Returns(x =>
			 new List<OwnerVisibility>() {
				 new OwnerVisibility() {
					  CustomerName="ATT",
					  CustomerType="Customer",
					  CustomerUID=CustomerUID,
					  SubscriptionEndDate=DateTime.UtcNow.AddDays(2),
					  SubscriptionName="CAT Esential",
					  SubscriptionStartDate=DateTime.UtcNow,
					  SubscriptionStatus="active",
					  SubscriptionUID=SubscriptionUID
			 },
				 new OwnerVisibility() {
					  CustomerName="jhondeer",
					  CustomerType="dealer",
					  CustomerUID=CustomerUID,
					  SubscriptionEndDate=DateTime.UtcNow.AddDays(2),
					  SubscriptionName="cat essentials - 10 minutes",
					  SubscriptionStartDate=DateTime.UtcNow,
					  SubscriptionStatus="inactive",
					  SubscriptionUID=SubscriptionUID
			 } });


			_transaction.Get<DbCustomer>(Arg.Any<string>()).Returns(x => new List<DbCustomer>() { new DbCustomer {
				BSSID = "wrer",
				CustomerID = 123123,
				CustomerName = "werwer",
				DealerAccountCode = "werwer",
				CustomerUID=CustomerUID,
				DealerNetwork = "werwe",
				FirstName = "CUs",
				fk_CustomerTypeID = 2423,
				IsActive = true,
				LastCustomerUTC = DateTime.UtcNow,
				LastName = "gdfgdf",
				NetworkCustomerCode = "qeqwe",
				NetworkDealerCode = "qweqw",
				PrimaryContactEmail = "qweqwe"
			}}); 

			//Act
			var subscription = _supportAssetServices.GetSubscriptionForAsset(assetUID);
			Assert.Equal(assetUID.ToString(), subscription.AssetUID.ToString());
			Assert.Equal(2, subscription.OwnersVisibility.Count);
		}

		[Fact]
		public void GetAssetDetailFromAssetGuids()
		{
			var assetUID = Guid.NewGuid();
			var CustomerUID = Guid.NewGuid();
			//Arrange
			_transaction.Get<AssetDetail>(Arg.Any<string>()).Returns(x=> new List<AssetDetail>{ new AssetDetail{
				AssetCustomerUIDs= "a162eb79-0317-11e9-a988-029d68d36a0w",
				AssetName="SD1222",
				AssetTypeName="Volve",
				AssetUID="a162eb79-0317-11e9-a988-029d68d36a0c",
				DeviceSerialNumber="TV123wrw",
				DeviceState="Provisioned",
				DeviceType="PL321",
				DeviceUID="a162eb79-0317-11e9-a988-029d68d36a0f",
				MakeCode="CAT",
				Model="DTO",
				ModelYear=2018,
				OwningCustomerUID="a162eb79-0317-11e9-a988-029d68d36a0q",
				SerialNumber="SER34lsd222222",
				TimestampOfModification=DateTime.UtcNow
			}
			} );

			//Act
			var subscription = _supportAssetServices.GetAssetDetailFromAssetGuids(new List<Guid>() { new Guid(), new Guid() });
			Assert.Single(subscription);
		}

		[Fact]
		public void GetAssetDetailFromAssetGuids_Exception()
		{
			var assetUID = Guid.NewGuid();
			var CustomerUID = Guid.NewGuid();

			//Arrange
			_transaction.Get<AssetDetail>(Arg.Any<string>()).Returns(x => throw new Exception());

			//Act
			var subscription = _supportAssetServices.GetAssetDetailFromAssetGuids(new List<Guid>() { new Guid(), new Guid() });
			Assert.Empty(subscription);
		}
	}
}
