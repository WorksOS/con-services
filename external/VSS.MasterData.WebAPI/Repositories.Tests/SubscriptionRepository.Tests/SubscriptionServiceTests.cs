using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VSS.MasterData.WebAPI.CustomerRepository;
using KafkaModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.SubscriptionRepository.Models;
using VSS.MasterData.WebAPI.Transactions;
using VSS.MasterData.WebAPI.Utilities.Enums;
using Xunit;

namespace VSS.MasterData.WebAPI.SubscriptionRepository.Tests
{
	public class SubscriptionServiceTests
	{
		private readonly IConfiguration configuration;
		private readonly ILogger logger;
		private readonly ITransactions transaction;
		private ISubscriptionService subscriptionService;
		private ICustomerService customerService;
		private readonly int noOfTopics;

		private List<DbAssetSubscription> dbAssetSubscriptions = new List<DbAssetSubscription>();
		private List<DbCustomerSubscription> dbCustomerSubscriptions = new List<DbCustomerSubscription>();
		private List<DbProjectSubscription> dbProjectSubscriptions = new List<DbProjectSubscription>();

		public SubscriptionServiceTests()
		{
			configuration = Substitute.For<IConfiguration>();
			logger = Substitute.For<ILogger>();
			transaction = Substitute.For<ITransactions>();
			configuration["SubscriptionKafkaTopicNames"] = "VSS.Interfaces.Events.MasterData.ISubscriptionEvent.V1,VSS.Interfaces.Events.MasterData.ISubscriptionEvent";
			noOfTopics = configuration["SubscriptionKafkaTopicNames"].Split(",").Length;
			string[] topics = configuration["SubscriptionKafkaTopicNames"].Split(',');
			configuration["CustomerTopicNames"] = "VSS.Interfaces.Events.MasterData.ICustomerEvent,VSS.Interfaces.Events.MasterData.ICustomerEvent.V1";
			configuration["TopicSuffix"] = "-Test";
			configuration["EngineeringOperationsCustomerUid"] = Guid.NewGuid().ToString();
			customerService = new CustomerService(transaction, configuration, logger);
			subscriptionService = new SubscriptionService(customerService, configuration, logger, transaction);
			transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				foreach (var action in x.Arg<List<Action>>())
				{
					action();
				}
				return true;
			});
		}

		#region CheckSubscription
		[Fact]
		public void Test_CheckExistingSubscription_True()
		{
			//Arrange
			dbAssetSubscriptions.Add(new DbAssetSubscription());
			dbCustomerSubscriptions.Add(new DbCustomerSubscription());
			dbProjectSubscriptions.Add(new DbProjectSubscription());

			transaction.Get<DbAssetSubscription>(Arg.Any<string>()).Returns(dbAssetSubscriptions);
			transaction.Get<DbCustomerSubscription>(Arg.Any<string>()).Returns(dbCustomerSubscriptions);
			transaction.Get<DbProjectSubscription>(Arg.Any<string>()).Returns(dbProjectSubscriptions);

			//Act & Assert
			Assert.True(subscriptionService.CheckExistingSubscription(Guid.NewGuid(), "AssetSubscriptionEvent"));
			Assert.True(subscriptionService.CheckExistingSubscription(Guid.NewGuid(), "CustomerSubscriptionEvent"));
			Assert.True(subscriptionService.CheckExistingSubscription(Guid.NewGuid(), "ProjectSubscriptionEvent"));
		}

		[Fact]
		public void Test_CheckExistingSubscription_False()
		{
			//Arrange
			transaction.Get<DbAssetSubscription>(Arg.Any<string>()).Returns(dbAssetSubscriptions);
			transaction.Get<DbCustomerSubscription>(Arg.Any<string>()).Returns(dbCustomerSubscriptions);
			transaction.Get<DbProjectSubscription>(Arg.Any<string>()).Returns(dbProjectSubscriptions);

			//Act & Assert
			Assert.False(subscriptionService.CheckExistingSubscription(Guid.NewGuid(), "AssetSubscriptionEvent"));
			Assert.False(subscriptionService.CheckExistingSubscription(Guid.NewGuid(), "CustomerSubscriptionEvent"));
			Assert.False(subscriptionService.CheckExistingSubscription(Guid.NewGuid(), "ProjectSubscriptionEvent"));
		}

		[Fact]
		public void Test_CheckExistingSubscription_OnInvalidEvent_False()
		{
			//Arrange
			transaction.Get<DbAssetSubscription>(Arg.Any<string>()).Returns(dbAssetSubscriptions);

			//Act & Assert
			Assert.False(subscriptionService.CheckExistingSubscription(Guid.NewGuid(), "InvalidEvent"));
		}
		#endregion

		#region ServicePlan
		[Fact]
		public void Test_GetServicePlan_PopulatingtoCache_Success()
		{
			//Arrange
			transaction.Get<ServiceView>(Arg.Any<string>()).Returns(x =>
			{
				var list = new List<ServiceView>();
				list.Add(new ServiceView() { FamilyName = "Asset", Name = "essentials", ServiceTypeID = 1 });
				list.Add(new ServiceView() { FamilyName = "Asset", Name = "manual maintenance log", ServiceTypeID = 2 });
				list.Add(new ServiceView() { FamilyName = "Asset", Name = "cat health", ServiceTypeID = 3 });
				list.Add(new ServiceView() { FamilyName = "Customer", Name = "landfill", ServiceTypeID = 19 });
				list.Add(new ServiceView() { FamilyName = "Customer", Name = "project monitoring", ServiceTypeID = 20 });
				list.Add(new ServiceView() { FamilyName = "Project", Name = "track and trace", ServiceTypeID = 32 });
				list.Add(new ServiceView() { FamilyName = "Project", Name = "fleet", ServiceTypeID = 33 });
				return list;
			});

			//Act
			subscriptionService = new SubscriptionService(customerService,configuration, logger, transaction);

			//Assert
			Type type = subscriptionService.GetType();
			var assetSubscriptionTypeCache = type.GetField("_assetSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(subscriptionService);
			var projectSubscriptionTypeCache = type.GetField("_projectSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(subscriptionService);
			var customerSubscriptionTypeCache = type.GetField("_customerSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(subscriptionService);
			Assert.Equal(3, ((Dictionary<string, Int64>)assetSubscriptionTypeCache).Count);
			Assert.Equal(2, ((Dictionary<string, Int64>)projectSubscriptionTypeCache).Count);
			Assert.Equal(2, ((Dictionary<string, Int64>)customerSubscriptionTypeCache).Count);
		}

		[Fact]
		public void Test_GetServicePlan_ValidatingCacheValues_Success()
		{
			//Arrange
			transaction.Get<ServiceView>(Arg.Any<string>()).Returns(x =>
			{
				var list = new List<ServiceView>();
				list.Add(new ServiceView() { FamilyName = "Asset", Name = "essentials", ServiceTypeID = 1 });
				list.Add(new ServiceView() { FamilyName = "Asset", Name = "manual maintenance log", ServiceTypeID = 2 });
				list.Add(new ServiceView() { FamilyName = "Asset", Name = "cat health", ServiceTypeID = 3 });
				list.Add(new ServiceView() { FamilyName = "Customer", Name = "landfill", ServiceTypeID = 19 });
				list.Add(new ServiceView() { FamilyName = "Customer", Name = "project monitoring", ServiceTypeID = 20 });
				list.Add(new ServiceView() { FamilyName = "Project", Name = "track and trace", ServiceTypeID = 32 });
				list.Add(new ServiceView() { FamilyName = "Project", Name = "fleet", ServiceTypeID = 33 });
				return list;
			});

			//Act
			subscriptionService = new SubscriptionService(customerService,configuration, logger, transaction);

			//Assert
			Type type = subscriptionService.GetType();
			var assetSubscriptionTypeCache = type.GetField("_assetSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(subscriptionService);
			var projectSubscriptionTypeCache = type.GetField("_projectSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(subscriptionService);
			var customerSubscriptionTypeCache = type.GetField("_customerSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(subscriptionService);
			Assert.Equal(1, ((Dictionary<string, Int64>)assetSubscriptionTypeCache)["ESSENTIALS"]);
			Assert.Equal(32, ((Dictionary<string, Int64>)projectSubscriptionTypeCache)["Track And Trace"]);
			Assert.Equal(20, ((Dictionary<string, Int64>)customerSubscriptionTypeCache)["project monitoring"]);
		}

		[Fact]
		public void Test_GetServicePlan_InvalidFamilyName_Exception()
		{
			//Arrange
			transaction.Get<ServiceView>(Arg.Any<string>()).Returns(x =>
			{
				var list = new List<ServiceView>();
				list.Add(new ServiceView() { FamilyName = "InvalidFamilyName", Name = "InvalidService", ServiceTypeID = 0 });
				return list;
			});

			//Assert
			Assert.Throws<Exception>(() => new SubscriptionService(customerService,configuration, logger, transaction));
		}
		#endregion

		#region Bulk Asset Subscriptions
		[Fact]
		public void Test_BulkCreateAssetSubscriptions_Valid_Success()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var assetSubscriptionTypeCache = type.GetField("_assetSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var assetSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			assetSubscriptionList.Add("essentials", 1);
			assetSubscriptionTypeCache.SetValue(subscriptionService, assetSubscriptionList);
			List<CreateAssetSubscriptionEvent> createList = SetupCreateListRequest();

			//Act and Assert
			Assert.True(subscriptionService.CreateAssetSubscriptions(createList));
			transaction.Received(1).Upsert<DbAssetSubscription>(Arg.Is<List<DbAssetSubscription>>((x => x.Count == createList.Count)));
			transaction.Received(1).Publish(Arg.Is<List<KafkaMessage>>(x => x.Count == createList.Count * noOfTopics));
		}

		[Theory]
		[InlineData("invalid")]
		[InlineData(null)]
		public void Test_BulkCreateAssetSubscriptions_InvalidSubscriptionType_Exception(string subscriptionType)
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var assetSubscriptionTypeCache = type.GetField("_assetSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var assetSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			assetSubscriptionList.Add("essentials", 1);
			assetSubscriptionTypeCache.SetValue(subscriptionService, assetSubscriptionList);
			List<CreateAssetSubscriptionEvent> createList = SetupCreateListRequest();
			createList[0].SubscriptionType = subscriptionType;

			//Act and Assert
			Assert.Throws<Exception>(() => subscriptionService.CreateAssetSubscriptions(createList));
			transaction.Received(0).Upsert<DbAssetSubscription>(Arg.Any<List<DbAssetSubscription>>());
			transaction.Received(0).Publish(Arg.Any<List<KafkaMessage>>());
		}

		[Fact]
		public void Test_BulkUpdateAssetSubscriptions_Valid_Success()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var assetSubscriptionTypeCache = type.GetField("_assetSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var assetSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			assetSubscriptionList.Add("essentials", 1);
			assetSubscriptionTypeCache.SetValue(subscriptionService, assetSubscriptionList);
			List<UpdateAssetSubscriptionEvent> updateList = SetupUpdateListRequest();

			//Act and Assert
			Assert.True(subscriptionService.UpdateAssetSubscriptions(updateList));
			transaction.Received(1).Upsert<DbAssetSubscription>(Arg.Is<List<DbAssetSubscription>>((x => x.Count == updateList.Count)));
			transaction.Received(1).Publish(Arg.Is<List<KafkaMessage>>(x => x.Count == updateList.Count * noOfTopics));
		}

		[Theory]
		[InlineData("invalid")]
		[InlineData(null)]
		public void Test_BulkUpdateAssetSubscriptions_InvalidSubscriptionType_Exception(string subscriptionType)
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var assetSubscriptionTypeCache = type.GetField("_assetSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var assetSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			assetSubscriptionList.Add("essentials", 1);
			assetSubscriptionTypeCache.SetValue(subscriptionService, assetSubscriptionList);
			List<UpdateAssetSubscriptionEvent> updateList = SetupUpdateListRequest();
			updateList[0].SubscriptionType = subscriptionType;

			//Act and Assert
			Assert.Throws<Exception>(() => subscriptionService.UpdateAssetSubscriptions(updateList));
			transaction.Received(0).Upsert<DbAssetSubscription>(Arg.Any<List<DbAssetSubscription>>());
			transaction.Received(0).Publish(Arg.Any<List<KafkaMessage>>());
		}
		#endregion

		#region Asset Subscription
		[Fact]
		public void Test_CreateAssetSubscription_Valid_Success()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var assetSubscriptionTypeCache = type.GetField("_assetSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var assetSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			assetSubscriptionList.Add("essentials", 1);
			assetSubscriptionTypeCache.SetValue(subscriptionService, assetSubscriptionList);
			CreateAssetSubscriptionEvent create = SetupCreateListRequest()[0];

			//Act and Assert
			Assert.True(subscriptionService.CreateAssetSubscription(create));
			transaction.Received(1).Upsert(Arg.Is<DbAssetSubscription>(x => x.AssetSubscriptionUID == create.SubscriptionUID));
			transaction.Received(noOfTopics).Publish(Arg.Is<KafkaMessage>(x => x.Key == create.SubscriptionUID.ToString()));
		}

		[Theory]
		[InlineData("invalid")]
		[InlineData(null)]
		public void Test_CreateAssetSubscription_InvalidSubscriptionType_Exception(string subscriptionType)
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var assetSubscriptionTypeCache = type.GetField("_assetSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var assetSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			assetSubscriptionList.Add("essentials", 1);
			assetSubscriptionTypeCache.SetValue(subscriptionService, assetSubscriptionList);
			CreateAssetSubscriptionEvent create = SetupCreateListRequest()[0];
			create.SubscriptionType = subscriptionType;

			//Act and Assert
			Assert.Throws<Exception>(() => subscriptionService.CreateAssetSubscription(create));
			transaction.Received(0).Upsert(Arg.Any<DbAssetSubscription>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
		}

		[Fact]
		public void Test_UpdateAssetSubscription_Valid_Success()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var assetSubscriptionTypeCache = type.GetField("_assetSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var assetSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			assetSubscriptionList.Add("essentials", 1);
			assetSubscriptionTypeCache.SetValue(subscriptionService, assetSubscriptionList);
			UpdateAssetSubscriptionEvent update = SetupUpdateListRequest()[0];
			var dbAsset = new List<DbAssetSubscription>{
				new DbAssetSubscription()
				{
					AssetSubscriptionUID =update.SubscriptionUID.Value,
					fk_CustomerUID = Guid.NewGuid(),
					fk_AssetUID = Guid.NewGuid(),
					fk_DeviceUID = Guid.NewGuid(),
					fk_ServiceTypeID = 1,
					fk_SubscriptionSourceID = 2,
					StartDate = DateTime.UtcNow,
					EndDate = DateTime.UtcNow.AddMonths(3),
					InsertUTC = DateTime.UtcNow.AddMonths(-2),
					UpdateUTC = DateTime.UtcNow.AddMonths(-2)
				}
			};
			transaction.Get<DbAssetSubscription>(Arg.Any<String>()).Returns(dbAsset);

			//Act and Assert
			Assert.True(subscriptionService.UpdateAssetSubscription(update));
			transaction.Received(1).Upsert(Arg.Is<DbAssetSubscription>(x => x.AssetSubscriptionUID == update.SubscriptionUID));
			transaction.Received(noOfTopics).Publish(Arg.Is<KafkaMessage>(x => x.Key == update.SubscriptionUID.ToString()));
		}

		[Fact]
		public void Test_UpdateAssetSubscription_UpdateOnlyNewValues_Success()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var assetSubscriptionTypeCache = type.GetField("_assetSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var assetSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			assetSubscriptionList.Add("essentials", 1);
			assetSubscriptionTypeCache.SetValue(subscriptionService, assetSubscriptionList);
			UpdateAssetSubscriptionEvent update = SetupUpdateListRequest()[0];
			update.DeviceUID = null;
			update.Source = null;
			var dbAsset = new List<DbAssetSubscription>{
				new DbAssetSubscription()
				{
					AssetSubscriptionUID =update.SubscriptionUID.Value,
					fk_CustomerUID = Guid.NewGuid(),
					fk_AssetUID = Guid.NewGuid(),
					fk_DeviceUID = Guid.NewGuid(),
					fk_ServiceTypeID = 1,
					fk_SubscriptionSourceID = 2,
					StartDate = DateTime.UtcNow,
					EndDate = DateTime.UtcNow.AddMonths(3),
					InsertUTC = DateTime.UtcNow.AddMonths(-2),
					UpdateUTC = DateTime.UtcNow.AddMonths(-2)
				}
			};
			transaction.Get<DbAssetSubscription>(Arg.Any<String>()).Returns(dbAsset);

			//Act and Assert
			Assert.True(subscriptionService.UpdateAssetSubscription(update));
			transaction.Received(1).Upsert(Arg.Is<DbAssetSubscription>(x => x.AssetSubscriptionUID == update.SubscriptionUID && x.fk_SubscriptionSourceID == 2 && !x.UpdateUTC.Equals(dbAsset[0].InsertUTC) && x.fk_DeviceUID == dbAsset[0].fk_DeviceUID));
			transaction.Received(noOfTopics).Publish(Arg.Is<KafkaMessage>(x => x.Key == update.SubscriptionUID.ToString()));
		}


		[Fact]
		public void Test_UpdateAssetSubscription_SubscriptionNotExist_Exception()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var assetSubscriptionTypeCache = type.GetField("_assetSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var assetSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			assetSubscriptionList.Add("essentials", 1);
			assetSubscriptionTypeCache.SetValue(subscriptionService, assetSubscriptionList);
			UpdateAssetSubscriptionEvent update = SetupUpdateListRequest()[0];
			transaction.Get<DbAssetSubscription>(Arg.Any<String>()).Returns(new List<DbAssetSubscription> { });

			//Act and Assert
			var exception = Assert.Throws<Exception>(() => subscriptionService.UpdateAssetSubscription(update));
			Assert.Contains("Asset Subscription not exists!", exception.Message);
			transaction.Received(0).Upsert(Arg.Any<DbAssetSubscription>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
		}

		[Fact]
		public void Test_UpdateAssetSubscription_InvalidSubscriptionType_Exception()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var assetSubscriptionTypeCache = type.GetField("_assetSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var assetSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			assetSubscriptionList.Add("essentials", 1);
			assetSubscriptionTypeCache.SetValue(subscriptionService, assetSubscriptionList);
			UpdateAssetSubscriptionEvent update = SetupUpdateListRequest()[0];
			update.SubscriptionType = "invalid";
			var dbAsset = new List<DbAssetSubscription>{
				new DbAssetSubscription()
				{
					AssetSubscriptionUID = update.SubscriptionUID.Value,
					fk_CustomerUID = Guid.NewGuid(),
					fk_AssetUID = Guid.NewGuid(),
					fk_DeviceUID = Guid.NewGuid(),
					fk_ServiceTypeID = 1,
					fk_SubscriptionSourceID = 2,
					StartDate = DateTime.UtcNow,
					EndDate = DateTime.UtcNow.AddMonths(3),
					InsertUTC = DateTime.UtcNow.AddMonths(-2),
					UpdateUTC = DateTime.UtcNow.AddMonths(-2)
				}
			};
			transaction.Get<DbAssetSubscription>(Arg.Any<String>()).Returns(dbAsset);

			//Act and Assert
			var exception = Assert.Throws<Exception>(() => subscriptionService.UpdateAssetSubscription(update));
			Assert.Contains("Invalid Asset Subscription Type for the SubscriptionUID", exception.Message);
			transaction.Received(0).Upsert(Arg.Any<DbAssetSubscription>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
		}
		#endregion

		#region Customer Subscription
		[Fact]
		public void Test_CreateCustomerSubscription_Valid_Success()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var customerSubscriptionTypeCache = type.GetField("_customerSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var customerSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			customerSubscriptionList.Add("manual 3d monitoring", 15);
			customerSubscriptionTypeCache.SetValue(subscriptionService, customerSubscriptionList);
			CreateCustomerSubscriptionEvent create = new CreateCustomerSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Manual 3D Monitoring",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			//Act and Assert
			Assert.True(subscriptionService.CreateCustomerSubscription(create));
			transaction.Received(1).Upsert(Arg.Is<DbCustomerSubscription>(x => x.CustomerSubscriptionUID == create.SubscriptionUID));
			transaction.Received(noOfTopics).Publish(Arg.Is<KafkaMessage>(x => x.Key == create.SubscriptionUID.ToString()));
		}

		[Theory]
		[InlineData("invalid")]
		[InlineData(null)]
		public void Test_CreateCustomerSubscription_InvalidSubscriptionType_Exception(string subscriptionType)
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var customerSubscriptionTypeCache = type.GetField("_customerSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var customerSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			customerSubscriptionList.Add("manual 3d monitoring", 15);
			customerSubscriptionTypeCache.SetValue(subscriptionService, customerSubscriptionList);
			CreateCustomerSubscriptionEvent create = new CreateCustomerSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = subscriptionType,
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			//Act and Assert
			Assert.Throws<Exception>(() => subscriptionService.CreateCustomerSubscription(create));
			transaction.Received(0).Upsert(Arg.Any<DbCustomerSubscription>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
		}

		[Fact]
		public void Test_UpdateCustomerSubscription_Valid_Success()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var customerSubscriptionTypeCache = type.GetField("_customerSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var customerSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			customerSubscriptionList.Add("manual 3d monitoring", 15);
			customerSubscriptionTypeCache.SetValue(subscriptionService, customerSubscriptionList);
			UpdateCustomerSubscriptionEvent update = new UpdateCustomerSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var dbCustomerList = new List<DbCustomerSubscription>
			{
				new DbCustomerSubscription
				{
					CustomerSubscriptionUID = update.SubscriptionUID.Value,
					StartDate = DateTime.UtcNow,
					EndDate = DateTime.UtcNow.AddMonths(4)
				}
			};
			transaction.Get<DbCustomerSubscription>(Arg.Any<String>()).Returns(dbCustomerList);

			//Act and Assert
			Assert.True(subscriptionService.UpdateCustomerSubscription(update));
			transaction.Received(1).Upsert(Arg.Is<DbCustomerSubscription>(x => x.CustomerSubscriptionUID == update.SubscriptionUID && x.EndDate == update.EndDate));
			transaction.Received(noOfTopics).Publish(Arg.Is<KafkaMessage>(x => x.Key == update.SubscriptionUID.ToString()));
		}

		[Fact]
		public void Test_UpdateCustomerSubscription_UpdateOnlyNewValues_Success()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var customerSubscriptionTypeCache = type.GetField("_customerSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var customerSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			customerSubscriptionList.Add("manual 3d monitoring", 15);
			customerSubscriptionTypeCache.SetValue(subscriptionService, customerSubscriptionList);
			UpdateCustomerSubscriptionEvent update = new UpdateCustomerSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var dbCustomerList = new List<DbCustomerSubscription>
			{
				new DbCustomerSubscription
				{
					CustomerSubscriptionUID = update.SubscriptionUID.Value,
					StartDate = DateTime.UtcNow,
				}
			};
			transaction.Get<DbCustomerSubscription>(Arg.Any<String>()).Returns(dbCustomerList);

			//Act and Assert
			Assert.True(subscriptionService.UpdateCustomerSubscription(update));
			transaction.Received(1).Upsert(Arg.Is<DbCustomerSubscription>(x => x.CustomerSubscriptionUID == update.SubscriptionUID && !x.UpdateUTC.Equals(dbCustomerList[0].InsertUTC) && x.StartDate == dbCustomerList[0].StartDate && x.EndDate == update.EndDate));
			transaction.Received(noOfTopics).Publish(Arg.Is<KafkaMessage>(x => x.Key == update.SubscriptionUID.ToString()));
		}

		[Fact]
		public void Test_UpdateCustomerSubscription_SubscriptionNotExist_Exception()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var customerSubscriptionTypeCache = type.GetField("_customerSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var customerSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			customerSubscriptionList.Add("manual 3d monitoring", 15);
			customerSubscriptionTypeCache.SetValue(subscriptionService, customerSubscriptionList);
			UpdateCustomerSubscriptionEvent update = new UpdateCustomerSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			transaction.Get<DbCustomerSubscription>(Arg.Any<String>()).Returns(new List<DbCustomerSubscription> { });

			//Act and Assert
			var exception = Assert.Throws<Exception>(() => subscriptionService.UpdateCustomerSubscription(update));
			Assert.Contains("Customer Subscription not exists!", exception.Message);
			transaction.Received(0).Upsert(Arg.Any<DbCustomerSubscription>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
		}

		[Fact]
		public void Test_UpdateCustomerSubscription_InvalidRequest_Exception()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var customerSubscriptionTypeCache = type.GetField("_customerSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var customerSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			customerSubscriptionList.Add("manual 3d monitoring", 15);
			customerSubscriptionTypeCache.SetValue(subscriptionService, customerSubscriptionList);
			UpdateCustomerSubscriptionEvent update = new UpdateCustomerSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var dbCustomerList = new List<DbCustomerSubscription>
			{
				new DbCustomerSubscription
				{
					CustomerSubscriptionUID =update.SubscriptionUID.Value,
					StartDate = DateTime.UtcNow,
					EndDate = DateTime.UtcNow.AddMonths(1)
				}
			};
			transaction.Get<DbCustomerSubscription>(Arg.Any<String>()).Returns(dbCustomerList);

			//Act and Assert
			var exception = Assert.Throws<Exception>(() => subscriptionService.UpdateCustomerSubscription(update));
			Assert.Contains("Update Customer Subscription Request should have atleast one field to update", exception.Message);
			transaction.Received(0).Upsert(Arg.Any<DbCustomerSubscription>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
		}

		#endregion

		#region Project Subscription
		[Fact]
		public void Test_CreateProjectSubscription_Valid_Success()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var ProjectSubscriptionTypeCache = type.GetField("_projectSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var ProjectSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			ProjectSubscriptionList.Add("landfill", 19);
			ProjectSubscriptionTypeCache.SetValue(subscriptionService, ProjectSubscriptionList);
			CreateProjectSubscriptionEvent create = new CreateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Landfill",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			//Act and Assert
			Assert.True(subscriptionService.CreateProjectSubscription(create));
			transaction.Received(1).Upsert(Arg.Is<DbProjectSubscription>(x => x.ProjectSubscriptionUID == create.SubscriptionUID));
			transaction.Received(noOfTopics).Publish(Arg.Is<KafkaMessage>(x => x.Key == create.SubscriptionUID.ToString()));
		}

		[Theory]
		[InlineData("invalid")]
		[InlineData(null)]
		public void Test_CreateProjectSubscription_InvalidSubscriptionType_Exception(string subscriptionType)
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var ProjectSubscriptionTypeCache = type.GetField("_projectSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var ProjectSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			ProjectSubscriptionList.Add("landfill", 19);
			ProjectSubscriptionTypeCache.SetValue(subscriptionService, ProjectSubscriptionList);
			CreateProjectSubscriptionEvent create = new CreateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = subscriptionType,
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			//Act and Assert
			Assert.Throws<Exception>(() => subscriptionService.CreateProjectSubscription(create));
			transaction.Received(0).Upsert(Arg.Any<DbProjectSubscription>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
		}

		[Fact]
		public void Test_UpdateProjectSubscription_Valid_Success()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var ProjectSubscriptionTypeCache = type.GetField("_projectSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var ProjectSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			ProjectSubscriptionList.Add("landfill", 19);
			ProjectSubscriptionTypeCache.SetValue(subscriptionService, ProjectSubscriptionList);
			UpdateProjectSubscriptionEvent update = new UpdateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Landfill",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var dbProjectList = new List<DbProjectSubscription>
			{
				new DbProjectSubscription()
				{
					ProjectSubscriptionUID = update.SubscriptionUID.Value,
					fk_CustomerUID = Guid.NewGuid(),
					fk_ServiceTypeID = 19,
					StartDate = DateTime.UtcNow,
					EndDate = DateTime.UtcNow.AddMonths(4),
					InsertUTC = DateTime.UtcNow.AddMonths(-2),
					UpdateUTC = DateTime.UtcNow.AddMonths(-2)
				}
			};
			transaction.Get<DbProjectSubscription>(Arg.Any<String>()).Returns(dbProjectList);

			//Act and Assert
			Assert.True(subscriptionService.UpdateProjectSubscription(update));
			transaction.Received(1).Upsert(Arg.Is<DbProjectSubscription>(x => x.ProjectSubscriptionUID == update.SubscriptionUID));
			transaction.Received(noOfTopics).Publish(Arg.Is<KafkaMessage>(x => x.Key == update.SubscriptionUID.ToString()));
		}

		[Fact]
		public void Test_UpdateProjectSubscription_UpdateOnlyNewValues_Success()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var ProjectSubscriptionTypeCache = type.GetField("_projectSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var ProjectSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			ProjectSubscriptionList.Add("landfill", 19);
			ProjectSubscriptionTypeCache.SetValue(subscriptionService, ProjectSubscriptionList);
			UpdateProjectSubscriptionEvent update = new UpdateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Landfill",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var dbProjectList = new List<DbProjectSubscription>
			{
				new DbProjectSubscription()
				{
					ProjectSubscriptionUID = update.SubscriptionUID.Value,
					fk_CustomerUID = Guid.NewGuid(),
					fk_ServiceTypeID = 19,
					StartDate = DateTime.UtcNow,
					EndDate = DateTime.UtcNow.AddMonths(4),
					InsertUTC = DateTime.UtcNow.AddMonths(-2),
					UpdateUTC = DateTime.UtcNow.AddMonths(-2)
				}
			};
			transaction.Get<DbProjectSubscription>(Arg.Any<String>()).Returns(dbProjectList);

			//Act and Assert
			Assert.True(subscriptionService.UpdateProjectSubscription(update));
			transaction.Received(1).Upsert(Arg.Is<DbProjectSubscription>(x => x.ProjectSubscriptionUID == update.SubscriptionUID && !x.UpdateUTC.Equals(dbProjectList[0].InsertUTC) && x.EndDate == update.EndDate));
			transaction.Received(noOfTopics).Publish(Arg.Is<KafkaMessage>(x => x.Key == update.SubscriptionUID.ToString()));
		}

		[Fact]
		public void Test_UpdateProjectSubscription_ValidWithoutSubscriptionType_Success()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var ProjectSubscriptionTypeCache = type.GetField("_projectSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var ProjectSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			ProjectSubscriptionList.Add("landfill", 19);
			ProjectSubscriptionTypeCache.SetValue(subscriptionService, ProjectSubscriptionList);
			UpdateProjectSubscriptionEvent update = new UpdateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var dbProjectList = new List<DbProjectSubscription>
			{
				new DbProjectSubscription()
				{
					ProjectSubscriptionUID = update.SubscriptionUID.Value,
					fk_CustomerUID = Guid.NewGuid(),
					fk_ServiceTypeID = 19,
					StartDate = DateTime.UtcNow,
					EndDate = DateTime.UtcNow.AddMonths(4),
					InsertUTC = DateTime.UtcNow.AddMonths(-2),
					UpdateUTC = DateTime.UtcNow.AddMonths(-2)
				}
			};
			transaction.Get<DbProjectSubscription>(Arg.Any<String>()).Returns(dbProjectList);

			//Act and Assert
			Assert.True(subscriptionService.UpdateProjectSubscription(update));
			transaction.Received(1).Upsert(Arg.Is<DbProjectSubscription>(x => x.ProjectSubscriptionUID == update.SubscriptionUID && x.fk_ServiceTypeID == 19 && x.EndDate == update.EndDate));
			transaction.Received(noOfTopics).Publish(Arg.Is<KafkaMessage>(x => x.Key == update.SubscriptionUID.ToString()));
		}

		[Fact]
		public void Test_UpdateProjectSubscription_SubscriptionNotExist_Exception()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var ProjectSubscriptionTypeCache = type.GetField("_projectSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var ProjectSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			ProjectSubscriptionList.Add("landfill", 19);
			ProjectSubscriptionTypeCache.SetValue(subscriptionService, ProjectSubscriptionList);
			UpdateProjectSubscriptionEvent update = new UpdateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "landfill",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			transaction.Get<DbProjectSubscription>(Arg.Any<String>()).Returns(new List<DbProjectSubscription>() { });

			//Act and Assert
			var exception = Assert.Throws<Exception>(() => subscriptionService.UpdateProjectSubscription(update));
			Assert.Contains("Project Subscription not exists!", exception.Message);
			transaction.Received(0).Upsert(Arg.Any<DbProjectSubscription>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
		}

		[Fact]
		public void Test_UpdateProjectSubscription_InvalidRequest_Exception()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var ProjectSubscriptionTypeCache = type.GetField("_projectSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var ProjectSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			ProjectSubscriptionList.Add("landfill", 19);
			ProjectSubscriptionTypeCache.SetValue(subscriptionService, ProjectSubscriptionList);
			UpdateProjectSubscriptionEvent update = new UpdateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var dbProjectList = new List<DbProjectSubscription>
			{
				new DbProjectSubscription()
				{
					ProjectSubscriptionUID = update.SubscriptionUID.Value,
					fk_CustomerUID = Guid.NewGuid(),
					fk_ServiceTypeID = 19,
					StartDate = DateTime.UtcNow,
					EndDate = DateTime.UtcNow.AddMonths(1),
					InsertUTC = DateTime.UtcNow.AddMonths(-2),
					UpdateUTC = DateTime.UtcNow.AddMonths(-2)
				}
			};
			transaction.Get<DbProjectSubscription>(Arg.Any<String>()).Returns(dbProjectList);

			//Act and Assert
			var exception = Assert.Throws<Exception>(() => subscriptionService.UpdateProjectSubscription(update));
			Assert.Contains("Update Project Subscription Request should have atleast one field to update", exception.Message);
			transaction.Received(0).Upsert(Arg.Any<DbProjectSubscription>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
		}

		[Fact]
		public void Test_UpdateProjectSubscription_InvalidSubscriptionType_Exception()
		{
			//Arrange
			Type type = subscriptionService.GetType();
			var ProjectSubscriptionTypeCache = type.GetField("_projectSubscriptionTypeCache", BindingFlags.NonPublic | BindingFlags.Instance);
			var ProjectSubscriptionList = new Dictionary<string, Int64>(StringComparer.InvariantCultureIgnoreCase);
			ProjectSubscriptionList.Add("landfill", 19);
			ProjectSubscriptionTypeCache.SetValue(subscriptionService, ProjectSubscriptionList);
			UpdateProjectSubscriptionEvent update = new UpdateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "invalid",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var dbProjectList = new List<DbProjectSubscription>
			{
				new DbProjectSubscription()
				{
					ProjectSubscriptionUID = update.SubscriptionUID.Value,
					fk_CustomerUID = Guid.NewGuid(),
					fk_ServiceTypeID = 19,
					StartDate = DateTime.UtcNow,
					EndDate = DateTime.UtcNow.AddMonths(4),
					InsertUTC = DateTime.UtcNow.AddMonths(-2),
					UpdateUTC = DateTime.UtcNow.AddMonths(-2)
				}
			};
			transaction.Get<DbProjectSubscription>(Arg.Any<String>()).Returns(dbProjectList);

			//Act and Assert
			var exception = Assert.Throws<Exception>(() => subscriptionService.UpdateProjectSubscription(update));
			Assert.Contains("Invalid Project Subscription Type", exception.Message);
			transaction.Received(0).Upsert(Arg.Any<DbProjectSubscription>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
		}

		[Fact]
		public void Test_AssociateProjectSubscription_Valid_Success()
		{
			//Arrange
			AssociateProjectSubscriptionEvent associateEvent = new AssociateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				ProjectUID = Guid.NewGuid(),
				EffectiveDate = DateTime.UtcNow,
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			var input = new DbProjectSubscription
			{
				ProjectSubscriptionUID = associateEvent.SubscriptionUID,
				fk_ProjectUID = Guid.NewGuid(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(3),
				InsertUTC = DateTime.UtcNow.AddMonths(-2),
				UpdateUTC = DateTime.UtcNow.AddMonths(-2)
			};

			transaction.Get<DbProjectSubscription>(Arg.Any<string>()).Returns(new List<DbProjectSubscription>() { input });

			//Act and Assert
			Assert.True(subscriptionService.AssociateProjectSubscription(associateEvent));
			transaction.Received(1).Upsert(Arg.Is<DbProjectSubscription>(x => x.ProjectSubscriptionUID == associateEvent.SubscriptionUID && x.fk_ProjectUID == associateEvent.ProjectUID));
			transaction.Received(noOfTopics).Publish(Arg.Is<KafkaMessage>(x => x.Key == input.ProjectSubscriptionUID.ToString()));
		}

		[Fact]
		public void Test_AssociateProjectSubscription_InvalidProjectSubscriptionUID_Exception()
		{
			//Arrange
			AssociateProjectSubscriptionEvent associateEvent = new AssociateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				ProjectUID = Guid.NewGuid(),
				EffectiveDate = DateTime.UtcNow,
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			transaction.Get<DbProjectSubscription>(Arg.Any<string>()).Returns(new List<DbProjectSubscription>() { });

			//Act and Assert
			Assert.Throws<Exception>(() => subscriptionService.AssociateProjectSubscription(associateEvent));
			transaction.Received(0).Upsert(Arg.Any<DbProjectSubscription>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
		}

		[Fact]
		public void Test_DissociateProjectSubscription_Valid_Success()
		{
			//Arrange
			DissociateProjectSubscriptionEvent dissociateEvent = new DissociateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				ProjectUID = Guid.NewGuid(),
				EffectiveDate = DateTime.UtcNow,
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			var input = new DbProjectSubscription
			{
				ProjectSubscriptionUID = dissociateEvent.SubscriptionUID,
				fk_ProjectUID = null,
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(3),
				InsertUTC = DateTime.UtcNow.AddMonths(-2),
				UpdateUTC = DateTime.UtcNow.AddMonths(-2)
			};

			transaction.Get<DbProjectSubscription>(Arg.Any<string>()).Returns(new List<DbProjectSubscription>() { input });

			//Act and Assert
			Assert.True(subscriptionService.DissociateProjectSubscription(dissociateEvent));
			transaction.Received(1).Upsert(Arg.Is<DbProjectSubscription>(x => x.ProjectSubscriptionUID == dissociateEvent.SubscriptionUID && x.fk_ProjectUID == null));
			transaction.Received(noOfTopics).Publish(Arg.Is<KafkaMessage>(x => x.Key == input.ProjectSubscriptionUID.ToString()));
		}

		[Fact]
		public void Test_DissociateProjectSubscription_InvalidProjectSubscriptionUID_Exception()
		{
			//Arrange
			DissociateProjectSubscriptionEvent dissociateEvent = new DissociateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				ProjectUID = Guid.NewGuid(),
				EffectiveDate = DateTime.UtcNow,
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			transaction.Get<DbProjectSubscription>(Arg.Any<string>()).Returns(new List<DbProjectSubscription>() { });

			//Act and Assert
			Assert.Throws<Exception>(() => subscriptionService.DissociateProjectSubscription(dissociateEvent));
			transaction.Received(0).Upsert(Arg.Any<DbProjectSubscription>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
		}
		#endregion

		#region Get Subscription

		[Fact]
		void GetSubscriptionForCustomer_Valid_Success()
		{
			//Arrange
			List<CustomerSubscriptionModel> mockedList = new List<CustomerSubscriptionModel>();
			DateTime startDate = DateTime.UtcNow;
			DateTime endDate = DateTime.MaxValue;
			mockedList.Add(new CustomerSubscriptionModel()
			{
				SubscriptionType = "Essentials",
				StartDate = startDate,
				EndDate = endDate
			});
			mockedList.Add(new CustomerSubscriptionModel()
			{
				SubscriptionType = "3D Project Monitoring",
				StartDate = startDate,
				EndDate = endDate
			});

			transaction.Get<CustomerSubscriptionModel>(Arg.Any<string>()).Returns(mockedList);

			//Act
			List<CustomerSubscriptionModel> resultList = (List<CustomerSubscriptionModel>)subscriptionService.GetSubscriptionForCustomer(Guid.NewGuid());

			//Assert
			Assert.Equal(mockedList.Count, resultList.Count);
			for (int index = 0; index < mockedList.Count; index++)
			{
				Assert.Equal(mockedList[index].SubscriptionType, resultList[index].SubscriptionType);
				Assert.Equal(mockedList[index].StartDate, resultList[index].StartDate);
				Assert.Equal(mockedList[index].EndDate, resultList[index].EndDate);
			}
		}

		[Fact]
		void GetSubscriptionForCustomer_OrderByEndDateAndSubscriptionType_Success()
		{
			//Arange
			List<CustomerSubscriptionModel> mockedList = new List<CustomerSubscriptionModel>();
			DateTime startDate = DateTime.UtcNow;
			DateTime endDate = DateTime.MaxValue;
			DateTime expired = new DateTime(2019, 07, 28);
			mockedList.Add(new CustomerSubscriptionModel()
			{
				SubscriptionType = "Essentials",
				StartDate = startDate,
				EndDate = endDate
			});
			mockedList.Add(new CustomerSubscriptionModel()
			{
				SubscriptionType = "3D Project Monitoring",
				StartDate = startDate,
				EndDate = endDate
			});
			mockedList.Add(new CustomerSubscriptionModel()
			{
				SubscriptionType = "Essentials",
				StartDate = startDate,
				EndDate = new DateTime(2019, 07, 28)
			});

			transaction.Get<CustomerSubscriptionModel>(Arg.Any<string>()).Returns(mockedList);

			//Act
			List<CustomerSubscriptionModel> resultList = (List<CustomerSubscriptionModel>)(subscriptionService.GetSubscriptionForCustomer(Guid.NewGuid()));

			//Assert
			Assert.Equal(2, resultList.Count);
			Assert.Equal(mockedList[0].SubscriptionType, resultList[0].SubscriptionType);
			Assert.Equal(mockedList[0].StartDate, resultList[0].StartDate);
			Assert.Equal(mockedList[0].EndDate, resultList[0].EndDate);
			Assert.Equal(mockedList[1].SubscriptionType, resultList[1].SubscriptionType);
			Assert.Equal(mockedList[1].StartDate, resultList[1].StartDate);
			Assert.Equal(mockedList[1].EndDate, resultList[1].EndDate);
		}

		[Fact]
		void GetSubscriptionForCustomer_NoSubscriptionExist_Success()
		{
			//Arrange
			transaction.Get<CustomerSubscriptionModel>(Arg.Any<string>()).Returns(new List<CustomerSubscriptionModel>());

			//Act
			List<CustomerSubscriptionModel> resultList = (List<CustomerSubscriptionModel>)subscriptionService.GetSubscriptionForCustomer(Guid.NewGuid());

			//Assert
			Assert.Empty(resultList);
		}

		[Fact]
		void GetSubscriptionForActiveProjectCustomer_Valid_Success()
		{
			//Arrange
			List<ActiveProjectCustomerSubscriptionModel> mockedList = new List<ActiveProjectCustomerSubscriptionModel>();
			DateTime startDate = DateTime.UtcNow;
			DateTime endDate = DateTime.MaxValue;
			mockedList.Add(new ActiveProjectCustomerSubscriptionModel()
			{
				SubscriptionGuid = Guid.NewGuid(),
				SubscriptionType = "Landfill",
				StartDate = startDate,
				EndDate = endDate
			});
			mockedList.Add(new ActiveProjectCustomerSubscriptionModel()
			{
				SubscriptionGuid = Guid.NewGuid(),
				SubscriptionType = "Project Monitoring",
				StartDate = startDate,
				EndDate = endDate
			});

			transaction.Get<ActiveProjectCustomerSubscriptionModel>(Arg.Any<string>()).Returns(mockedList);

			//Act
			List<ActiveProjectCustomerSubscriptionModel> resultList = (List<ActiveProjectCustomerSubscriptionModel>)subscriptionService.GetActiveProjectSubscriptionForCustomer(Guid.NewGuid());

			//Assert
			Assert.Equal(mockedList.Count, resultList.Count);
			for (int index = 0; index < mockedList.Count; index++)
			{
				Assert.Equal(mockedList[index].SubscriptionGuid, resultList[index].SubscriptionGuid);
				Assert.Equal(mockedList[index].SubscriptionType, resultList[index].SubscriptionType);
				Assert.Equal(mockedList[index].StartDate, resultList[index].StartDate);
				Assert.Equal(mockedList[index].EndDate, resultList[index].EndDate);
			}
		}

		[Fact]
		void GetSubscriptionForActiveProjectCustomer_NoSubscriptionExist_Success()
		{
			//Arrange
			transaction.Get<ActiveProjectCustomerSubscriptionModel>(Arg.Any<string>()).Returns(new List<ActiveProjectCustomerSubscriptionModel>());

			//Act
			List<ActiveProjectCustomerSubscriptionModel> resultList = (List<ActiveProjectCustomerSubscriptionModel>)subscriptionService.GetActiveProjectSubscriptionForCustomer(Guid.NewGuid());

			//Assert
			Assert.Empty(resultList);
		}

		[Fact]
		void GetSubscriptionForAsset_Valid_Success()
		{
			//Arrange
			Guid uid = Guid.NewGuid();
			List<OwnerVisibility> customerVisibilityList = new List<OwnerVisibility>();
			customerVisibilityList.Add(new OwnerVisibility()
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionName = "Essentials",
				SubscriptionStatus = "Active",
				SubscriptionStartDate = DateTime.UtcNow,
				SubscriptionEndDate = DateTime.MaxValue
			});
			customerVisibilityList.Add(new OwnerVisibility()
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionName = "CAT Health",
				SubscriptionStatus = "Active",
				SubscriptionStartDate = DateTime.UtcNow,
				SubscriptionEndDate = DateTime.MaxValue
			});
			customerVisibilityList.Add(new OwnerVisibility()
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionName = "CATMAINT",
				SubscriptionStatus = "Active",
				SubscriptionStartDate = DateTime.UtcNow,
				SubscriptionEndDate = DateTime.MaxValue
			});

			transaction.Get<OwnerVisibility>(Arg.Any<string>()).Returns(customerVisibilityList);

			//Act
			AssetSubscriptionModel asset = subscriptionService.GetSubscriptionForAsset(uid);

			//Assert
			Assert.Equal(uid, asset.AssetUID);
			Assert.Equal("Active", asset.SubscriptionStatus);
			Assert.Equal(customerVisibilityList, asset.OwnersVisibility);
		}

		[Theory]
		[InlineData("Active", "Customer", CustomerType.Customer)]
		[InlineData("InActive", "Dealer", CustomerType.Dealer)]
		void GetSubscriptionForAsset_ValidActiveSubscriptionCheck_Success(string status, string customerName, CustomerType customerType)
		{
			//Arrange
			Guid assetUid = Guid.NewGuid();
			List<DbCustomer> customers = new List<DbCustomer>
			{
				new DbCustomer()
				{
					CustomerName = customerName,
					fk_CustomerTypeID = (int) customerType,
					CustomerUID = Guid.NewGuid()
				}
			};
			List<OwnerVisibility> customerVisibilityList = new List<OwnerVisibility>();
			customerVisibilityList.Add(new OwnerVisibility()
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionName = "Essentials",
				SubscriptionStatus = status,
				SubscriptionStartDate = DateTime.MinValue,
				SubscriptionEndDate = DateTime.UtcNow
			});
			customerVisibilityList.Add(new OwnerVisibility()
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = customers[0].CustomerUID,
				SubscriptionName = "CAT Health",
				SubscriptionStatus = "InActive",
				SubscriptionStartDate = DateTime.MinValue,
				SubscriptionEndDate = DateTime.UtcNow
			});

			transaction.Get<OwnerVisibility>(Arg.Any<string>()).Returns(customerVisibilityList);
			transaction.Get<DbCustomer>(Arg.Any<string>()).Returns(customers);

			//Act
			AssetSubscriptionModel asset = subscriptionService.GetSubscriptionForAsset(assetUid);

			//Assert
			Assert.Equal(assetUid, asset.AssetUID);
			Assert.Equal(status, asset.SubscriptionStatus);
			var subscriptionOne = customerVisibilityList.Where(x => x.SubscriptionUID == asset.OwnersVisibility[0].SubscriptionUID).ToList().First();
			var subscriptionTwo = customerVisibilityList.Where(x => x.SubscriptionUID == asset.OwnersVisibility[1].SubscriptionUID).ToList().First();
			Assert.Null(subscriptionOne.CustomerName);
			Assert.Null(subscriptionOne.CustomerType);
			Assert.Equal(customerName, subscriptionTwo.CustomerName);
			Assert.Equal(customerName, subscriptionTwo.CustomerType);
		}

		[Fact]
		void GetSubscriptionForAsset_NoSubscriptionExist_Success()
		{
			//Arrange
			Guid assetUid = Guid.NewGuid();

			transaction.Get<OwnerVisibility>(Arg.Any<string>()).Returns(new List<OwnerVisibility>());

			//Act
			AssetSubscriptionModel asset = subscriptionService.GetSubscriptionForAsset(assetUid);

			//Assert
			Assert.Null(asset.AssetUID);
			Assert.Null(asset.SubscriptionStatus);
			Assert.Null(asset.OwnersVisibility);
		}
		#endregion

		private static List<CreateAssetSubscriptionEvent> SetupCreateListRequest()
		{
			List<CreateAssetSubscriptionEvent> obj = new List<CreateAssetSubscriptionEvent>{
			new CreateAssetSubscriptionEvent() { SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.SAV.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow } ,
			new CreateAssetSubscriptionEvent() { SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.SAV.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow },
			new CreateAssetSubscriptionEvent() { SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.SAV.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow }};
			return obj;
		}

		private static List<UpdateAssetSubscriptionEvent> SetupUpdateListRequest()
		{
			List<UpdateAssetSubscriptionEvent> obj = new List<UpdateAssetSubscriptionEvent>{
			new UpdateAssetSubscriptionEvent() { SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.SAV.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow } ,
			new UpdateAssetSubscriptionEvent() { SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.SAV.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow },
			new UpdateAssetSubscriptionEvent() { SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.SAV.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow }};
			return obj;
		}
	}
}