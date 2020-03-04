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
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.Customer.KafkaModel;
using VSS.MasterData.WebAPI.CustomerRepository.Tests.TestDataGenerators;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Transactions;
using Xunit;
using static VSS.MasterData.WebAPI.CustomerRepository.Tests.TestDataEnums;

namespace VSS.MasterData.WebAPI.CustomerRepository.Tests
{
	public class CustomerServiceTests
	{
		private readonly ILogger logger;
		private readonly IConfiguration configuration;
		private readonly ITransactions transaction;
		private readonly ICustomerService customerService;
		private static List<string> CustomerTopics;
		public CustomerServiceTests()
		{
			logger = Substitute.For<ILogger>();
			transaction = Substitute.For<ITransactions>();
			string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			configuration = new ConfigurationBuilder().SetBasePath(currentDirectory)
													.AddJsonFile("appsettings.json", true)
													.AddEnvironmentVariables()
													.Build();
			customerService = new CustomerService(transaction, configuration, logger);
			CustomerTopics = configuration["CustomerTopicNames"]
				.Split(',')
				.Select(t => t + configuration["TopicSuffix"])
				.ToList();
		}

		#region Get Customer
		[Fact]
		public void GetCustomer_ValidCustomer_ReturnsData()
		{
			//Arrange
			var customerUid = Guid.NewGuid();
			var customer = new DbCustomer
			{
				CustomerID = 1,
				CustomerUID = customerUid,
				CustomerName = "CUS01",
				fk_CustomerTypeID = 0,
				LastCustomerUTC = DateTime.UtcNow,
				PrimaryContactEmail = "testCust01@mail.com",
				FirstName = "FN01",
				LastName = "LN01",
				NetworkDealerCode = "NDC01",
				IsActive = true
			};
			var config = new ComparisonConfig
			{
				IgnoreObjectTypes = true,
				MaxMillisecondsDateDifference = 500,
				MaxDifferences = 0
			};
			var accountCompareLogic = new CompareLogic(config);
			transaction.Get<DbCustomer>(Arg.Any<string>()).Returns(new List<DbCustomer> { customer });

			//Act
			var resultData = customerService.GetCustomer(customerUid);

			//Arrange
			Assert.NotNull(resultData);
			transaction.Received(1)
				.Get<DbCustomer>(Arg.Is<string>(q => q.Contains($"UNHEX('{customerUid.ToString("N")}')")));
			ComparisonResult compareResult = accountCompareLogic.Compare(customer, resultData);
			Assert.True(compareResult.Differences.Count == 0);
		}

		[Fact]
		public void GetCustomer_InValidCustomer_EmptyResult()
		{
			//Arrange
			var customerUid = Guid.NewGuid();
			transaction.Get<DbCustomer>(Arg.Any<string>()).Returns(new List<DbCustomer>());

			//Act
			var result = customerService.GetCustomer(customerUid);

			//Arrange
			Assert.Null(result);
			transaction.Received(1)
				.Get<DbCustomer>(Arg.Is<string>(q => q.Contains($"UNHEX('{customerUid.ToString("N")}')")));
		}
		#endregion

		#region Get Associated Customers By User
		[Fact]
		public void GetAssociatedCustomersbyUserUid_ValidUser_ReturnsCustomers()
		{
			//Arrange
			var userUid = Guid.NewGuid();
			var customer1 = new DbCustomer
			{
				CustomerID = 1,
				CustomerUID = Guid.NewGuid(),
				CustomerName = "CUS01",
				fk_CustomerTypeID = 0,
				LastCustomerUTC = DateTime.UtcNow
			};
			var customer2 = new DbCustomer
			{
				CustomerID = 2,
				CustomerUID = Guid.NewGuid(),
				CustomerName = "CUS02",
				fk_CustomerTypeID = 1,
				LastCustomerUTC = DateTime.UtcNow
			};
			var customers = new List<DbCustomer> { customer1, customer2 };
			var config = new ComparisonConfig
			{
				IgnoreObjectTypes = true,
				MaxMillisecondsDateDifference = 500,
				MaxDifferences = 0
			};
			var accountCompareLogic = new CompareLogic(config);
			transaction.Get<DbCustomer>(Arg.Any<string>()).Returns(customers);

			//Act
			var resultData = customerService.GetAssociatedCustomersbyUserUid(userUid);

			//Assert
			Assert.NotNull(resultData);
			Assert.Equal(2, resultData.Count);
			transaction.Received(1)
				.Get<DbCustomer>(Arg.Is<string>(q => q.Contains($"UNHEX('{userUid.ToString("N")}')")));
			ComparisonResult compareResult = accountCompareLogic.Compare(customers, resultData);
			Assert.True(compareResult.Differences.Count == 0);
		}

		[Fact]
		public void GetAssociatedCustomersbyUserUid_InValidUser_EmptyResult()
		{
			//Arrange
			var userUid = Guid.NewGuid();
			List<DbCustomer> customers = null;
			transaction.Get<DbCustomer>(Arg.Any<string>()).Returns(customers);

			//Act
			var result = customerService.GetAssociatedCustomersbyUserUid(userUid);

			//Arrange
			Assert.Null(result);
			transaction.Received(1)
				.Get<DbCustomer>(Arg.Is<string>(q => q.Contains($"UNHEX('{userUid.ToString("N")}')")));
		}
		#endregion

		#region Get Associated Customers For Dealer
		[Fact]
		public void GetAssociatedCustomersForDealer_ValidDealer_ReturnsCustomers()
		{
			//Arrange
			var customerUid = Guid.NewGuid();
			var customers = new List<DbCustomer>
			{
				new DbCustomer
				{
					CustomerID = 1,
					CustomerUID = Guid.NewGuid(),
					fk_CustomerTypeID = 1,
					CustomerName = "CUS01",
					LastCustomerUTC = DateTime.UtcNow.AddDays(-1)
				},
				new DbCustomer
				{
					CustomerID = 2,
					CustomerUID = Guid.NewGuid(),
					fk_CustomerTypeID = 1,
					CustomerName = "CUS02",
					LastCustomerUTC = DateTime.UtcNow.AddDays(-2)
				}
			};

			var config = new ComparisonConfig
			{
				IgnoreObjectTypes = true,
				MaxMillisecondsDateDifference = 500,
				MaxDifferences = 0
			};
			var accountCompareLogic = new CompareLogic(config);
			transaction.Get<DbCustomer>(Arg.Any<string>()).Returns(customers);

			//Act
			var resultData = customerService.GetAssociatedCustomersForDealer(customerUid);

			//Assert
			Assert.NotNull(resultData);
			Assert.Equal(2, resultData.Count);
			transaction.Received(1)
				.Get<DbCustomer>(Arg.Is<string>(q => q.Contains($"UNHEX('{customerUid.ToString("N")}')")));
			ComparisonResult compareResult = accountCompareLogic.Compare(customers, resultData);
			Assert.True(compareResult.Differences.Count == 0);
		}

		[Fact]
		public void GetAssociatedCustomersForDealer_InValidDealer_EmptyResult()
		{
			//Arrange
			var dealerUid = Guid.NewGuid();
			List<DbCustomer> customers = null;
			transaction.Get<DbCustomer>(Arg.Any<string>()).Returns(customers);

			//Act
			var result = customerService.GetAssociatedCustomersForDealer(dealerUid);

			//Arrange
			Assert.Null(result);
			transaction.Received(1)
				.Get<DbCustomer>(Arg.Is<string>(q => q.Contains($"UNHEX('{dealerUid.ToString("N")}')")));
		}
		#endregion

		#region Get Customer By Customer Guids
		[Fact]
		public void GetCustomerByCustomerGuids_ValidCustomers_ReturnsData()
		{
			//Arrange
			var customerUid1 = Guid.NewGuid();
			var customerUid2 = Guid.NewGuid();
			var customeUids = new Guid[] { customerUid1, customerUid2 };
			var customers = new List<DbCustomer>
			{
				new DbCustomer
				{
					CustomerID = 1,
					CustomerUID = customerUid1,
					fk_CustomerTypeID = 1,
					CustomerName = "CUS01",
					LastCustomerUTC = DateTime.UtcNow.AddDays(-1)
				},
				new DbCustomer
				{
					CustomerID = 2,
					CustomerUID = customerUid2,
					fk_CustomerTypeID = 1,
					CustomerName = "CUS02",
					LastCustomerUTC = DateTime.UtcNow.AddDays(-2)
				}
			};

			var config = new ComparisonConfig
			{
				IgnoreObjectTypes = true,
				MaxMillisecondsDateDifference = 500,
				MaxDifferences = 0
			};
			var accountCompareLogic = new CompareLogic(config);
			transaction.Get<DbCustomer>(Arg.Any<string>()).Returns(customers);

			//Act
			var resultData = customerService.GetCustomerByCustomerGuids(customeUids);

			//Assert
			Assert.NotNull(resultData);
			Assert.Equal(2, resultData.Count);
			transaction.Received(1).Get<DbCustomer>(Arg.Is<string>(q =>
					q.Contains($"UNHEX('{customerUid1.ToString("N")}')," +
						$"UNHEX('{customerUid2.ToString("N")}')")));
			ComparisonResult compareResult = accountCompareLogic.Compare(customers, resultData);
			Assert.True(compareResult.Differences.Count == 0);
		}

		[Fact]
		public void GetCustomerByCustomerGuids_InvalidCustomer_EmptyResult()
		{
			//Arrange
			var customerUid1 = Guid.NewGuid();
			var customerUid2 = Guid.NewGuid();
			var customeUids = new Guid[] { customerUid1, customerUid2 };
			List<DbCustomer> customers = null;
			transaction.Get<DbCustomer>(Arg.Any<string>()).Returns(customers);

			//Act
			var result = customerService.GetCustomerByCustomerGuids(customeUids);

			//Arrange
			Assert.Null(result);
			transaction.Received(1).Get<DbCustomer>(Arg.Is<string>(q =>
					q.Contains($"UNHEX('{customerUid1.ToString("N")}')," +
						$"UNHEX('{customerUid2.ToString("N")}')")));
		}
		#endregion

		#region Get Customers By NameSearch
		#endregion

		#region Get Only Associated CustomersbyUserUid
		[Fact]
		public void GetOnlyAssociatedCustomersbyUserUid_GivenInput_ReturnsData()
		{
			//Arrange
			var userUid = Guid.NewGuid();
			var customer = new DbCustomer
			{
				CustomerID = 1,
				CustomerUID = Guid.NewGuid(),
				CustomerName = "CUS01",
				fk_CustomerTypeID = 0,
				BSSID = "Store_123",
				DealerNetwork = "None",
				IsActive = true
			};
			var account = new DbAccount
			{
				AccountName = "ACC01",
				CustomerAccountUID = Guid.NewGuid(),
				BSSID = "Store_123",
				DealerAccountCode = "DAC01",
				NetworkCustomerCode = "NCC01",
				fk_ChildCustomerUID = customer.CustomerUID
			};

			var customerAccount = new List<Tuple<DbCustomer, DbAccount>>
			{
				new Tuple<DbCustomer, DbAccount>(customer, account)
			};
			transaction.Get<DbCustomer, DbAccount>(Arg.Any<string>(), "AccountName")
				.Returns(customerAccount);

			//Act
			var result = customerService.GetOnlyAssociatedCustomersbyUserUid(userUid);

			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			transaction.Received(1).Get<DbCustomer, DbAccount>(Arg.Any<string>(), Arg.Is("AccountName"));
			transaction.Received(1).Get<DbCustomer, DbAccount>(
				Arg.Is<string>(q => q.Contains($"UNHEX('{userUid.ToString("N")}')")), Arg.Is("AccountName"));
			Assert.Equal(customer, result[0].Item1);
			Assert.Equal(account, result[0].Item2);
		}
		#endregion

		#region Get AssetCustomer By AssetGuid
		[Fact]
		public void GetAssetCustomerByAssetGuid_ValidAsset_ReturnsCustomers()
		{
			//Arrange
			var assetUid = Guid.NewGuid();
			var customerUid = Guid.NewGuid();
			var dealerUid = Guid.NewGuid();
			var customers = new List<AssetCustomerDetail>
			{
				new AssetCustomerDetail
				{
					AssetUID = assetUid,
					CustomerType = 0,
					CustomerUID = customerUid,
					ParentCustomerType = 1,
					ParentName = "DLR01",
					ParentCustomerUID = dealerUid,
					CustomerName = "CUS01"
				},
				new AssetCustomerDetail
				{
					AssetUID = assetUid,
					CustomerType = 1,
					CustomerUID = dealerUid,
					ParentCustomerType = -1,
					CustomerName = "DLR01"
				}
			};

			var config = new ComparisonConfig
			{
				IgnoreObjectTypes = true,
				MaxMillisecondsDateDifference = 500,
				MaxDifferences = 0
			};
			var accountCompareLogic = new CompareLogic(config);
			transaction.Get<AssetCustomerDetail>(Arg.Any<string>()).Returns(customers);

			//Act
			var resultData = customerService.GetAssetCustomerByAssetGuid(assetUid);

			//Assert
			Assert.NotNull(resultData);
			Assert.Equal(2, resultData.Count);
			transaction.Received(1).Get<AssetCustomerDetail>(Arg.Is<string>(q =>
					q.Contains($"fk_AssetUID = UNHEX('{assetUid.ToString("N")}')")));
			ComparisonResult compareResult = accountCompareLogic.Compare(customers, resultData);
			Assert.True(compareResult.Differences.Count == 0);
		}

		[Fact]
		public void GetAssetCustomerByAssetGuid_InvalidAsset_EmptyResult()
		{
			//Arrange
			var assetUid = Guid.NewGuid();
			List<AssetCustomerDetail> customers = null;
			transaction.Get<AssetCustomerDetail>(Arg.Any<string>()).Returns(customers);

			//Act
			var result = customerService.GetAssetCustomerByAssetGuid(assetUid);

			//Arrange
			Assert.Null(result);
			transaction.Received(1).Get<AssetCustomerDetail>(Arg.Is<string>(q =>
					q.Contains($"fk_AssetUID = UNHEX('{assetUid.ToString("N")}')")));
		}
		#endregion

		#region Get Account Count
		[Fact]
		public void GetAccountsCount_ValidDealerCustomer_ReturnsNonZeroCount()
		{
			//Arrange
			var dealerUid = Guid.NewGuid();
			var customerUid = Guid.NewGuid();
			var account = new DbAccount
			{
				CustomerAccountID = 1,
				AccountName = "ACC01",
				CustomerAccountUID = Guid.NewGuid(),
				fk_ChildCustomerUID = customerUid,
				fk_ParentCustomerUID = dealerUid,
				RowUpdatedUTC = DateTime.UtcNow.AddDays(-10)
			};
			transaction.Get<object>(Arg.Any<string>())
				.Returns(new List<DbAccount> { account });

			//Act
			var result = customerService.GetAccountsCount(dealerUid, customerUid);

			//Arrange
			Assert.Equal(1, result);
			transaction.Received(1).Get<object>(Arg.Is<string>(q =>
					q.Contains($"fk_ParentCustomerUID = UNHEX('{dealerUid.ToString("N")}')" +
					$" AND fk_ChildCustomerUID = UNHEX('{customerUid.ToString("N")}')")));
		}

		[Fact]
		public void GetAccountsCount_InvalidInput_ReturnsZero()
		{
			//Arrange
			var dealerUid = Guid.NewGuid();
			var customerUid = Guid.NewGuid();
			transaction.Get<object>(Arg.Any<string>())
				.Returns(new List<DbAccount> { });

			//Act
			var result = customerService.GetAccountsCount(dealerUid, customerUid);

			//Arrange
			Assert.Equal(0, result);
			transaction.Received(1).Get<object>(Arg.Is<string>(q =>
					q.Contains($"fk_ParentCustomerUID = UNHEX('{dealerUid.ToString("N")}')" +
					$" AND fk_ChildCustomerUID = UNHEX('{customerUid.ToString("N")}')")));
		}
		#endregion

		#region Create Customer
		[Theory]
		[InlineData("Customer")]
		[InlineData("Dealer")]
		[InlineData("Operations")]
		[InlineData("Corporate")]
		public void CreateCustomer_ValidPayload_TransactionSuccess(string customerType)
		{
			//Arrange
			var customerEvent = new CreateCustomerEvent
			{
				CustomerName = $"{customerType}01",
				CustomerUID = Guid.NewGuid(),
				CustomerType = customerType,
				BSSID = "BSS01",
				DealerNetwork = "None",
				NetworkDealerCode = "NDC01",
				PrimaryContactEmail = $"{customerType}01@mail.com",
				FirstName = $"{customerType}FN01",
				LastName = $"{customerType}LN01",
				IsActive = true,
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
			var resultData = customerService.CreateCustomer(customerEvent);

			//Assert
			Assert.True(resultData);
			transaction.Received(1).Upsert(
				Arg.Is<DbCustomer>(o =>
					ValidateCustomerObject(customerEvent, o, customerType, false, true, null)));
			transaction.Received(1).Publish(Arg.Is<List<KafkaMessage>>(messages =>
					messages.TrueForAll(m =>
						ValidateCustomerKafkaObject(customerEvent, m, customerType, false, null))));
		}

		[Fact]
		public void CreateCustomer_PublishUpsertException_TransactionException()
		{
			//Arrange
			var customerEvent = new CreateCustomerEvent
			{
				CustomerName = "CUS01",
				CustomerUID = Guid.NewGuid(),
				CustomerType = "Customer",
				BSSID = "BSS01",
				DealerNetwork = "None",
				NetworkDealerCode = "NDC01",
				PrimaryContactEmail = "CUS01@mail.com",
				FirstName = "FN01",
				LastName = "LN01",
				IsActive = true,
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			transaction.Execute(Arg.Any<List<Action>>())
				.Returns(a => { throw new Exception(); });

			//Act
			Assert.Throws<Exception>(() => customerService.CreateCustomer(customerEvent));

			//Assert
			transaction.DidNotReceive().Upsert(Arg.Any<DbCustomer>());
			transaction.DidNotReceive().Publish(Arg.Any<List<KafkaMessage>>());
		}
		#endregion

		#region Update Customer
		[Theory]
		[InlineData("Customer", true, true)]
		[InlineData("Customer", true, false)]
		[InlineData("Customer", false, null)]
		public void UpdateCustomer_ValidPayload_TransactionSuccess(
			string customerType, bool hasActive, bool? isActive)
		{
			//Arrange
			var customerEvent = new UpdateCustomerEvent
			{
				CustomerName = $"{customerType}01",
				CustomerUID = Guid.NewGuid(),
				BSSID = "BSS01",
				DealerNetwork = "None",
				NetworkDealerCode = "NDC01",
				PrimaryContactEmail = $"{customerType}01@mail.com",
				FirstName = $"{customerType}FN01",
				LastName = $"{customerType}LN01",
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var custDetail = new DbCustomer
			{
				CustomerID = 1,
				CustomerUID = customerEvent.CustomerUID,
				fk_CustomerTypeID = 0,
				IsActive = true
			};
			if (hasActive)
			{
				customerEvent.IsActive = isActive;
			}

			transaction.Execute(Arg.Any<List<Action>>())
					.Returns(a =>
					{
						a.Arg<List<Action>>().ForEach(action => action.Invoke());
						return true;
					});

			//Act
			var resultData = customerService.UpdateCustomer(customerEvent, custDetail);

			//Assert
			Assert.True(resultData);
			transaction.Received(1).Upsert(
				Arg.Is<DbCustomer>(o =>
					ValidateCustomerObject(customerEvent, o, customerType, true, hasActive, custDetail)));
			transaction.Received(1).Publish(Arg.Is<List<KafkaMessage>>(messages =>
					messages.TrueForAll(m =>
						ValidateCustomerKafkaObject(customerEvent, m, customerType, true, custDetail))));
		}
		[Fact]
		public void UpdateCustomer_PublishUpsertException_TransactionException()
		{
			//Arrange
			var customerEvent = new UpdateCustomerEvent
			{
				CustomerName = "CUS01",
				CustomerUID = Guid.NewGuid(),
				BSSID = "BSS01",
				DealerNetwork = "None",
				NetworkDealerCode = "NDC01",
				PrimaryContactEmail = "CUS01@mail.com",
				FirstName = "FN01",
				LastName = "LN01",
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};
			var custDetail = new DbCustomer
			{
				CustomerID = 1,
				CustomerUID = customerEvent.CustomerUID,
				fk_CustomerTypeID = 0,
				IsActive = true
			};

			transaction.Execute(Arg.Any<List<Action>>())
				.Returns(a =>
				{
					throw new Exception();
				});

			//Act
			Assert.Throws<Exception>(() => customerService.UpdateCustomer(customerEvent, custDetail));

			//Assert

			transaction.DidNotReceive().Upsert(Arg.Any<DbCustomer>());
			transaction.DidNotReceive().Publish(Arg.Any<List<KafkaMessage>>());
		}
		#endregion

		#region Delete Customer
		[Fact]
		public void DeleteCustomer_ValidPayload_TranscationSuccess()
		{
			//Arrange
			var customerUid = Guid.NewGuid();
			transaction.Execute(Arg.Any<List<Action>>()).Returns(a =>
			{
				a.Arg<List<Action>>().ForEach(action => action.Invoke());
				return true;
			});
			var deleteQuery = $"DELETE FROM md_customer_Customer " +
				$"WHERE CustomerUID = UNHEX('{customerUid.ToString("N")}');";

			//Act
			var resultData = customerService.DeleteCustomer(customerUid, DateTime.UtcNow);

			//Assert
			Assert.True(resultData);
			transaction.Received(1).Delete(Arg.Is(deleteQuery));
			transaction.Received(1).Publish(
				Arg.Is<List<KafkaMessage>>(messages =>
				messages.TrueForAll(m => CustomerTopics.Contains(m.Topic) && m.Key == customerUid.ToString()
					&& JsonConvert.SerializeObject(m.Message).Contains("DeleteCustomerEvent"))));
		}

		[Fact]
		public void DeleteCustomer_PublishDeleteException_TranscationFailure()
		{
			//Arrange
			var customerUid = Guid.NewGuid();
			transaction.Execute(Arg.Any<List<Action>>()).Returns(a =>
			{
				throw new Exception();
			});

			//Act
			Assert.Throws<Exception>(() => customerService.DeleteCustomer(customerUid, DateTime.UtcNow));

			//Assert
			transaction.DidNotReceive().Delete(Arg.Any<string>());
			transaction.DidNotReceive().Publish(Arg.Any<List<KafkaMessage>>());
		}
		#endregion

		#region Create User Customer Relationship
		[Theory]
		[MemberData(nameof(CustomerRepositoryTestDataGenerator.GetCreateUserCustomerRelationshipTestData),
			MemberType = typeof(CustomerRepositoryTestDataGenerator))]
		public void CreateUserCutsomerRelationship_GivenPayload_ExpectedStatus(
			CreateUserCustomerRelationshipEvent relationshipEvent, bool hasValidCustomer, bool transactionStatus,
			int upsertCalls, int publishCalls, bool hasException)
		{
			//Arrange
			DbCustomer customerData = hasValidCustomer
				? new DbCustomer() { CustomerID = 109, CustomerUID = relationshipEvent.CustomerUID } : null;
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

			if (hasException)
				Assert.Throws<Exception>(() => customerService.CreateUserCustomerRelationship(relationshipEvent));
			else
			{
				var resultData = customerService.CreateUserCustomerRelationship(relationshipEvent);


				Assert.Equal(transactionStatus, resultData);
			}

			//Assert
			transaction.Received(upsertCalls).Upsert(
				Arg.Is<DbUserCustomer>(userCust =>
				ValidateCustomerUserObject(relationshipEvent, userCust, customerData)));
			transaction.Received(publishCalls).Publish(
				Arg.Is<List<KafkaMessage>>(messages => messages
				.TrueForAll(m => ValidateCustomerUserKafkaObject(false, false, m, relationshipEvent))));
		}

		[Theory]
		[MemberData(nameof(CustomerRepositoryTestDataGenerator.GetUpdateUserCustomerRelationshipTestData),
			MemberType = typeof(CustomerRepositoryTestDataGenerator))]
		public void UpdateUserCustomerRelationship_GivenPayload_ExpectedStatus(
			UpdateUserCustomerRelationshipEvent relationshipEvent, bool transactionStatus, int upsertCalls,
			int publishCalls, bool hasException)
		{
			//Arrange
			if (hasException)
			{
				transaction.When(x => x.Publish(Arg.Any<List<KafkaMessage>>()))
					.Do(x => throw new Exception());
			}

			if (hasException)
			{
				Assert.Throws<Exception>(() => customerService.UpdateUserCustomerRelationship(relationshipEvent));
			}
			else
			{
				var resultData = customerService.UpdateUserCustomerRelationship(relationshipEvent);

				Assert.Equal(transactionStatus, resultData);
			}
			//Act

			transaction.Received(upsertCalls).Upsert(Arg.Any<DbUserCustomer>());
			transaction.Received(publishCalls).Publish(Arg.Is<List<KafkaMessage>>(messages => messages
				.TrueForAll(m => ValidateCustomerUserKafkaObject(false, true, m, relationshipEvent))));
		}

		[Theory]
		[MemberData(nameof(CustomerRepositoryTestDataGenerator.GetDeleteUserCustomerRelationshipTestData),
			MemberType = typeof(CustomerRepositoryTestDataGenerator))]
		public void DeleteUserCutsomerRelationship_GivenPayload_ExpectedStatus(
			DeleteUserCustomerRelationshipEvent relationshipEvent, bool transactionStatus,
			int upsertCalls, int deleteCalls, int publishCalls, bool hasException)
		{
			//Arrange
			if (hasException)
			{
				transaction.When(x => x.Delete(Arg.Any<string>()))
				.Do(e => throw new Exception());
			}
			transaction.Execute(Arg.Any<List<Action>>())
				.Returns(a =>
				{
					a.Arg<List<Action>>().ForEach(action => action.Invoke());
					return true;
				});
			var deleteQuery = $"DELETE FROM md_customer_CustomerUser " +
				$"WHERE fk_CustomerUID = UNHEX('{relationshipEvent.CustomerUID.ToString("N")}') " +
				$"AND fk_UserUID = UNHEX('{relationshipEvent.UserUID.ToString("N")}');";

			//Act
			if (hasException)
			{
				Assert.Throws<Exception>(() => customerService.DeleteUserCustomerRelationship(relationshipEvent));
			}
			else
			{
				var resultData = customerService.DeleteUserCustomerRelationship(relationshipEvent);
				Assert.Equal(transactionStatus, resultData);
			}


			//Assert
			transaction.Received(upsertCalls).Upsert(Arg.Any<DbUserCustomer>());
			transaction.Received(deleteCalls).Delete(Arg.Is(deleteQuery));
			transaction.Received(publishCalls).Publish(
				Arg.Is<List<KafkaMessage>>(messages => messages
				.TrueForAll(m => ValidateCustomerUserKafkaObject(true, false, m, relationshipEvent))));
		}
		#endregion

		#region Private Methods
		private bool ValidateCustomerObject(dynamic source, DbCustomer target,
			string customerType, bool isIncludeID, bool hasActiveFlag, DbCustomer sourceCustomerDetail)
		{
			Enum.TryParse(customerType, out CustomerType custType);

			return source.CustomerUID == target.CustomerUID
				&& source.CustomerName == target.CustomerName
				&& source.NetworkDealerCode == target.NetworkDealerCode
				&& (int)custType == target.fk_CustomerTypeID
				&& source.FirstName == target.FirstName
				&& source.PrimaryContactEmail == target.PrimaryContactEmail
				&& source.LastName == target.LastName
				&& source.BSSID == target.BSSID
				&& source.DealerNetwork == target.DealerNetwork
				&& source.NetworkCustomerCode == target.NetworkCustomerCode
				&& source.DealerAccountCode == target.DealerAccountCode
				&& hasActiveFlag ? source.IsActive == target.IsActive
					: sourceCustomerDetail.IsActive == target.IsActive
				&& isIncludeID ? sourceCustomerDetail.CustomerID == target.CustomerID : true;
		}

		private bool ValidateCustomerKafkaObject(dynamic source, KafkaMessage target, string customerType,
			bool isIncludeID, DbCustomer sourceCustomerDetail)
		{
			Enum.TryParse(customerType, out CustomerType custType);
			var json = JObject.Parse(JsonConvert.SerializeObject(target.Message));
			dynamic eventMsg;
			if (!isIncludeID)
			{
				eventMsg = JsonConvert.DeserializeObject<CreateCustomerEvent>(
						json.SelectToken($"CreateCustomerEvent").ToString());
			}
			else
			{
				eventMsg = JsonConvert.DeserializeObject<UpdateCustomerEvent>(
						json.SelectToken($"UpdateCustomerEvent").ToString());
			}

			return CustomerTopics.Contains(target.Topic)
				&& source.CustomerUID.ToString() == target.Key
				&& eventMsg.CustomerName == source.CustomerName
				&& eventMsg.CustomerUID == source.CustomerUID
				&& eventMsg.BSSID == source.BSSID
				&& eventMsg.DealerNetwork == source.DealerNetwork
				&& eventMsg.NetworkDealerCode == source.NetworkDealerCode
				&& eventMsg.DealerAccountCode == source.DealerAccountCode
				&& eventMsg.NetworkCustomerCode == source.NetworkCustomerCode
				&& eventMsg.FirstName == source.FirstName
				&& eventMsg.LastName == source.LastName
				&& eventMsg.PrimaryContactEmail == source.PrimaryContactEmail
				&& !isIncludeID ? true
					: sourceCustomerDetail.fk_CustomerTypeID == (int)custType;
		}

		private bool ValidateCustomerUserObject(dynamic source, DbUserCustomer target,
			DbCustomer customerDetail)
		{
			return target.fk_CustomerUID == source.CustomerUID
				&& target.fk_UserUID == source.UserUID
				&& target.fk_CustomerID == customerDetail.CustomerID;
		}
		private bool ValidateCustomerUserKafkaObject(bool isDissociate, bool isUpdate, KafkaMessage target,
			dynamic source)
		{
			var isValid = target.Key == source.CustomerUID.ToString()
				&& CustomerTopics.Contains(target.Topic);
			if (isUpdate)
			{
				var json = JObject.Parse(JsonConvert.SerializeObject(target.Message));
				var eventMsg = JsonConvert.DeserializeObject<UpdateUserCustomerRelationshipEvent>(
					json.SelectToken("UpdateUserCustomerRelationshipEvent").ToString());
				return isValid
					&& eventMsg.CustomerUID == source.CustomerUID
					&& eventMsg.UserUID == source.UserUID
					&& eventMsg.ActionUTC == source.ActionUTC
					&& eventMsg.ReceivedUTC == source.ReceivedUTC
					&& eventMsg.JobTitle == source.JobTitle
					&& eventMsg.JobType == source.JobType;
			}

			var messages = target.Message;
			//if (messages.Any())
			//{
			var json1 = JObject.Parse(JsonConvert.SerializeObject(messages));
			if (isDissociate)
			{
				var value = json1.SelectToken("DissociateCustomerUserEvent")?.ToString();
				if (value != null)
				{
					var eventMsg1 = JsonConvert.DeserializeObject<DissociateCustomerUserEvent>(value);
					return isValid
							&& eventMsg1.CustomerUID == source.CustomerUID
							&& eventMsg1.UserUID == source.UserUID
							&& eventMsg1.ActionUTC == source.ActionUTC
							&& eventMsg1.ReceivedUTC == source.ReceivedUTC;
				}


				var json2 = JObject.Parse(JsonConvert.SerializeObject(messages));
				var value1 = json2.SelectToken("DeleteUserCustomerRelationshipEvent")?.ToString();
				if (value1 != null)
				{
					var eventMsg2 = JsonConvert.DeserializeObject<DeleteUserCustomerRelationshipEvent>(
						value1);

					return isValid
							&& eventMsg2.CustomerUID == source.CustomerUID
							&& eventMsg2.UserUID == source.UserUID
							&& eventMsg2.ActionUTC == source.ActionUTC
							&& eventMsg2.ReceivedUTC == source.ReceivedUTC;
				}
			}
			else
			{
				var cuvalue = json1.SelectToken("AssociateCustomerUserEvent")?.ToString();
				if (cuvalue != null)
				{
					var eventMsg1 = JsonConvert.DeserializeObject<AssociateCustomerUserEvent>(
						json1.SelectToken("AssociateCustomerUserEvent").ToString());
					return isValid
							&& eventMsg1.CustomerUID == source.CustomerUID
							&& eventMsg1.UserUID == source.UserUID
							&& eventMsg1.ActionUTC == source.ActionUTC
							&& eventMsg1.ReceivedUTC == source.ReceivedUTC;
				}
				var json2 = JObject.Parse(JsonConvert.SerializeObject(messages));
				var value = json2.SelectToken("CreateUserCustomerRelationshipEvent")?.ToString();
				if (value != null)
				{
					var eventMsg2 = JsonConvert.DeserializeObject<CreateUserCustomerRelationshipEvent>(value);
					return isValid && eventMsg2.CustomerUID == source.CustomerUID
									&& eventMsg2.UserUID == source.UserUID
									&& eventMsg2.ActionUTC == source.ActionUTC
									&& eventMsg2.ReceivedUTC == source.ReceivedUTC
									&& eventMsg2.JobTitle == source.JobTitle
									&& eventMsg2.JobType == source.JobType;
				}




			}
			//}
			return false;
		}
		#endregion
	}
}
