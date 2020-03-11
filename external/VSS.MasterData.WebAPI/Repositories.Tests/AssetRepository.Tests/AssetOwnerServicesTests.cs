using KafkaModel;
using Microsoft.Extensions.Configuration;
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
using static VSS.MasterData.WebAPI.Utilities.Enums.Enums;

namespace AssetRepository.Tests
{
	public class AssetOwnerServicesTests
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger _logger;
		private readonly ITransactions _transaction;
		private IAssetOwnerServices _assetOwnerServices;

		public AssetOwnerServicesTests()
		{
			_configuration = Substitute.For<IConfiguration>();
			_logger = Substitute.For<ILogger>();
			_transaction = Substitute.For<ITransactions>();
			_configuration["AssetOwnerTopicName"] = "VSS.Interfaces.Events.MasterData.IAssetOwnerEvent.V1-Alpha,VSS.Interfaces.Events.MasterData.IAssetOwnerEvent.V2-Alpha";
			_assetOwnerServices = new AssetOwnerServices(_transaction, _configuration, _logger);
			string[] topics = _configuration["AssetOwnerTopicName"].Split(',');
			_transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				return true;
			});
		}
				
		[Theory]
		[InlineData ("1", true)]
		[InlineData("0", false)]
		public void Check_ExistingAssetOwner(string avail, bool isExist )
		{
			//Arrange
			_transaction.Get<string>(Arg.Any<string>()).Returns(x => {
				return new List<string>() { avail };
			});

			//Act 
			var result = _assetOwnerServices.CheckExistingAssetOwner(new Guid());

			//Assert
			Assert.Equal(result, isExist);
		}

		[Fact]
		public void Get_ExistingAssetOwner()
		{
			//Arrange
			_transaction.Get<AssetOwnerInfo>(Arg.Any<string>()).Returns(x => {
				return new List<AssetOwnerInfo> () { new AssetOwnerInfo(){
					AccountName = "Sam",
					AccountUID = "a162eb79-0317-11e9-a988-029d68d36a0c",
					CustomerName = "Cat",
					CustomerUID = "a162eb79-0317-11e9-a988-029d68d36a0d",
					DealerAccountCode = "TD00",
					DealerName = "DemoDeler",
					DealerUID = "a162eb79-0317-11e9-a988-029d68d36a0e",
					NetworkCustomerCode = "SAP",
					NetworkDealerCode = "TeT"}
				};
			});

			//Act 
			var result = _assetOwnerServices.GetExistingAssetOwner(new Guid());

			//Assert
			Assert.Equal("TeT", result.NetworkDealerCode);
			Assert.Equal("Sam", result.AccountName);
			Assert.Equal("a162eb79-0317-11e9-a988-029d68d36a0c", result.AccountUID);
			Assert.Equal("Cat", result.CustomerName);
			Assert.Equal("a162eb79-0317-11e9-a988-029d68d36a0d", result.CustomerUID);
			Assert.Equal("TD00", result.DealerAccountCode);
			Assert.Equal("DemoDeler", result.DealerName);
			Assert.Equal("a162eb79-0317-11e9-a988-029d68d36a0e", result.DealerUID);
			Assert.Equal("SAP", result.NetworkCustomerCode);
		}

		[Fact]
		public void Create_Update_Delete_AssetOwnerEvent()
		{
			//Arrange for Create

			var assetOwnerEvent = new AssetOwnerEvent() {
				Action = Operation.Create,
				ActionUTC = DateTime.UtcNow,
				AssetUID = new Guid(),
				AssetOwnerRecord= new VSS.MasterData.WebAPI.ClientModel.AssetOwner() {
				AccountName = "Sam",
				AccountUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0c"),
				CustomerName = "Cat",
				CustomerUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0d"),
				DealerAccountCode = "TD00",
				DealerName = "DemoDeler",
				DealerUID = new Guid("a162eb79-0317-11e9-a988-029d68d36a0e"),
				NetworkCustomerCode = "SAP",
				NetworkDealerCode = "TeT",}
			};

			_transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				foreach (var action in x.Arg<List<Action>>())
					action();
				return true;
			});

			//Act 
			Assert.True(_assetOwnerServices.CreateAssetOwnerEvent(assetOwnerEvent));

			//Arrange for Update
			assetOwnerEvent.Action = Operation.Update;

			//Act
			Assert.True(_assetOwnerServices.UpdateAssetOwnerEvent(assetOwnerEvent));

			//Arrange for Delete
			assetOwnerEvent.Action = Operation.Delete;

			//Act
			Assert.True(_assetOwnerServices.DeleteAssetOwnerEvent(assetOwnerEvent));
		}
	}
}
