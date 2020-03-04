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
using System.Reflection;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.Customer.KafkaModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Transactions;
using Xunit;

namespace VSS.MasterData.WebAPI.CustomerRepository.Tests
{
	public class AccountServiceTests
	{
		private readonly ILogger logger;
		private readonly IConfiguration configuration;
		private readonly ITransactions transaction;
		private readonly IAccountService accountService;
		private readonly string AccountTopic;
		public AccountServiceTests()
		{
			logger = Substitute.For<ILogger>();
			transaction = Substitute.For<ITransactions>();
			string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			configuration = new ConfigurationBuilder().SetBasePath(currentDirectory)
													.AddJsonFile("appsettings.json", true)
													.AddEnvironmentVariables()
													.Build();
			accountService = new AccountService(transaction, configuration, logger);
			AccountTopic = configuration["AccountTopicName"] + configuration["TopicSuffix"];
		}

		#region GetAccount
		[Fact]
		public void GetAccount_ValidAccount_ReturnsResult()
		{
			//Arrange
			var accountUid = Guid.NewGuid();
			var account = new DbAccount
			{
				CustomerAccountID = 1,
				CustomerAccountUID = accountUid,
				BSSID = "BSS01",
				AccountName = "ACC01",
				NetworkCustomerCode = "NCC01",
				DealerAccountCode = "DAC01",
				fk_ParentCustomerUID = Guid.NewGuid(),
				fk_ChildCustomerUID = Guid.NewGuid(),
				RowUpdatedUTC = DateTime.UtcNow
			};
			var config = new ComparisonConfig
			{
				IgnoreObjectTypes = true,
				MaxMillisecondsDateDifference = 500,
				MaxDifferences = 0
			};
			var accountCompareLogic = new CompareLogic(config);
			transaction.Get<DbAccount>(Arg.Any<string>()).Returns(new List<DbAccount> { account });

			//Act
			var resultData = accountService.GetAccount(accountUid);

			//Arrange
			Assert.NotNull(resultData);
			ComparisonResult compareResult = accountCompareLogic.Compare(account, resultData);
			Assert.True(compareResult.Differences.Count == 0);
		}

		[Fact]
		public void GetAccount_InValidAccount_EmptyResult()
		{
			//Arrange
			var accountUid = Guid.NewGuid();
			transaction.Get<DbAccount>(Arg.Any<string>()).Returns(new List<DbAccount>());

			//Act
			var result = accountService.GetAccount(accountUid);

			//Arrange
			Assert.Null(result);
		}
		#endregion

		#region CreateAccount
		[Fact]
		public void CreateAccount_ValidPayload_TransactionSuccess()
		{
			//Arrange
			var accountEvent = new CreateCustomerEvent
			{
				CustomerName = "ACC01",
				CustomerUID = Guid.NewGuid(),
				BSSID = "BSS01",
				DealerAccountCode = "DAC01",
				NetworkCustomerCode = "NCC01",
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			transaction.Execute(Arg.Any<List<Action>>())
				.Returns(a =>
				{
					a.Arg<List<Action>>().ForEach(action => action.Invoke());
					return true;
				});

			//Act
			var resultData = accountService.CreateAccount(accountEvent);

			//Assert
			Assert.True(resultData);
			transaction.Received(1).Upsert(Arg.Is<DbAccount>(o => ValidateAccountObject(accountEvent, o, false, null)));
			transaction.Received(1).Publish(
				Arg.Is<KafkaMessage>(m => ValidateAccountKafkaObject(accountEvent, m, false, null)));
		}

		[Fact]
		public void CreateAccount_PublishUpsertException_TransactionException()
		{
			//Arrange
			var accountEvent = new CreateCustomerEvent
			{
				CustomerName = "ACC01",
				CustomerUID = Guid.NewGuid(),
				BSSID = "BSS01",
				DealerAccountCode = "DAC01",
				NetworkCustomerCode = "NCC01",
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			transaction.Execute(Arg.Any<List<Action>>())
				.Returns(a =>
				{
					throw new Exception();
				});

			//Act
			Assert.Throws<Exception>(() => accountService.CreateAccount(accountEvent));


			transaction.Received(0).Upsert(Arg.Any<DbAccount>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
		}
		#endregion

		#region UpdateAccount
		[Theory]
		[InlineData(false, false)]
		[InlineData(true, true)]
		public void UpdateAccount_ValidPayload_TransactionSuccess(bool isParentNull, bool isChildNull)
		{
			//Arrange
			var accountUid = Guid.NewGuid();
			var accountDetail = new DbAccount
			{
				CustomerAccountID = 1,
				fk_ChildCustomerUID = isChildNull ? (Guid?)null : Guid.NewGuid(),
				fk_ParentCustomerUID = isParentNull ? (Guid?)null : Guid.NewGuid(),
				CustomerAccountUID = accountUid
			};
			var updateEvent = new UpdateCustomerEvent
			{
				CustomerUID = accountUid,
				BSSID = "BSS02",
				CustomerName = "CUS02",
				NetworkCustomerCode = "NCC02",
				DealerAccountCode = "DAC02",
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			transaction.Execute(Arg.Any<List<Action>>()).Returns(a =>
			{
				a.Arg<List<Action>>().ForEach(action => action.Invoke());
				return true;
			});

			//Act
			var resultData = accountService.UpdateAccount(updateEvent, accountDetail);

			//Assert
			Assert.True(resultData);
			transaction.Received(1).Upsert(
				Arg.Is<DbAccount>(account => ValidateAccountObject(updateEvent, account, true, accountDetail)));
			transaction.Received(1).Publish(
				Arg.Is<KafkaMessage>(m => ValidateAccountKafkaObject(updateEvent, m, true, accountDetail)));
		}

		[Fact]
		public void UpdateAccount_PublishUpsertException_TransactionException()
		{
			//Arrange
			var accountUid = Guid.NewGuid();
			var accountDetail = new DbAccount
			{
				CustomerAccountID = 1,
				fk_ChildCustomerUID = Guid.NewGuid(),
				fk_ParentCustomerUID = Guid.NewGuid(),
				CustomerAccountUID = accountUid
			};
			var updateEvent = new UpdateCustomerEvent
			{
				CustomerUID = accountUid,
				BSSID = "BSS02",
				CustomerName = "CUS02",
				NetworkCustomerCode = "NCC02",
				DealerAccountCode = "DAC02",
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			transaction.Execute(Arg.Any<List<Action>>()).Returns(a =>
			{
				throw new Exception();
			});

			//Act
			Assert.Throws<Exception>(() => accountService.UpdateAccount(updateEvent, accountDetail));

			//Assert
			transaction.Received(0).Upsert(Arg.Any<DbAccount>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
		}
		#endregion

		#region DeleteAccount
		[Fact]
		public void DeleteAccount_ValidPayload_TranscationSuccess()
		{
			//Arrange
			var accountUid = Guid.NewGuid();
			var accountDetail = new DbAccount
			{
				CustomerAccountID = 1,
				CustomerAccountUID = accountUid,
				AccountName = "ACC03",
				BSSID = "BSS03",
				DealerAccountCode = "DAC03",
				NetworkCustomerCode = "NCC03",
			};
			transaction.Execute(Arg.Any<List<Action>>()).Returns(a =>
			{
				a.Arg<List<Action>>().ForEach(action => action.Invoke());
				return true;
			});

			//Act
			var resultData = accountService.DeleteAccount(accountUid, DateTime.UtcNow, accountDetail);

			//Assert
			Assert.True(resultData);
			transaction.Received(1).Delete(Arg.Any<string>());
			transaction.Received(1).Publish(
				Arg.Is<KafkaMessage>(m => AccountTopic == m.Topic && m.Key == accountUid.ToString()
				&& JsonConvert.SerializeObject(m.Message).Contains("Delete")));
		}

		[Fact]
		public void DeleteAccount_PublishDeleteException_TranscationFailure()
		{
			//Arrange
			var accountUid = Guid.NewGuid();
			var accountDetail = new DbAccount
			{
				CustomerAccountID = 1,
				CustomerAccountUID = accountUid,
				AccountName = "ACC03",
				BSSID = "BSS03",
				DealerAccountCode = "DAC03",
				NetworkCustomerCode = "NCC03",
			};
			transaction.Execute(Arg.Any<List<Action>>()).Returns(a =>
			{
				throw new Exception();
			});

			//Act
			Assert.Throws<Exception>(() => accountService.DeleteAccount(accountUid, DateTime.UtcNow, accountDetail));

			//Assert
			transaction.Received(0).Delete(Arg.Any<string>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
		}
		#endregion

		#region CreateAccountCustomerRelationShip
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void CreateAccountCustomerRelationShip_ValidPayload_TransactionSuccess(bool isParentNull)
		{
			//Arrange
			var accountUid = Guid.NewGuid();
			var accountEvent = new CreateCustomerRelationshipEvent
			{
				AccountCustomerUID = accountUid,
				ParentCustomerUID = isParentNull ? (Guid?)null : Guid.NewGuid(),
				ChildCustomerUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var accountDetail = new DbAccount
			{
				CustomerAccountID = 1,
				CustomerAccountUID = accountUid,
				AccountName = "ACC04",
				BSSID = "BSS04",
				DealerAccountCode = "DAC04",
				NetworkCustomerCode = "NCC04",
			};
			transaction.Execute(Arg.Any<List<Action>>()).Returns(a =>
			{
				a.Arg<List<Action>>().ForEach(action => action.Invoke());
				return true;
			});

			//Act
			var resultData = accountService.CreateAccountCustomerRelationShip(accountEvent, accountDetail);

			//Assert
			Assert.True(resultData);
			transaction.Received(1).Upsert(
				Arg.Is<DbAccount>(account => ValidateCreateCustomerRelationshipAccountObject(
					false, null, accountEvent.ParentCustomerUID, accountEvent.ChildCustomerUID,
					account, accountDetail)));
			transaction.Received(1).Publish(
				Arg.Is<KafkaMessage>(message => ValidateCreateCustomerRelationshipAccountKafkaObject(
					null, accountEvent.ParentCustomerUID, accountEvent.ChildCustomerUID,
					message, accountDetail, accountEvent.ActionUTC)));
		}

		[Fact]
		public void CreateAccountCustomerRelationShip_PublishUpsertException_TransactionFailure()
		{
			//Arrange
			var accountUid = Guid.NewGuid();
			var accountEvent = new CreateCustomerRelationshipEvent
			{
				AccountCustomerUID = accountUid,
				ParentCustomerUID = Guid.NewGuid(),
				ChildCustomerUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var accountDetail = new DbAccount
			{
				CustomerAccountID = 1,
				CustomerAccountUID = accountUid,
				AccountName = "ACC04",
				BSSID = "BSS04",
				DealerAccountCode = "DAC04",
				NetworkCustomerCode = "NCC04",
			};
			transaction.Execute(Arg.Any<List<Action>>()).Returns(a =>
			{
				throw new Exception();
			});

			//Act && Assert;
			Assert.Throws<Exception>(() => accountService.CreateAccountCustomerRelationShip(accountEvent, accountDetail));



			transaction.Received(0).Upsert(Arg.Any<DbAccount>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
		}

		[Theory]
		[InlineData("RemoveDealer")]
		[InlineData("Remove")]
		public void CreateAccountCustomerRelationShip1_ValidPayload_TransactionSuccess(string deleteType)
		{
			//Arrange
			var accountUid = Guid.NewGuid();
			var parentUid = Guid.NewGuid();
			var childUid = Guid.NewGuid();
			var actionUTC = DateTime.UtcNow;
			var accountDetail = new DbAccount
			{
				CustomerAccountID = 1,
				CustomerAccountUID = accountUid,
				AccountName = "ACC04",
				BSSID = "BSS04",
				DealerAccountCode = "DAC04",
				NetworkCustomerCode = "NCC04",
			};
			transaction.Execute(Arg.Any<List<Action>>()).Returns(a =>
			{
				a.Arg<List<Action>>().ForEach(action => action.Invoke());
				return true;
			});

			//Act
			var resultData = accountService.CreateAccountCustomerRelationShip(
				parentUid, childUid, accountDetail, actionUTC, deleteType);

			//Assert
			Assert.True(resultData);
			transaction.Received(1).Upsert(
				Arg.Is<DbAccount>(account => ValidateCreateCustomerRelationshipAccountObject(
					false, deleteType, parentUid, childUid, account, accountDetail)));
			transaction.Received(1).Publish(
				Arg.Is<KafkaMessage>(message => ValidateCreateCustomerRelationshipAccountKafkaObject(
					deleteType, parentUid, childUid, message, accountDetail, actionUTC)));
		}

		[Fact]
		public void CreateAccountCustomerRelationShip1_PublishUpsertException_TransactionFailure()
		{
			//Arrange
			var accountUid = Guid.NewGuid();
			var parentUid = Guid.NewGuid();
			var childUid = Guid.NewGuid();
			var actionUTC = DateTime.UtcNow;
			var accountDetail = new DbAccount
			{
				CustomerAccountID = 1,
				CustomerAccountUID = accountUid,
				AccountName = "ACC04",
				BSSID = "BSS04",
				DealerAccountCode = "DAC04",
				NetworkCustomerCode = "NCC04",
			};
			transaction.Execute(Arg.Any<List<Action>>()).Returns(a => throw new Exception());

			//Act
			Assert.Throws<Exception>(() => accountService.CreateAccountCustomerRelationShip(
				parentUid, childUid, accountDetail, actionUTC, "Remove"));

			//Assert
			transaction.DidNotReceive().Upsert(Arg.Any<DbAccount>());
			transaction.DidNotReceive().Publish(Arg.Any<KafkaMessage>());
		}
		#endregion

		#region DeleteAccountCustomerRelationShip
		[Fact]
		public void DeleteAccountCustomerRelationShip_ValidPayload_TransactionSuccess()
		{
			//Arrange
			var accountUid = Guid.NewGuid();
			var parentUid = Guid.NewGuid();
			var childUid = Guid.NewGuid();
			var actionUTC = DateTime.UtcNow;
			var accountDetail = new DbAccount
			{
				CustomerAccountID = 1,
				CustomerAccountUID = accountUid,
				AccountName = "ACC04",
				BSSID = "BSS04",
				DealerAccountCode = "DAC04",
				NetworkCustomerCode = "NCC04",
			};
			transaction.Execute(Arg.Any<List<Action>>()).Returns(a =>
			{
				a.Arg<List<Action>>().ForEach(action => action.Invoke());
				return true;
			});

			//Act
			var resultData = accountService.DeleteAccountCustomerRelationShip(
				parentUid, childUid, accountDetail, actionUTC);

			//Assert
			Assert.True(resultData);
			transaction.Received(1).Upsert(
				Arg.Is<DbAccount>(account => ValidateCreateCustomerRelationshipAccountObject(
					true, null, parentUid, childUid, account, accountDetail)));
			transaction.Received(1).Publish(
				Arg.Is<KafkaMessage>(message => ValidateCreateCustomerRelationshipAccountKafkaObject(
					null, parentUid, childUid, message, accountDetail, actionUTC)));
		}

		[Fact]
		public void DeleteAccountCustomerRelationShip_PublishUpsertException_TransactionFailure()
		{
			//Arrange
			var accountUid = Guid.NewGuid();
			var parentUid = Guid.NewGuid();
			var childUid = Guid.NewGuid();
			var actionUTC = DateTime.UtcNow;
			var accountDetail = new DbAccount
			{
				CustomerAccountID = 1,
				CustomerAccountUID = accountUid,
				AccountName = "ACC04",
				BSSID = "BSS04",
				DealerAccountCode = "DAC04",
				NetworkCustomerCode = "NCC04",
			};
			transaction.Execute(Arg.Any<List<Action>>()).Returns(a =>
			{
				throw new Exception();
			});

			//Act
			Assert.Throws<Exception>(() => accountService.DeleteAccountCustomerRelationShip(
				parentUid, childUid, accountDetail, actionUTC));

			//Assert
			transaction.DidNotReceive().Upsert(Arg.Any<DbAccount>());
			transaction.DidNotReceive().Publish(Arg.Any<KafkaMessage>());
		}
		#endregion

		#region Private Methods
		private bool ValidateAccountKafkaObject(dynamic source, KafkaMessage target,
			bool isIncludeParentChild, DbAccount sourceAccountDetail)
		{
			var json = JObject.Parse(JsonConvert.SerializeObject(target.Message));
			var eventMsg = JsonConvert.DeserializeObject<AccountEvent>(json.SelectToken("AccountEvent").ToString());
			return AccountTopic == target.Topic
				&& source.CustomerUID.ToString() == target.Key
				&& eventMsg.AccountName == source.CustomerName
				&& eventMsg.AccountUID == source.CustomerUID
				&& (new List<string> { "Create", "Update" }.Contains(eventMsg.Action))
				&& eventMsg.BSSID == source.BSSID
				&& eventMsg.DealerAccountCode == source.DealerAccountCode
				&& eventMsg.NetworkCustomerCode == source.NetworkCustomerCode
				&& isIncludeParentChild
					? (eventMsg.fk_ParentCustomerUID == sourceAccountDetail.fk_ParentCustomerUID
						&& eventMsg.fk_ChildCustomerUID == sourceAccountDetail.fk_ChildCustomerUID)
					: true;
		}

		private bool ValidateAccountObject(dynamic source, DbAccount target,
			bool isIncludeParentChild, DbAccount sourceAccountDetail)
		{
			return source.CustomerUID == target.CustomerAccountUID
				&& source.CustomerName == target.AccountName
				&& source.BSSID == target.BSSID
				&& source.DealerAccountCode == target.DealerAccountCode
				&& source.NetworkCustomerCode == target.NetworkCustomerCode
				&& isIncludeParentChild
					? (target.fk_ParentCustomerUID == sourceAccountDetail.fk_ParentCustomerUID
						&& target.fk_ChildCustomerUID == sourceAccountDetail.fk_ChildCustomerUID)
					: true;
		}

		private bool ValidateCreateCustomerRelationshipAccountObject(bool isDelete, string deleteType,
			Guid? parentUid, Guid chidUid, DbAccount target, DbAccount sourceAccountDetail)
		{
			return target.CustomerAccountID == sourceAccountDetail.CustomerAccountID
				&& target.CustomerAccountUID == sourceAccountDetail.CustomerAccountUID
				&& target.BSSID == sourceAccountDetail.BSSID
				&& target.AccountName == sourceAccountDetail.AccountName
				&& target.NetworkCustomerCode == sourceAccountDetail.NetworkCustomerCode
				&& target.DealerAccountCode == sourceAccountDetail.DealerAccountCode
				&& isDelete
					? (target.fk_ParentCustomerUID == null && target.fk_ChildCustomerUID == null)
					: (string.IsNullOrEmpty(deleteType)
					? (target.fk_ParentCustomerUID == parentUid && target.fk_ChildCustomerUID == chidUid)
					: (deleteType == "RemoveDealer"
					? (target.fk_ParentCustomerUID == chidUid && target.fk_ChildCustomerUID == chidUid)
					: (target.fk_ParentCustomerUID == parentUid && target.fk_ChildCustomerUID == parentUid)));
		}

		private bool ValidateCreateCustomerRelationshipAccountKafkaObject(string deleteType,
			Guid? parentUid, Guid chidUid, KafkaMessage target, DbAccount sourceAccountDetail, DateTime actionUtc)
		{
			var json = JObject.Parse(JsonConvert.SerializeObject(target.Message));
			var eventMsg = JsonConvert.DeserializeObject<AccountEvent>(json.SelectToken("AccountEvent").ToString());
			return target.Topic == AccountTopic
				&& target.Key == sourceAccountDetail.CustomerAccountUID.ToString()
				&& (new List<string> { "Create", "Update", "Delete" }.Contains(eventMsg.Action))
				&& eventMsg.AccountName == sourceAccountDetail.AccountName
				&& eventMsg.BSSID == sourceAccountDetail.BSSID
				&& eventMsg.NetworkCustomerCode == sourceAccountDetail.NetworkCustomerCode
				&& eventMsg.DealerAccountCode == sourceAccountDetail.DealerAccountCode
				&& eventMsg.ActionUTC == actionUtc
				&& string.IsNullOrEmpty(deleteType)
					? (eventMsg.fk_ParentCustomerUID == parentUid && eventMsg.fk_ChildCustomerUID == chidUid)
					: (deleteType == "RemoveDealer"
					? (eventMsg.fk_ParentCustomerUID == chidUid && eventMsg.fk_ChildCustomerUID == chidUid)
					: (eventMsg.fk_ParentCustomerUID == parentUid && eventMsg.fk_ChildCustomerUID == parentUid));
		}
		#endregion
	}
}
