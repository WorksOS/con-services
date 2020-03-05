using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.AssetRepository;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Transactions;
using Xunit;
using static VSS.MasterData.WebAPI.Utilities.Enums.Enums;

namespace AssetRepository.Tests
{
	public class AssetECMServicesTests
	{
		private readonly IConfiguration _configuration;
		private readonly ITransactions _transaction;
		private IAssetECMInfoServices _assetECMServices;

		public AssetECMServicesTests()
		{
			_configuration = Substitute.For<IConfiguration>();
			_transaction = Substitute.For<ITransactions>();
			_assetECMServices = new AssetECMServices(_transaction, _configuration);
		}

		[Fact]
		public void GetNullECMInfo()
		{
			//Arrange
			_transaction.Get<AssetECM>(Arg.Any<string>()).Returns(x => {
				return new List<AssetECM>() { new AssetECM() };
			});
			
			//Act 
			var ecmData = _assetECMServices.GetAssetECMInfo(new Guid());

			//Assert
			Assert.Null(ecmData[0].AssetECMInfoUID);
		}

		[Fact]
		public void GetValidECMInfo()
		{
			//Arrange
			_transaction.Get<AssetECM>(Arg.Any<string>()).Returns(x => {
				return new List<AssetECM>() { new AssetECM() {
					MID = "330",
					DataLink = DataLinkEnum.CDL,
					Description = string.Empty,
					J1939Name = string.Empty,
					SoftwarePartNumber = "test part number",
					SourceAddress = string.Empty,
					SerialNumber = "TestSerialNumber",
					PartNumber = "test part number",
					SyncClockEnabled = true,
					SyncClockLevel = true,
					AssetECMInfoUID="1BA4AB7D-B456-11E9-812B-061307E34288"
				} };
			});

			//Act 
			var ecmData = _assetECMServices.GetAssetECMInfo(new Guid());

			//Assert
			Assert.Equal("1BA4AB7D-B456-11E9-812B-061307E34288", ecmData[0].AssetECMInfoUID);
			Assert.Equal(DataLinkEnum.CDL, ecmData[0].DataLink);
			Assert.Equal("", ecmData[0].Description);
			Assert.Equal("", ecmData[0].J1939Name);
			Assert.Equal("test part number", ecmData[0].SoftwarePartNumber);
			Assert.Equal("", ecmData[0].SourceAddress);
			Assert.Equal("TestSerialNumber", ecmData[0].SerialNumber);
			Assert.Equal("test part number", ecmData[0].PartNumber);
			Assert.Equal("330", ecmData[0].MID);
		}
	}
}
