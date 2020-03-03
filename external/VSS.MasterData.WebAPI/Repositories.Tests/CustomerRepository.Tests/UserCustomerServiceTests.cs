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
using VSS.MasterData.WebAPI.CustomerRepository.Tests.TestDataGenerators;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Transactions;
using Xunit;

namespace VSS.MasterData.WebAPI.CustomerRepository.Tests
{
	public class UserCustomerServiceTests
	{
		private readonly ILogger logger;
		private readonly IConfiguration configuration;
		private readonly ITransactions transaction;
		private readonly IUserCustomerService userCustomerService;
		private static List<string> CustomerTopics;
		public UserCustomerServiceTests()
		{
			logger = Substitute.For<ILogger>();
			transaction = Substitute.For<ITransactions>();
			string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			configuration = new ConfigurationBuilder().SetBasePath(currentDirectory)
													.AddJsonFile("appsettings.json", true)
													.AddEnvironmentVariables()
													.Build();
			userCustomerService = new UserCustomerService(transaction, configuration, logger);
			CustomerTopics = configuration["CustomerTopicNames"]
				.Split(',')
				.Select(t => t + configuration["TopicSuffix"])
				.ToList();
		}

		#region GetCustomerUser
		[Fact]
		public void GetCustomerUser_ValidCustomerUser_ReturnsResult()
		{
			//Arrange
			var customerUser = new DbUserCustomer
			{
				fk_CustomerID = 88,
				fk_CustomerUID = Guid.NewGuid(),
				fk_UserUID = Guid.NewGuid(),
				LastUserUTC = DateTime.UtcNow,
				UserCustomerID = 123
			};
			var config = new ComparisonConfig
			{
				IgnoreObjectTypes = true,
				MaxMillisecondsDateDifference = 500,
				MaxDifferences = 0
			};
			var assetCustomerCompareLogic = new CompareLogic(config);
			transaction.Get<DbUserCustomer>(Arg.Any<string>()).Returns(new List<DbUserCustomer> { customerUser });

			//Act
			var resultData = userCustomerService.GetCustomerUser(customerUser.fk_CustomerUID, customerUser.fk_UserUID);

			//Arrange
			Assert.NotNull(resultData);
			ComparisonResult compareResult = assetCustomerCompareLogic.Compare(customerUser, resultData);
			Assert.True(compareResult.Differences.Count == 0);
		}

		[Fact]
		public void GetCustomerUser_InValidCustomerUser_EmptyResult()
		{
			//Arrange
			var customerUid = Guid.NewGuid();
			var assetUid = Guid.NewGuid();
			transaction.Get<DbUserCustomer>(Arg.Any<string>()).Returns(new List<DbUserCustomer>());

			//Act
			var result = userCustomerService.GetCustomerUser(customerUid, assetUid);

			//Arrange
			Assert.Null(result);
		}
		#endregion

		#region GetUsersForCustomer
		[Fact]
		public void GetUsersForCustomer_ValidCustomerUsers_ReturnsResult()
		{
			//Arrange
			var customerUid = Guid.NewGuid();
			var customerUser = new DbUserCustomer
			{
				fk_CustomerID = 88,
				fk_CustomerUID = customerUid,
				fk_UserUID = Guid.NewGuid(),
				LastUserUTC = DateTime.UtcNow,
				UserCustomerID = 123
			};
			var customerUser1 = new DbUserCustomer
			{
				fk_CustomerID = 85,
				fk_CustomerUID = customerUid,
				fk_UserUID = Guid.NewGuid(),
				LastUserUTC = DateTime.UtcNow,
				UserCustomerID = 125
			};
			var customerUsers = new List<DbUserCustomer> { customerUser, customerUser1 };
			var config = new ComparisonConfig
			{
				IgnoreObjectTypes = true,
				MaxMillisecondsDateDifference = 500,
				MaxDifferences = 0
			};
			var assetCustomerCompareLogic = new CompareLogic(config);
			transaction.Get<DbUserCustomer>(Arg.Any<string>()).Returns(customerUsers);

			//Act
			var resultData = userCustomerService
				.GetUsersForCustomer(customerUser.fk_CustomerUID,
				new List<Guid> { customerUser.fk_UserUID, customerUser1.fk_UserUID });

			//Arrange
			Assert.NotNull(resultData);
			Assert.Equal(2, resultData.Count());
			ComparisonResult compareResult = assetCustomerCompareLogic.Compare(customerUsers, resultData);
			Assert.True(compareResult.Differences.Count == 0);
		}
		#endregion

		#region AssociateCustomerUser
		[Theory]
		[MemberData(nameof(CustomerRepositoryTestDataGenerator.GetAssociateCustomerUserTestData),
			MemberType = typeof(CustomerRepositoryTestDataGenerator))]
		public void AssociateCustomerUser_GivenPayload_ExpectedTransactionStatus(AssociateCustomerUserEvent userEvent,
			bool hasValidCustomer, bool transactionStatus, int upsertCalls, int publishCalls, bool hasException)
		{
			//Arrange
			DbCustomer customerData = hasValidCustomer ? new DbCustomer() { CustomerID = 109 } : null;
			if (hasException)
			{
				transaction.Get<DbCustomer>(Arg.Any<string>()).Returns(e => throw new Exception());
			}
			else
			{
				transaction.Get<DbCustomer>(Arg.Any<string>()).Returns(new List<DbCustomer> { customerData });
			}
			transaction.Execute(Arg.Any<List<Action>>())
				.Returns(a =>
				{
					a.Arg<List<Action>>().ForEach(action => action.Invoke());
					return true;
				});

			//Act
			if (hasException)
			{
				Assert.Throws<Exception>(() => userCustomerService.AssociateCustomerUser(userEvent));
			}
			else
			{
				var resultData = userCustomerService.AssociateCustomerUser(userEvent);

				Assert.Equal(transactionStatus, resultData);
			}

			//Assert
			transaction.Received(upsertCalls).Upsert(
				Arg.Is<DbUserCustomer>(userCust => ValidateCustomerUserObject(userEvent, userCust, customerData)));
			transaction.Received(publishCalls).Publish(
				Arg.Is<List<KafkaMessage>>(messages => messages
				.TrueForAll(m => ValidateCustomerUserKafkaObject(false, m, userEvent))));
		}
		#endregion

		#region DissociateCustomerUser
		[Theory]
		[MemberData(nameof(CustomerRepositoryTestDataGenerator.GetDissociateCustomerUserTestData),
			MemberType = typeof(CustomerRepositoryTestDataGenerator))]
		public void DissociateCustomerUser_GivenPayload_ExpectedTransactionStatus(DissociateCustomerUserEvent userEvent,
			bool transactionStatus, int deleteCalls, int publishCalls, bool hasException)
		{
			//Arrange
			var deleteQuery = $"DELETE FROM md_customer_CustomerUser " +
				$"WHERE fk_CustomerUID = UNHEX('{userEvent.CustomerUID.ToString("N")}') " +
				$"AND fk_UserUID = UNHEX('{userEvent.UserUID.ToString("N")}');";
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
			{
				Assert.Throws<Exception>(() => userCustomerService.DissociateCustomerUser(userEvent));
			}
			else
			{
				var resultData = userCustomerService.DissociateCustomerUser(userEvent);

				Assert.Equal(transactionStatus, resultData);
			}


			//Assert
			transaction.Received(deleteCalls).Delete(Arg.Is(deleteQuery));
			transaction.Received(publishCalls).Publish(
				Arg.Is<List<KafkaMessage>>(messages => messages
				.TrueForAll(m => ValidateCustomerUserKafkaObject(true, m, userEvent))));
		}
		#endregion

		#region BulkDissociateCustomerUser
		[Fact]
		public void BulkDissociateCustomerUser_ValidPayload_SuccessTransactionStatus()
		{
			//Arrange
			var customerUid = Guid.NewGuid();
			var userUid1 = Guid.NewGuid();
			var userUid2 = Guid.NewGuid();
			var userIds = new List<Guid> { userUid1, userUid2 };
			var actionUtc = DateTime.UtcNow;

			var userCustomers
				= new List<DbUserCustomer> {
					new DbUserCustomer { fk_CustomerUID=customerUid,UserCustomerID =1, fk_UserUID = userUid1 },
					new DbUserCustomer { fk_CustomerUID=customerUid,UserCustomerID =2, fk_UserUID = userUid2 }
				};
			transaction.Get<DbUserCustomer>(Arg.Any<string>()).Returns(userCustomers);

			var deleteQuery = $"DELETE FROM md_customer_CustomerUser WHERE UserCustomerID IN (1,2);";
			transaction.Execute(Arg.Any<List<Action>>())
				.Returns(a =>
				{
					a.Arg<List<Action>>().ForEach(action => action.Invoke());
					return true;
				});

			//Act
			var resultData = userCustomerService
				.BulkDissociateCustomerUser(customerUid, userIds, actionUtc);

			//Assert
			Assert.True(resultData);
			transaction.Received(1).Get<DbUserCustomer>(
				Arg.Is<string>(m => m.Contains(customerUid.ToString("N"))
				&& m.Contains(userUid1.ToString("N")) && m.Contains(userUid2.ToString("N"))));
			transaction.Received(1).Delete(Arg.Is(deleteQuery));
			transaction.Received(1).Publish(Arg.Is<List<KafkaMessage>>(messages => messages
			.TrueForAll(m => ValidateBulkDissociateKafkaObject(m, customerUid, userIds, actionUtc))));
		}

		[Fact]
		public void BulkDissociateCustomerUser_InValidUsersForCustomer_NoTransaction()
		{
			//Arrange
			var customerUid = Guid.NewGuid();
			var userUid1 = Guid.NewGuid();
			var userUid2 = Guid.NewGuid();
			var userIds = new List<Guid> { userUid1, userUid2 };

			//Act
			var resultData = userCustomerService
				.BulkDissociateCustomerUser(customerUid, userIds, DateTime.UtcNow);

			//Assert
			Assert.False(resultData);
			transaction.Received(1).Get<DbUserCustomer>(
				Arg.Is<string>(m => m.Contains(customerUid.ToString("N"))
				&& m.Contains(userUid1.ToString("N")) && m.Contains(userUid2.ToString("N"))));
			transaction.DidNotReceive().Delete(Arg.Any<string>());
			transaction.DidNotReceive().Publish(Arg.Any<List<KafkaMessage>>());
		}

		[Fact]
		public void BulkDissociateCustomerUser_GeUsersForCustomerException_NoTransaction()
		{
			//Arrange
			var customerUid = Guid.NewGuid();
			var userUid1 = Guid.NewGuid();
			var userUid2 = Guid.NewGuid();
			var userIds = new List<Guid> { userUid1, userUid2 };

			transaction.Get<DbUserCustomer>(Arg.Any<string>())
				.Returns(x => throw new Exception());

			//Act
			Assert.Throws<Exception>(() => userCustomerService
				.BulkDissociateCustomerUser(customerUid, userIds, DateTime.UtcNow));

			//Assert
			transaction.Received(1).Get<DbUserCustomer>(
				Arg.Is<string>(m => m.Contains(customerUid.ToString("N"))
				&& m.Contains(userUid1.ToString("N")) && m.Contains(userUid2.ToString("N"))));
			transaction.DidNotReceive().Delete(Arg.Any<string>());
			transaction.DidNotReceive().Publish(Arg.Any<List<KafkaMessage>>());
		}
		#endregion

		#region Private Methods
		private bool ValidateCustomerUserObject(dynamic source, DbUserCustomer target,
			DbCustomer customerDetail)
		{
			return target.fk_CustomerUID == source.CustomerUID
				&& target.fk_UserUID == source.UserUID
				&& target.fk_CustomerID == customerDetail.CustomerID;
		}

		private bool ValidateCustomerUserKafkaObject(bool isDissociate, KafkaMessage target, dynamic source)
		{
			var isValid = target.Key == source.CustomerUID.ToString()
				&& CustomerTopics.Contains(target.Topic);

			var json = JObject.Parse(JsonConvert.SerializeObject(target.Message));
			if (isDissociate)
			{
				var eventMsg = JsonConvert.DeserializeObject<DissociateCustomerUserEvent>(
				json.SelectToken("DissociateCustomerUserEvent").ToString());
				return isValid
					&& eventMsg.CustomerUID == source.CustomerUID
					&& eventMsg.UserUID == source.UserUID
					&& eventMsg.ActionUTC == source.ActionUTC
					&& eventMsg.ReceivedUTC == source.ReceivedUTC;
			}
			else
			{
				var eventMsg = JsonConvert.DeserializeObject<AssociateCustomerUserEvent>(
				json.SelectToken("AssociateCustomerUserEvent").ToString());
				return isValid
					&& eventMsg.CustomerUID == source.CustomerUID
					&& eventMsg.UserUID == source.UserUID
					&& eventMsg.ActionUTC == source.ActionUTC
					&& eventMsg.ReceivedUTC == source.ReceivedUTC;
			}
		}

		private bool ValidateBulkDissociateKafkaObject(KafkaMessage target, Guid customerUid,
			List<Guid> userUids, DateTime actionUtc)
		{
			var isValid = target.Key == customerUid.ToString()
				&& CustomerTopics.Contains(target.Topic);
			var json = JObject.Parse(JsonConvert.SerializeObject(target.Message));
			var eventMsg = JsonConvert.DeserializeObject<DissociateCustomerUserEvent>(
				json.SelectToken("DissociateCustomerUserEvent").ToString());
			return isValid
					&& eventMsg.CustomerUID == customerUid
					&& userUids.Contains(eventMsg.UserUID)
					&& eventMsg.ActionUTC == actionUtc
					&& eventMsg.ReceivedUTC > actionUtc;
		}
		#endregion
	}
}
