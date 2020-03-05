using KafkaModel;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using VSS.MasterData.WebAPI.Customer.KafkaModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Transactions;
using Xunit;

namespace VSS.MasterData.WebAPI.CustomerRepository.Tests
{
	public class CustomerAssetServiceTests
	{
		private readonly ILogger logger;
		private readonly IConfiguration configuration;
		private readonly ITransactions transaction;
		private readonly ICustomerAssetService customerAssetService;
		private static List<string> CustomerTopics;
		public CustomerAssetServiceTests()
		{
			logger = Substitute.For<ILogger>();
			transaction = Substitute.For<ITransactions>();
			string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			configuration = new ConfigurationBuilder().SetBasePath(currentDirectory)
													.AddJsonFile("appsettings.json", true)
													.AddEnvironmentVariables()
													.Build();
			customerAssetService = new CustomerAssetService(transaction, configuration, logger);
			CustomerTopics = configuration["CustomerTopicNames"]
				.Split(',')
				.Select(t => t + configuration["TopicSuffix"])
				.ToList();
		}

		#region GetAssetCustomer
		[Fact]
		public void GetAssetCustomer_ValidAssetCustomer_ReturnsResult()
		{
			//Arrange
			var assetCustomer = new DbAssetCustomer
			{
				AssetCustomerID = 1,
				Fk_CustomerUID = Guid.NewGuid(),
				Fk_AssetUID = Guid.NewGuid(),
				fk_AssetRelationTypeID = 0,
				LastCustomerUTC = DateTime.UtcNow
			};
			var config = new ComparisonConfig
			{
				IgnoreObjectTypes = true,
				MaxMillisecondsDateDifference = 500,
				MaxDifferences = 0
			};
			var assetCustomerCompareLogic = new CompareLogic(config);
			transaction.Get<DbAssetCustomer>(Arg.Any<string>()).Returns(new List<DbAssetCustomer> { assetCustomer });

			//Act
			var resultData = customerAssetService.GetAssetCustomer(assetCustomer.Fk_CustomerUID, assetCustomer.Fk_AssetUID);

			//Arrange
			Assert.NotNull(resultData);
			ComparisonResult compareResult = assetCustomerCompareLogic.Compare(assetCustomer, resultData);
			Assert.True(compareResult.Differences.Count == 0);
		}

		[Fact]
		public void GetAssetCustomer_InValidCustomerAsset_EmptyResult()
		{
			//Arrange
			var customerUid = Guid.NewGuid();
			var assetUid = Guid.NewGuid();
			transaction.Get<DbAssetCustomer>(Arg.Any<string>()).Returns(new List<DbAssetCustomer>());

			//Act
			var result = customerAssetService.GetAssetCustomer(customerUid, assetUid);

			//Arrange
			Assert.Null(result);
		}
		#endregion

		#region GetAssetCustomerByRelationType
		[Fact]
		public void GetAssetCustomerByRelationType_ValidCustomerAsset_ReturnsResult()
		{
			//Arrange
			var assetCustomer = new DbAssetCustomer
			{
				AssetCustomerID = 1,
				Fk_CustomerUID = Guid.NewGuid(),
				Fk_AssetUID = Guid.NewGuid(),
				fk_AssetRelationTypeID = 1,
				LastCustomerUTC = DateTime.UtcNow
			};
			var config = new ComparisonConfig
			{
				IgnoreObjectTypes = true,
				MaxMillisecondsDateDifference = 500,
				MaxDifferences = 0
			};
			var assetCustomerCompareLogic = new CompareLogic(config);
			transaction.Get<DbAssetCustomer>(Arg.Any<string>()).Returns(new List<DbAssetCustomer> { assetCustomer });

			//Act
			var resultData = customerAssetService.GetAssetCustomer(assetCustomer.Fk_CustomerUID, assetCustomer.Fk_AssetUID);

			//Arrange
			Assert.NotNull(resultData);
			ComparisonResult compareResult = assetCustomerCompareLogic.Compare(assetCustomer, resultData);
			Assert.True(compareResult.Differences.Count == 0);
		}

		[Fact]
		public void GetAssetCustomerByRelationType_InValidCustomerAsset_EmptyResult()
		{
			//Arrange
			var customerUid = Guid.NewGuid();
			var assetUid = Guid.NewGuid();
			transaction.Get<DbAssetCustomer>(Arg.Any<string>()).Returns(new List<DbAssetCustomer>());

			//Act
			var result = customerAssetService.GetAssetCustomer(customerUid, assetUid);

			//Arrange
			Assert.Null(result);
		}
		#endregion

		#region AssociateCustomerAsset
		[Theory]
		[InlineData(true, true, 1, 1, false, "Owner")]
		[InlineData(true, true, 1, 1, false, "0")]
		[InlineData(false, false, 0, 0, false, "Owner")]
		[InlineData(false, false, 0, 0, true, "Owner")]
		[InlineData(true, true, 1, 1, false, "Customer")]
		[InlineData(true, true, 1, 1, false, "1")]
		[InlineData(true, true, 1, 1, false, "Dealer")]
		[InlineData(true, true, 1, 1, false, "2")]
		[InlineData(true, true, 1, 1, false, "Operations")]
		[InlineData(true, true, 1, 1, false, "3")]
		[InlineData(true, true, 1, 1, false, "Corporate")]
		[InlineData(true, true, 1, 1, false, "4")]
		[InlineData(true, true, 1, 1, false, "SharedOwner")]
		[InlineData(true, true, 1, 1, false, "5")]
		public void AssociateCustomerAsset_ValidPayload_ExpectedTransactionStatus(
			bool hasValidCustomer, bool transactionStatus, int upsertCalls, int publishCalls, bool hasException,
			string relType)
		{
			//Arrange
			var customerUid = Guid.NewGuid();
			var assetUid = Guid.NewGuid();
			var assetCustomerEvent = new AssociateCustomerAssetEvent
			{
				CustomerUID = customerUid,
				AssetUID = assetUid,
				RelationType = relType,
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			DbCustomer customerData = hasValidCustomer ? new DbCustomer() { CustomerID = 109 } : null;
			transaction.Get<DbCustomer>(Arg.Any<string>()).Returns(new List<DbCustomer> { customerData });
			transaction.Execute(Arg.Any<List<Action>>())
				.Returns(a =>
				{
					if (hasException)
					{
						a.Arg<List<Action>>().ForEach(action => action.Returns(e => throw new Exception()));
						return false;
					}
					else
					{
						a.Arg<List<Action>>().ForEach(action => action.Invoke());
						return true;
					}
				});

			//Act
			var resultData = customerAssetService.AssociateCustomerAsset(assetCustomerEvent);

			//Assert
			Assert.Equal(transactionStatus, resultData);
			transaction.Received(upsertCalls).Upsert(
				Arg.Is<DbAssetCustomer>(assetCust => ValidateCustomerAssetObject(assetCustomerEvent, assetCust)));
			transaction.Received(publishCalls).Publish(
				Arg.Is<List<KafkaMessage>>(messages => messages
				.TrueForAll(m => ValidateCustomerAssetKafkaObject(false, m, assetCustomerEvent))));
		}
		#endregion

		#region DissociateCustomerAsset
		[Theory]
		[InlineData(true, 1, 1, false)]
		[InlineData(false, 0, 0, true)]
		public void DissociateCustomerAsset_ValidPayload_ExpectedTransactionStatus(
			bool transactionStatus, int deleteCalls, int publishCalls, bool hasException)
		{
			//Arrange
			var customerUid = Guid.NewGuid();
			var assetUid = Guid.NewGuid();
			var assetCustomerEvent = new DissociateCustomerAssetEvent
			{
				CustomerUID = customerUid,
				AssetUID = assetUid,
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var deleteQuery = $"DELETE FROM md_customer_CustomerAsset " +
				$"WHERE fk_CustomerUID = UNHEX('{assetCustomerEvent.CustomerUID.ToString("N")}')" +
				$" AND fk_AssetUID = UNHEX('{assetCustomerEvent.AssetUID.ToString("N")}');";
			transaction.Execute(Arg.Any<List<Action>>())
				.Returns(a =>
				{
					if (hasException)
					{
						throw new Exception();
					}
					else
					{
						a.Arg<List<Action>>().ForEach(action => action.Invoke());
						return true;
					}
				});

			//Act
			if (hasException)
				Assert.Throws<Exception>(() => customerAssetService.DissociateCustomerAsset(assetCustomerEvent));
			else
			{
				var resultData = customerAssetService.DissociateCustomerAsset(assetCustomerEvent);
				Assert.Equal(transactionStatus, resultData);
			}

			//Assert
			transaction.Received(deleteCalls).Delete(Arg.Is(deleteQuery));
			transaction.Received(publishCalls).Publish(
				Arg.Is<List<KafkaMessage>>(messages => messages
				.TrueForAll(m => ValidateCustomerAssetKafkaObject(true, m, assetCustomerEvent))));
		}
		#endregion

		#region Private Methods
		private bool ValidateCustomerAssetObject(dynamic source, DbAssetCustomer target)
		{
			Enum.TryParse(source.RelationType, true, out TestDataEnums.RelationType relationType);
			return target.Fk_CustomerUID == source.CustomerUID
				&& target.Fk_AssetUID == source.AssetUID
				&& target.fk_AssetRelationTypeID == (int)relationType;
		}
		private bool ValidateCustomerAssetKafkaObject(bool isDissociate, KafkaMessage target, dynamic source)
		{
			var isValid = target.Key == source.CustomerUID.ToString()
				&& CustomerTopics.Contains(target.Topic);
			var json = JObject.Parse(JsonConvert.SerializeObject(target.Message));
			if (isDissociate)
			{
				var eventMsg = JsonConvert.DeserializeObject<DissociateCustomerAssetEvent>(
				json.SelectToken("DissociateCustomerAssetEvent").ToString());
				return isValid
					&& eventMsg.CustomerUID == source.CustomerUID
					&& eventMsg.AssetUID == source.AssetUID
					&& eventMsg.ActionUTC == source.ActionUTC
					&& eventMsg.ReceivedUTC == source.ReceivedUTC;
			}
			else
			{
				Enum.TryParse(source.RelationType, true, out TestDataEnums.RelationType relationType);

				var eventMsg = JsonConvert.DeserializeObject<AssociateCustomerAssetEvent>(
				json.SelectToken("AssociateCustomerAssetEvent").ToString());
				return isValid
					&& eventMsg.CustomerUID == source.CustomerUID
					&& eventMsg.AssetUID == source.AssetUID
					&& eventMsg.ActionUTC == source.ActionUTC
					&& eventMsg.RelationType == relationType.ToString()
					&& eventMsg.ReceivedUTC == source.ReceivedUTC;
			}
		}
		#endregion
	}
}
