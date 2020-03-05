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

namespace AssetRepository.Tests
{
	public class AssetRepositoryTests
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger _logger;
		private readonly ITransactions _transaction;
		private IAssetServices _assetServices;

		public AssetRepositoryTests()
		{
			_configuration = Substitute.For<IConfiguration>();
			_logger = Substitute.For<ILogger>();
			_transaction = Substitute.For<ITransactions>();
			_configuration["AssetTopicNames"] = "VSS.Interfaces.Events.MasterData.IAssetEvent-Alpha,VSS.Interfaces.Events.MasterData.IAssetEvent.V1-Alpha";
			_assetServices = new AssetServices(_transaction, _configuration, _logger);
			string[] topics = _configuration["AssetTopicNames"].Split(',');
		}	

		[Fact]
		public void Get_GetAssetDetail()
		{
			//Arrange
			_transaction.Get<AssetDetail>(Arg.Any<string>()).Returns(x => {
				return new List<AssetDetail>() { new AssetDetail()
				{
					AssetName ="TestAsset",
					AssetTypeName ="AS",
					AssetUID = "a162eb79-0317-11e9-a988-029d68d36a0c",
					DeviceSerialNumber="TestDevice",
					AssetCustomerUIDs="a162eb79-0317-11e9-a988-029d68d36a0t",
					DeviceState="0",
					DeviceType="PL640",
					DeviceUID="a162eb79-0317-11e9-a988-029d68d36a0r",
					MakeCode="DAT",
					Model="JJJ",
					ModelYear=2019,
					OwningCustomerUID="a162eb79-0317-11e9-a988-029d68d36a0y",
					SerialNumber="SerialNo1",
					TimestampOfModification=DateTime.Now,
				}
				};
			});

			//Act 
			var assetServices = (List<AssetDetail>)_assetServices.GetAssetDetail(Guid.NewGuid(), Guid.NewGuid());

			//Assert
			Check(assetServices);

			//Act
			assetServices = (List<AssetDetail>)_assetServices.GetAssetDetail(Guid.NewGuid());
			//Assert
			Check(assetServices);

			//Act
			assetServices = (List<AssetDetail>)_assetServices.GetAssetDetail(null, Guid.NewGuid());
			//Assert
			Check(assetServices);
		}

		private void Check(List<AssetDetail> assetServices)
		{
			//Assert
			Assert.Equal("TestAsset", assetServices[0].AssetName);
			Assert.Equal("a162eb79-0317-11e9-a988-029d68d36a0r", assetServices[0].DeviceUID);
			Assert.Equal("a162eb79-0317-11e9-a988-029d68d36a0y", assetServices[0].OwningCustomerUID);
			Assert.Equal("a162eb79-0317-11e9-a988-029d68d36a0t", assetServices[0].AssetCustomerUIDs);
			Assert.Equal("SerialNo1", assetServices[0].SerialNumber);
			Assert.Equal("JJJ", assetServices[0].Model);
			Assert.Equal("DAT", assetServices[0].MakeCode);
			Assert.Equal("PL640", assetServices[0].DeviceType);
			Assert.Equal("TestDevice", assetServices[0].DeviceSerialNumber);
		}

		[Fact]
		public void GetHarvesterAssets()
		{
			//Arrange
			_transaction.Get<VSS.MasterData.WebAPI.DbModel.Asset>(Arg.Any<string>()).Returns(x => {
				return new List<VSS.MasterData.WebAPI.DbModel.Asset>() { new VSS.MasterData.WebAPI.DbModel.Asset()
				{
					AssetName ="TestAsset",
					AssetTypeName ="AS",
					AssetUID = "a162eb79-0317-11e9-a988-029d68d36a0c",
					EquipmentVIN= "TestDevice",
					MakeCode="DAT",
					Model="JJJ",
					ModelYear=2019,
					SerialNumber="SerialNo1",
					IconKey=1,
					LegacyAssetID=2631204
				}
				};
			});

			//Act 
			var assetServices = _assetServices.GetHarvesterAssets();

			//Assert
			Assert.Single(assetServices);
		}

		[Fact]
		public void GetAssets()
		{
			//Arrange
			_transaction.Get<VSS.MasterData.WebAPI.DbModel.Asset>(Arg.Any<string>()).Returns(x => {
				return new List<VSS.MasterData.WebAPI.DbModel.Asset>() { new VSS.MasterData.WebAPI.DbModel.Asset()
				{
					AssetName ="TestAsset",
					AssetTypeName ="AS",
					AssetUID = "a162eb79-0317-11e9-a988-029d68d36a0c",
					EquipmentVIN= "TestDevice",
					MakeCode="CAT",
					Model="JJJ",
					ModelYear=2019,
					SerialNumber="SerialNo1",
					IconKey=1,
					LegacyAssetID=2631204
				}
				};
			});

			var assetGuid = new Guid[] { Guid.NewGuid() };
			//Act 
			var assetServices = _assetServices.GetAssets(assetGuid, Guid.NewGuid());

			//Assert
			Assert.Equal("TestAsset", assetServices[0].AssetName);
			Assert.Equal("AS", assetServices[0].AssetTypeName);
			Assert.Equal("CAT", assetServices[0].MakeCode);
			Assert.Equal("JJJ", assetServices[0].Model);
			Assert.Equal(2019, assetServices[0].ModelYear);
			Assert.Equal("SerialNo1", assetServices[0].SerialNumber);
		}

		[Fact]
		public void GetAssetByAssetLegacyID()
		{
			//Arrange
			_transaction.Get<LegacyAssetData>(Arg.Any<string>()).Returns(x => {
				return new List<LegacyAssetData>() { new LegacyAssetData()
				{
					AssetName ="TestAsset",
					MakeName ="AS",
					AssetUID = "a162eb79-0317-11e9-a988-029d68d36a0c",
					EquipmentVIN = "TestDevice",
					MakeCode = "CAT",
					Model = "JJJ",
					ModelYear = "2019",
					SerialNumber = "SerialNo1",
					DeviceType = "PL640",
					DeviceSerialNumber = "DS",
					ProductFamily = "CAT",
					LegacyAssetID = 2631204
				}
				};
			});

			var assetGuid = new Guid[] { Guid.NewGuid() };
			//Act 

			var assetServices = _assetServices.GetAssetByAssetLegacyID(Guid.NewGuid());

			//Assert
			Assert.Equal("DS", assetServices[0].DeviceSerialNumber);
			Assert.Equal("CAT", assetServices[0].ProductFamily);
			Assert.Equal("TestAsset", assetServices[0].AssetName);
			Assert.Equal("PL640", assetServices[0].DeviceType);
			Assert.Equal("CAT", assetServices[0].MakeCode);
			Assert.Equal("JJJ", assetServices[0].Model);
			Assert.Equal("2019", assetServices[0].ModelYear);
			Assert.Equal("SerialNo1", assetServices[0].SerialNumber);
		}
		
		[Theory]
		[InlineData ("1", true)]
		[InlineData("0", false)]
		public void ValidateAuthorizedCustomerByAsset(string isExist, bool result)
		{
			//Arrange
			_transaction.Get<string>(Arg.Any<string>()).Returns(x => {
				return new List<string>() { isExist };
			});
			//Act & Assert
			Assert.Equal(result, _assetServices.ValidateAuthorizedCustomerByAsset(Guid.NewGuid(), Guid.NewGuid()));
		}

		[Fact]		
		public void GetAssetUid()
		{
			//Arrange
			_transaction.Get<string>(Arg.Any<string>()).Returns(x => {
				return new List<string>() { "a162eb79-0317-11e9-a988-029d68d36a0c" };
			});
			//Act 
			var uid = _assetServices.GetAssetUid(Guid.NewGuid(), "CAT", "SN123");
			//Assert
			Assert.Equal("a162eb79-0317-11e9-a988-029d68d36a0c", uid.ToString() );

			//Arrange
			_transaction.Get<string>(Arg.Any<string>()).Returns(x => {
				return new List<string>() { null};
			});
			//Act 
			uid = _assetServices.GetAssetUid(Guid.NewGuid(), "CAT", "SN123");
			//Assert
			Assert.Null(uid);
		}

		[Fact]
		public void IsValidMakeCode()
		{
			//Arrange
			_transaction.Get<string>(Arg.Any<string>()).Returns(x => {
				return new List<string>() { "CAT" };
			});
			//Act  
			//Assert
			Assert.True(_assetServices.IsValidMakeCode("CAT"));

			//Arrange
			_transaction.Get<string>(Arg.Any<string>()).Returns(x => {
				return new List<string>() {  };
			});
			//Act 
		  
			//Assert
			Assert.False(_assetServices.IsValidMakeCode("CAT"));
		}


		[Fact]
		public void GetCustomersForApplication()
		{
			//Arrange
			_transaction.Get<string>(Arg.Any<string>()).Returns(x => {
				return new List<string>() { "a162eb79-0317-11e9-a988-029d68d36a0c" };
			});
			//Act 
			var uid = _assetServices.GetCustomersForApplication("MasterData");
			//Assert
			Assert.Single(uid);
			Assert.Equal("a162eb79-0317-11e9-a988-029d68d36a0c", uid[0].ToString());
		}

		[Fact]
		public void Create_Asset()
		{
			//Arrange for Create

			var assetObject = new CreateAssetEvent()
			{
				AssetName = "TestAssetName",
				AssetType = "loader",
				SerialNumber = "TestSerialNumber",
				AssetUID = Guid.NewGuid(),
				MakeCode = "TestMake",
				Model = "model",
				EquipmentVIN = "equipmentVIN",
				IconKey = 1,
				LegacyAssetID = 1,
				ActionUTC = DateTime.UtcNow,
				ModelYear = 2016,
				ReceivedUTC = DateTime.UtcNow,
				OwningCustomerUID = Guid.NewGuid(),
				ObjectType = "test",
				Category = "test",
				ProjectStatus = "test",
				SortField = "test",
				Source = "test",
				Classification = "test",
				PlanningGroup = "test",
				UserEnteredRuntimeHours = "1234"
			};

			_transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				foreach (var action in x.Arg<List<Action>>())
					action();
				return true;
			});

			//Act 
			Assert.True(_assetServices.CreateAsset(assetObject));
		}

		[Fact]
		public void Update_Asset()
		{
			//Arrange for Create

			var upadteAssetObject = new UpdateAssetEvent()
			{
				AssetName = "TestAssetName",
				AssetType = "loader",
				AssetUID = Guid.NewGuid(),
				Model = "model",
				EquipmentVIN = "equipmentVIN",
				IconKey = 1,
				LegacyAssetID = 1,
				ActionUTC = DateTime.UtcNow,
				ModelYear = 2016,
				ReceivedUTC = DateTime.UtcNow,
				OwningCustomerUID = Guid.NewGuid(),
				ObjectType = "test",
				Category = "test",
				ProjectStatus = "test",
				SortField = "test",
				Source = "test",
				Classification = "test",
				PlanningGroup = "test",
				UserEnteredRuntimeHours = "1234"
			};

			VSS.MasterData.WebAPI.ClientModel.Asset getAssetObject = new VSS.MasterData.WebAPI.ClientModel.Asset
			{
				AssetUID= upadteAssetObject.AssetUID.ToString(),
				OwningCustomerUID= upadteAssetObject.OwningCustomerUID.ToString(),
				AssetName = "TestAssetName",
				AssetType = "loader",
				SerialNumber = "TestSerialNumber",
				MakeCode = "TestMake",
				Model = "model",
				EquipmentVIN = "equipmentVIN",
				IconKey = 1,
				LegacyAssetID = 1,
				InsertUTC = DateTime.UtcNow,
				ModelYear = 2016,
				UpdateUTC = DateTime.UtcNow,
				ObjectType = "test",
				Category = "test",
				ProjectStatus = "test",
				SortField = "test",
				Source = "test",
				Classification = "test",
				PlanningGroup = "test",
				UserEnteredRuntimeHours = "1234",
				StatusInd = 1
				};

			_transaction.Get<VSS.MasterData.WebAPI.ClientModel.Asset>(Arg.Any<string>()).Returns(x =>
			{
				return new List<VSS.MasterData.WebAPI.ClientModel.Asset>() { getAssetObject };
			});

			_transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				foreach (var action in x.Arg<List<Action>>())
					action();
				return true;
			});

			//Act 
			Assert.True(_assetServices.UpdateAsset(upadteAssetObject));
		}

		[Fact]
		public void Delete_Asset()
		{
			//Arrange for Create

			var assetObject = new DeleteAssetPayload()
			{
				AssetUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow,
			};

			_transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				foreach (var action in x.Arg<List<Action>>())
					action();
				return true;
			});

			//Act 
			Assert.True(_assetServices.DeleteAsset(assetObject));
		}
	}
}