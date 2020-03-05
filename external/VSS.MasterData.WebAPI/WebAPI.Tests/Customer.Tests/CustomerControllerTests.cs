using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.IO;
using System.Reflection;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.Customer.Tests.TestDataGenerators;
using CustomerClientModel = VSS.MasterData.WebAPI.ClientModel.Customer;
using ACDbModel = VSS.MasterData.WebAPI.DbModel.DbAssetCustomer;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using Xunit;
using static VSS.MasterData.WebAPI.Customer.Tests.TestDataEnums;
using APIEnums = VSS.MasterData.WebAPI.Utilities.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using VSS.MasterData.WebAPI.Customer.KafkaModel;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using VSS.MasterData.WebAPI.Utilities;
using VSS.MasterData.WebAPI.Customer.Controllers.V1;

namespace VSS.MasterData.WebAPI.Customer.Tests
{
	public class CustomerControllerTests
	{
		private readonly string JWTAssertion = "xxxx.eyJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6Il" +
			"ZMMi4wIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy91c2VydHlwZSI6IkFQUExJQ0FUSU9OX1VTRVIiLCJodHRwOi8vd3NvMi5v" +
			"cmcvY2xhaW1zL3V1aWQiOiJlZDZmNGQ0YS0wMzI2LTQwZTUtYjY0Zi1hMTZjMmM0YTU5NDMifQ==.xxxx";
		private readonly ILogger logger;
		private readonly IConfiguration configuration;
		private readonly ICustomerService customerService;
		private readonly IAccountService accountService;
		private readonly IUserCustomerService userCustomerService;
		private readonly ICustomerAssetService customerAssetService;
		private readonly CustomerController customerController;

		public CustomerControllerTests()
		{
			logger = Substitute.For<ILogger>();
			string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			configuration = new ConfigurationBuilder().SetBasePath(currentDirectory)
													.AddJsonFile("appsettings.json", true)
													.AddEnvironmentVariables()
													.Build();
			customerService = Substitute.For<ICustomerService>();
			accountService = Substitute.For<IAccountService>();
			userCustomerService = Substitute.For<IUserCustomerService>();
			customerAssetService = Substitute.For<ICustomerAssetService>();

			customerController = new CustomerController(customerService, accountService, userCustomerService
				, customerAssetService, logger)
			{
				ControllerContext = new ControllerContext()
				{
					HttpContext = new DefaultHttpContext()
				}
			};
		}

		#region Create Customer
		[Theory]
		[MemberData(nameof(CustomerTestDataGenerator.GetCreateCustomerTestData),
			MemberType = typeof(CustomerTestDataGenerator))]
		public void CreateCustomer_GivenInput_ExpectedStatusCodeWithMessage(CreateCustomerEvent customerEvent,
			CustomerType type, bool isAccountExists, bool isCustomerExists, bool createAccountStatus,
			 bool createCustomerStatus, int? statusCode, string message)
		{
			//Arrange
			DbAccount accountData = isAccountExists ? new DbAccount() : null;
			DbCustomer customerData = isCustomerExists ? new DbCustomer() : null;

			switch (type)
			{
				case CustomerType.Account:
					{
						accountService.GetAccount(customerEvent.CustomerUID).Returns(accountData);
						accountService.CreateAccount(customerEvent).Returns(createAccountStatus);
						break;
					}
				case CustomerType.Customer:
				case CustomerType.Operations:
				case CustomerType.Corporate:
					{
						customerService.GetCustomer(customerEvent.CustomerUID).Returns(customerData);
						customerService.CreateCustomer(customerEvent).Returns(createCustomerStatus);
						break;
					}
				case CustomerType.Dealer:
					{
						customerService.GetCustomer(customerEvent.CustomerUID).Returns(customerData);
						customerService.CreateCustomer(customerEvent).Returns(createCustomerStatus);
						customerService.GetCustomerRelationships(Arg.Any<Guid>(), Arg.Any<Guid>())
							.Returns(new List<DbCustomerRelationshipNode>());
						customerService.CreateCustomerRelationShip(Arg.Any<CreateCustomerRelationshipEvent>())
							.Returns(true);
						break;
					}
				default: break;
			}

			//Act
			var response = customerController.CreateCustomer(customerEvent);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(statusCode, responseData.StatusCode);
			if (statusCode != (int)HttpStatusCode.OK)
			{
				Assert.Equal(message, responseData.Value);
			}
			else
			{
				switch (type)
				{
					case CustomerType.Dealer:
						{
							customerService.Received(1).GetCustomer(Arg.Is<Guid>(customerEvent.CustomerUID));
							customerService.Received(1).CreateCustomer(Arg.Is<CreateCustomerEvent>(customerEvent));
							accountService.DidNotReceive().CreateAccount(Arg.Is<CreateCustomerEvent>(customerEvent));
							customerService.Received(1).CreateCustomerRelationShip(Arg.Any<CreateCustomerRelationshipEvent>());
							break;
						}
					case CustomerType.Customer:
					case CustomerType.Corporate:
					case CustomerType.Operations:
						{
							customerService.Received(1).GetCustomer(Arg.Is<Guid>(customerEvent.CustomerUID));
							customerService.Received(1).CreateCustomer(Arg.Is<CreateCustomerEvent>(customerEvent));
							accountService.DidNotReceive().CreateAccount(Arg.Is<CreateCustomerEvent>(customerEvent));
							customerService.DidNotReceive().CreateCustomerRelationShip(Arg.Any<CreateCustomerRelationshipEvent>());
							break;
						}
					case CustomerType.Account:
						{
							accountService.Received(1).GetAccount(Arg.Is<Guid>(customerEvent.CustomerUID));
							accountService.Received(1).CreateAccount(Arg.Is<CreateCustomerEvent>(customerEvent));
							customerService.DidNotReceive().CreateCustomer(Arg.Is<CreateCustomerEvent>(customerEvent));
							customerService.DidNotReceive().CreateCustomerRelationShip(Arg.Any<CreateCustomerRelationshipEvent>());
							break;
						}
					default: break;
				}
			}
		}

		[Theory]
		[InlineData(true, "1", 1)]
		[InlineData(false, "1", 0)]
		[InlineData(false, "Dealer", 0)]
		[InlineData(true, "Dealer", 1)]
		[InlineData(false, "DEALER", 0)]
		[InlineData(true, "DEALER", 1)]
		[InlineData(false, "dealer", 0)]
		[InlineData(true, "dealer", 1)]
		public void CreateCutsomer_ValidDealer_CreateCustomerRelationshipSuccess(
			bool createRelationshipSuccess, string customerType, int createRelationCalls)
		{
			//Arrange
			var customerEvent = new CreateCustomerEvent()
			{
				CustomerType = customerType,
				CustomerUID = Guid.NewGuid(),
				CustomerName = "CUS01",
				NetworkCustomerCode = "NCC01",
				DealerNetwork = "DN01",
				NetworkDealerCode = "NDC01",
				PrimaryContactEmail = "asset01@gma.com",
				FirstName = "FN",
				LastName = "LN",
				BSSID = "1173",
				ActionUTC = DateTime.UtcNow,
				DealerAccountCode = "DAC01"
			};
			accountService.GetAccount(Arg.Any<Guid>()).Returns(x => null);
			customerService.GetCustomer(Arg.Any<Guid>()).Returns(x => null);
			customerService.CreateCustomer(customerEvent).Returns(true);

			if (createRelationshipSuccess)
			{
				customerService.GetCustomerRelationships(customerEvent.CustomerUID, customerEvent.CustomerUID)
				.Returns(new List<DbCustomerRelationshipNode> { });
				customerService.CreateCustomerRelationShip(Arg.Any<CreateCustomerRelationshipEvent>())
					.Returns(true);
			}
			else
			{
				customerService.GetCustomerRelationships(customerEvent.CustomerUID, customerEvent.CustomerUID)
				.Returns(x => throw new Exception());
			}

			//Act
			var response = customerController.CreateCustomer(customerEvent);

			//Assert
			Assert.NotNull(response);
			var responseData = (StatusCodeResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(200, responseData.StatusCode);

			accountService.DidNotReceive().GetAccount(Arg.Is<Guid>(customerEvent.CustomerUID));
			accountService.DidNotReceive().CreateAccount(Arg.Is<CreateCustomerEvent>(customerEvent));
			customerService.Received(1).GetCustomer(Arg.Is<Guid>(customerEvent.CustomerUID));
			customerService.Received(1).CreateCustomer(Arg.Is<CreateCustomerEvent>(customerEvent));

			customerService.Received(1)
				.GetCustomerRelationships(customerEvent.CustomerUID, customerEvent.CustomerUID);
			customerService.Received(createRelationCalls).CreateCustomerRelationShip(
				Arg.Any<CreateCustomerRelationshipEvent>());
		}

		[Fact]
		public void CreateCustomer_ThrowsException_InternalServerError()
		{
			//Arrange
			var customerEvent = new CreateCustomerEvent() { CustomerType = "Account" };
			accountService.GetAccount(Arg.Any<Guid>()).Returns(x => null);
			accountService.CreateAccount(customerEvent).Returns(x => throw new Exception());

			//Act
			var response = customerController.CreateCustomer(customerEvent);

			//Assert
			Assert.NotNull(response);

			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(500, responseData.StatusCode);
			Assert.Contains("System.Exception", responseData.Value.ToString());
		}
		#endregion

		#region Update Customer
		[Theory]
		[MemberData(nameof(CustomerTestDataGenerator.GetUpdateCustomerTestData),
			MemberType = typeof(CustomerTestDataGenerator))]
		public void UpdateCustomer_GivenInput_ExpectedStatusCodeWithMessage(UpdateCustomerEvent customerEvent,
			CustomerType type, bool isAccountExists, bool isCustomerExists, bool updateAccountStatus,
			 bool updateCustomerStatus, int? statusCode, string message)
		{
			//Arrange
			DbAccount accountData = isAccountExists ? new DbAccount() : null;
			DbCustomer customerData = isCustomerExists ? new DbCustomer() : null;

			switch (type)
			{
				case CustomerType.Account:
					{
						accountService.GetAccount(customerEvent.CustomerUID).Returns(accountData);
						accountService.UpdateAccount(customerEvent, accountData).Returns(updateAccountStatus);
						break;
					}
				case CustomerType.Customer:
					{
						customerService.GetCustomer(customerEvent.CustomerUID).Returns(customerData);
						customerService.UpdateCustomer(customerEvent, customerData).Returns(updateCustomerStatus);
						break;
					}
				default: break;
			}

			//Act
			var response = customerController.UpdateCustomer(customerEvent);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			if (statusCode == (int)HttpStatusCode.OK)
			{
				Assert.Equal(statusCode, responseData.StatusCode);
			}
			else
			{
				Assert.Equal(statusCode, responseData.StatusCode);
				Assert.Equal(message, responseData.Value);
			}
		}

		[Fact]
		public void UpdateCustomer_ThrowsException_InternalServerError()
		{
			//Arrange
			var customerEvent = new UpdateCustomerEvent()
			{
				CustomerUID = Guid.NewGuid(),
				CustomerName = "ABC01"
			};
			accountService.GetAccount(Arg.Any<Guid>()).Returns(x => throw new Exception());

			//Act
			var response = customerController.UpdateCustomer(customerEvent);

			//Assert
			Assert.NotNull(response);

			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(500, responseData.StatusCode);
			Assert.Contains("System.Exception", responseData.Value.ToString());
		}
		#endregion

		#region Delete Customer
		[Theory]
		[MemberData(nameof(CustomerTestDataGenerator.GetDeleteCustomerTestData),
			MemberType = typeof(CustomerTestDataGenerator))]
		public void DeleteCustomer_GivenInput_ExpectedStatusCodeWithMessage(Guid customerUID, DateTime actionUTC,
			CustomerType type, bool isAccountExists, bool isCustomerExists, bool deleteAccountStatus,
			 bool deleteCustomerStatus, int? statusCode, string message)
		{
			//Arrange
			DbAccount accountData = isAccountExists ? new DbAccount() : null;
			DbCustomer customerData = isCustomerExists ? new DbCustomer() : null;

			switch (type)
			{
				case CustomerType.Account:
					{
						accountService.GetAccount(customerUID).Returns(accountData);
						accountService.DeleteAccount(customerUID, actionUTC, accountData)
							.Returns(deleteAccountStatus);
						break;
					}
				case CustomerType.Customer:
					{
						customerService.GetCustomer(customerUID).Returns(customerData);
						customerService.DeleteCustomer(customerUID, actionUTC).Returns(deleteCustomerStatus);
						break;
					}
				default: break;
			}

			//Act
			var response = customerController.DeleteCustomer(customerUID, actionUTC);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			if (statusCode == (int)HttpStatusCode.OK)
			{
				Assert.Equal(statusCode, responseData.StatusCode);
			}
			else
			{
				Assert.Equal(statusCode, responseData.StatusCode);
				Assert.Equal(message, responseData.Value);
			}
		}

		[Fact]
		public void DeleteCustomer_ThrowsException_InternalServerError()
		{
			//Arrange
			accountService.GetAccount(Arg.Any<Guid>()).Returns(x => throw new Exception());

			//Act
			var response = customerController.DeleteCustomer(Guid.NewGuid(), DateTime.UtcNow);

			//Assert
			Assert.NotNull(response);

			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(500, responseData.StatusCode);
			Assert.Contains("System.Exception", responseData.Value.ToString());
		}
		#endregion

		#region Associate Customer Asset
		[Theory]
		[MemberData(nameof(CustomerTestDataGenerator.GetAssociateAssetCustomerTestData),
			MemberType = typeof(CustomerTestDataGenerator))]
		public void AssociateCustomerAsset_GivenInput_ExpectedStatusCodeWithMessage(
			AssociateCustomerAssetEvent customerAsset, bool isCustomerExists, bool isCustomerAssetExists,
			bool createCustomerAssetStatus, int statusCode, string message)
		{
			//Arrange
			DbCustomer customerData = isCustomerExists ? new DbCustomer() : null;
			ACDbModel assetCustomerData = isCustomerAssetExists ? new ACDbModel() : null;

			customerService.GetCustomer(customerAsset.CustomerUID).Returns(customerData);
			customerAssetService.GetAssetCustomerByRelationType(customerAsset.CustomerUID,
				customerAsset.AssetUID, Arg.Any<int>()).Returns(assetCustomerData);

			customerAssetService.AssociateCustomerAsset(customerAsset)
				.Returns(createCustomerAssetStatus);

			//Act
			var response = customerController.AssociateCustomerAsset(customerAsset);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(statusCode, responseData.StatusCode);

			if (statusCode != (int)HttpStatusCode.OK)
			{
				Assert.Equal(message, responseData.Value.ToString());
			}

		}

		[Fact]
		public void AssociateCustomerAsset_ThrowsException_InternalServerError()
		{
			//Arrange
			var customerAssetEvent = new AssociateCustomerAssetEvent()
			{
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				RelationType = "Owner"
			};
			customerService.GetCustomer(customerAssetEvent.CustomerUID).Returns(x => new DbCustomer());
			customerAssetService.GetAssetCustomerByRelationType(customerAssetEvent.CustomerUID,
				customerAssetEvent.AssetUID, Arg.Any<int>())
				.Returns(x => throw new Exception());

			//Act
			var response = customerController.AssociateCustomerAsset(customerAssetEvent);

			//Assert
			Assert.NotNull(response);

			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(500, responseData.StatusCode);
			Assert.Contains("System.Exception", responseData.Value.ToString());
		}
		#endregion

		#region Dissociate Customer Asset
		[Theory]
		[MemberData(nameof(CustomerTestDataGenerator.GetDissociateAssetCustomerTestData),
			MemberType = typeof(CustomerTestDataGenerator))]
		public void DissociateCustomerAsset_GivenInput_ExpectedStatusCodeWithMessage(
			DissociateCustomerAssetEvent customerAsset, bool isCustomerExists, bool isCustomerAssetExists,
			bool removeCustomerAssetStatus, int statusCode, string message)
		{
			//Arrange
			DbCustomer customerData = isCustomerExists ? new DbCustomer() : null;
			ACDbModel assetCustomerData = isCustomerAssetExists ? new ACDbModel() : null;

			customerService.GetCustomer(customerAsset.CustomerUID).Returns(customerData);
			customerAssetService.GetAssetCustomer(customerAsset.CustomerUID,
				customerAsset.AssetUID).Returns(assetCustomerData);

			customerAssetService.DissociateCustomerAsset(customerAsset)
				.Returns(removeCustomerAssetStatus);

			//Act
			var response = customerController.DissociateCustomerAsset(customerAsset);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(statusCode, responseData.StatusCode);

			if (statusCode != (int)HttpStatusCode.OK)
			{
				Assert.Equal(message, responseData.Value.ToString());
			}
		}

		[Fact]
		public void DissociateCustomerAsset_ThrowsException_InternalServerError()
		{
			//Arrange
			var customerAssetEvent = new DissociateCustomerAssetEvent()
			{
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid()
			};
			customerService.GetCustomer(customerAssetEvent.CustomerUID).Returns(x => new DbCustomer());
			customerAssetService.GetAssetCustomer(customerAssetEvent.CustomerUID,
				customerAssetEvent.AssetUID).Returns(x => throw new Exception());

			//Act
			var response = customerController.DissociateCustomerAsset(customerAssetEvent);

			//Assert
			Assert.NotNull(response);

			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(500, responseData.StatusCode);
			Assert.Contains("System.Exception", responseData.Value.ToString());
		}
		#endregion

		#region Create User Customer Relationship
		[Theory]
		[MemberData(nameof(CustomerTestDataGenerator.GetCreateUserCutsomerRelationshipTestData),
			MemberType = typeof(CustomerTestDataGenerator))]
		public void CreateUserCustomerRelationship_GivenInput_ExpectedStatusCodeWithMessage(
			CreateUserCustomerRelationshipEvent userCustomer, bool isCustomerExists, bool isCustomerUserExists,
			bool createUserCustomerStatus, int statusCode, string message)
		{
			//Arrange
			DbCustomer customerData = isCustomerExists ? new DbCustomer() : null;
			DbUserCustomer userCustomerData = isCustomerUserExists ? new DbUserCustomer() : null;

			customerService.GetCustomer(userCustomer.CustomerUID).Returns(customerData);
			userCustomerService.GetCustomerUser(userCustomer.CustomerUID,
				userCustomer.UserUID).Returns(userCustomerData);

			customerService.CreateUserCustomerRelationship(userCustomer)
				.Returns(createUserCustomerStatus);

			//Act
			var response = customerController.CreateUserCustomerRelationship(userCustomer);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(statusCode, responseData.StatusCode);

			if (statusCode != (int)HttpStatusCode.OK)
			{
				Assert.Equal(message, responseData.Value.ToString());
			}
		}

		[Fact]
		public void CreateUserCustomerRelationship_ThrowsException_InternalServerError()
		{
			//Arrange
			var userCustomerEvent = new CreateUserCustomerRelationshipEvent()
			{
				CustomerUID = Guid.NewGuid(),
				UserUID = Guid.NewGuid()
			};
			customerService.GetCustomer(userCustomerEvent.CustomerUID).Returns(x => new DbCustomer());
			userCustomerService.GetCustomerUser(userCustomerEvent.CustomerUID,
				userCustomerEvent.UserUID).Returns(x => throw new Exception());

			//Act
			var response = customerController.CreateUserCustomerRelationship(userCustomerEvent);

			//Assert
			Assert.NotNull(response);

			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(500, responseData.StatusCode);
			Assert.Contains("System.Exception", responseData.Value.ToString());
		}
		#endregion

		#region Update User Customer Relationship
		[Theory]
		[MemberData(nameof(CustomerTestDataGenerator.GetUpdateUserCutsomerRelationshipTestData),
			MemberType = typeof(CustomerTestDataGenerator))]
		public void UpdateUserCustomerRelationship_GivenInput_ExpectedStatusCodeWithMessage(
			UpdateUserCustomerRelationshipEvent userCustomer, bool isCustomerExists, bool isCustomerUserExists,
			bool updateUserCustomerStatus, int statusCode, string message)
		{
			//Arrange
			DbCustomer customerData = isCustomerExists ? new DbCustomer() : null;
			DbUserCustomer userCustomerData = isCustomerUserExists ? new DbUserCustomer() : null;

			customerService.GetCustomer(userCustomer.CustomerUID).Returns(customerData);
			userCustomerService.GetCustomerUser(userCustomer.CustomerUID,
				userCustomer.UserUID).Returns(userCustomerData);

			customerService.UpdateUserCustomerRelationship(userCustomer)
				.Returns(updateUserCustomerStatus);

			//Act
			var response = customerController.UpdateUserCustomerRelationship(userCustomer);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(statusCode, responseData.StatusCode);

			if (statusCode != (int)HttpStatusCode.OK)
			{
				Assert.Equal(message, responseData.Value.ToString());
			}
		}

		[Fact]
		public void UpdateUserCustomerRelationship_ThrowsException_InternalServerError()
		{
			//Arrange
			var userCustomerEvent = new UpdateUserCustomerRelationshipEvent()
			{
				CustomerUID = Guid.NewGuid(),
				UserUID = Guid.NewGuid()
			};
			customerService.GetCustomer(userCustomerEvent.CustomerUID).Returns(x => new DbCustomer());
			userCustomerService.GetCustomerUser(userCustomerEvent.CustomerUID,
				userCustomerEvent.UserUID).Returns(x => throw new Exception());

			//Act
			var response = customerController.UpdateUserCustomerRelationship(userCustomerEvent);

			//Assert
			Assert.NotNull(response);

			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(500, responseData.StatusCode);
			Assert.Contains("System.Exception", responseData.Value.ToString());
		}
		#endregion

		#region Delete User Customer Relationship
		[Theory]
		[MemberData(nameof(CustomerTestDataGenerator.GetDeleteUserCutsomerRelationshipTestData),
			MemberType = typeof(CustomerTestDataGenerator))]
		public void DeleteUserCustomerRelationship_GivenInput_ExpectedStatusCodeWithMessage(
			DeleteUserCustomerRelationshipEvent userCustomer, bool isCustomerUserExists,
			bool deleteUserCustomerStatus, int statusCode, string message)
		{
			//Arrange
			DbUserCustomer userCustomerData = isCustomerUserExists ? new DbUserCustomer() : null;

			userCustomerService.GetCustomerUser(userCustomer.CustomerUID,
				userCustomer.UserUID).Returns(userCustomerData);
			customerService.DeleteUserCustomerRelationship(userCustomer)
				.Returns(deleteUserCustomerStatus);

			//Act
			var response = customerController.DeleteUserCustomerRelationship(userCustomer);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(statusCode, responseData.StatusCode);

			if (statusCode != (int)HttpStatusCode.OK)
			{
				Assert.Equal(message, responseData.Value.ToString());
			}
		}

		[Fact]
		public void DeleteUserCustomerRelationship_ThrowsException_InternalServerError()
		{
			//Arrange
			var userCustomerEvent = new DeleteUserCustomerRelationshipEvent()
			{
				CustomerUID = Guid.NewGuid(),
				UserUID = Guid.NewGuid()
			};
			customerService.GetCustomer(userCustomerEvent.CustomerUID).Returns(x => new DbCustomer());
			userCustomerService.GetCustomerUser(userCustomerEvent.CustomerUID,
				userCustomerEvent.UserUID).Returns(x => throw new Exception());

			//Act
			var response = customerController.DeleteUserCustomerRelationship(userCustomerEvent);

			//Assert
			Assert.NotNull(response);

			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(500, responseData.StatusCode);
			Assert.Contains("System.Exception", responseData.Value.ToString());
		}
		#endregion

		#region Associate Customer User
		[Theory]
		[MemberData(nameof(CustomerTestDataGenerator.GetAssociateCustomerUserTestData),
			MemberType = typeof(CustomerTestDataGenerator))]
		public void AssociateCustomerUser_GivenInput_ExpectedStatusCodeWithMessage(
			AssociateCustomerUserEvent userCustomer, bool isCustomerExists, bool isCustomerUserExists,
			bool associateUserCustomerStatus, int statusCode, string message)
		{
			//Arrange
			DbCustomer customerData = isCustomerExists ? new DbCustomer() : null;
			DbUserCustomer userCustomerData = isCustomerUserExists ? new DbUserCustomer() : null;

			customerService.GetCustomer(userCustomer.CustomerUID).Returns(customerData);
			userCustomerService.GetCustomerUser(userCustomer.CustomerUID,
				userCustomer.UserUID).Returns(userCustomerData);

			userCustomerService.AssociateCustomerUser(userCustomer)
				.Returns(associateUserCustomerStatus);

			//Act
			var response = customerController.AssociateCustomerUser(userCustomer);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(statusCode, responseData.StatusCode);

			if (statusCode != (int)HttpStatusCode.OK)
			{
				Assert.Equal(message, responseData.Value.ToString());
			}
		}

		[Fact]
		public void AssociateCustomerUser_ThrowsException_InternalServerError()
		{
			//Arrange
			var userCustomerEvent = new AssociateCustomerUserEvent()
			{
				CustomerUID = Guid.NewGuid(),
				UserUID = Guid.NewGuid()
			};
			customerService.GetCustomer(userCustomerEvent.CustomerUID).Returns(x => new DbCustomer());
			userCustomerService.GetCustomerUser(userCustomerEvent.CustomerUID,
				userCustomerEvent.UserUID).Returns(x => throw new Exception());

			//Act
			var response = customerController.AssociateCustomerUser(userCustomerEvent);

			//Assert
			Assert.NotNull(response);

			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(500, responseData.StatusCode);
			Assert.Contains("System.Exception", responseData.Value.ToString());
		}
		#endregion

		#region Dissociate Customer User
		[Theory]
		[MemberData(nameof(CustomerTestDataGenerator.GetDissociateCustomerUserTestData),
			MemberType = typeof(CustomerTestDataGenerator))]
		public void DissociateCustomerUser_GivenInput_ExpectedStatusCodeWithMessage(
			DissociateCustomerUserEvent userCustomer, bool isCustomerUserExists,
			bool dissociateUserCustomerStatus, int statusCode, string message)
		{
			//Arrange
			DbUserCustomer userCustomerData = isCustomerUserExists ? new DbUserCustomer() : null;
			userCustomerService.GetCustomerUser(userCustomer.CustomerUID,
				userCustomer.UserUID).Returns(userCustomerData);

			userCustomerService.DissociateCustomerUser(userCustomer)
				.Returns(dissociateUserCustomerStatus);

			//Act
			var response = customerController.DissociateCustomerUser(userCustomer);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(statusCode, responseData.StatusCode);

			if (statusCode != (int)HttpStatusCode.OK)
			{
				Assert.Equal(message, responseData.Value.ToString());
			}
		}

		[Fact]
		public void DissociateCustomerUser_ThrowsException_InternalServerError()
		{
			//Arrange
			var userCustomerEvent = new DissociateCustomerUserEvent()
			{
				CustomerUID = Guid.NewGuid(),
				UserUID = Guid.NewGuid()
			};
			userCustomerService.GetCustomerUser(userCustomerEvent.CustomerUID,
				userCustomerEvent.UserUID).Returns(x => throw new Exception());

			//Act
			var response = customerController.DissociateCustomerUser(userCustomerEvent);

			//Assert
			Assert.NotNull(response);

			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(500, responseData.StatusCode);
			Assert.Contains("System.Exception", responseData.Value.ToString());
		}
		#endregion

		#region BulkDissociate Customer User
		[Theory]
		[MemberData(nameof(CustomerTestDataGenerator.GetBulkDissociateCustomerUserTestData),
			MemberType = typeof(CustomerTestDataGenerator))]
		public void BulkDissociateCustomerUser_GivenInput_ExpectedStatusCodeWithMessage(
			BulkDissociateCustomerUserEvent userCustomer, bool dissociateUserCustomerStatus,
			int statusCode, string message)
		{
			//Arrange
			userCustomerService.BulkDissociateCustomerUser(
				userCustomer.CustomerUID, userCustomer.UserUID, userCustomer.ActionUTC)
				.Returns(dissociateUserCustomerStatus);

			//Act
			var response = customerController.BulkCustomerUserDissociation(userCustomer);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(statusCode, responseData.StatusCode);

			if (statusCode != (int)HttpStatusCode.OK)
			{
				Assert.Equal(message, responseData.Value.ToString());
			}
		}

		[Fact]
		public void BulkDissociateCustomerUser_ThrowsException_InternalServerError()
		{
			//Arrange
			var userCustomerEvent = new BulkDissociateCustomerUserEvent()
			{
				CustomerUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow
			};
			userCustomerService.BulkDissociateCustomerUser(userCustomerEvent.CustomerUID,
				userCustomerEvent.UserUID, userCustomerEvent.ActionUTC)
				.Returns(x => throw new Exception());

			//Act
			var response = customerController.BulkCustomerUserDissociation(userCustomerEvent);

			//Assert
			Assert.NotNull(response);

			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(500, responseData.StatusCode);
			Assert.Contains("System.Exception", responseData.Value.ToString());
		}
		#endregion

		#region Create Customer Relationship
		[Theory]
		[MemberData(nameof(CustomerTestDataGenerator.GetCreateCustomerRelationshipTestData),
			MemberType = typeof(CustomerTestDataGenerator))]
		public void CreateCustomerRelationship_GivenInput_ExpectedStatusCodeWithMessage(
			CreateCustomerRelationshipEvent relationshipEvent, bool hasAccountUID, bool accountExists,
			bool createAccountRelationStatus, bool isParentChildSame, bool hasRelationships,
			bool createCustomerRelationStatus, int statusCode, string message)
		{
			//Arrange
			if (hasAccountUID)
			{
				DbAccount accountData = accountExists ? new DbAccount() : null;
				accountService.GetAccount((Guid)relationshipEvent.AccountCustomerUID)
					.Returns(accountData);
				accountService.CreateAccountCustomerRelationShip(relationshipEvent, accountData)
					.Returns(createAccountRelationStatus);
			}
			if (!isParentChildSame)
			{
				List<DbCustomerRelationshipNode> relationshipNodes = hasRelationships
					? new List<DbCustomerRelationshipNode>() { new DbCustomerRelationshipNode() }
					: new List<DbCustomerRelationshipNode>();
				customerService.GetCustomerRelationships(Arg.Any<Guid>(), Arg.Any<Guid>())
					.Returns(relationshipNodes);
				customerService.CreateCustomerRelationShip(relationshipEvent)
					.Returns(createCustomerRelationStatus);
			}

			//Act
			var response = customerController.CreateCustomerRelationship(relationshipEvent);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(statusCode, responseData.StatusCode);
			if (statusCode != (int)HttpStatusCode.OK
				&& statusCode != (int)HttpStatusCode.InternalServerError)
			{
				Assert.Equal(message, responseData.Value);
			}
		}

		[Fact]
		public void CreateCustomerRelationship_AccountThrowsException_InternalServerError()
		{
			//Arrange
			var relationEvent = new CreateCustomerRelationshipEvent
			{
				ChildCustomerUID = Guid.NewGuid(),
				AccountCustomerUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow
			};
			accountService.GetAccount((Guid)relationEvent.AccountCustomerUID)
				.Returns(x => throw new Exception());

			//Act
			var response = customerController.CreateCustomerRelationship(relationEvent);

			//Assert
			Assert.NotNull(response);

			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(500, responseData.StatusCode);
			Assert.Contains("System.Exception", responseData.Value.ToString());
			accountService.DidNotReceive().CreateAccountCustomerRelationShip(
				Arg.Any<CreateCustomerRelationshipEvent>(), Arg.Any<DbAccount>());
			customerService.DidNotReceive()
				.GetCustomerRelationships(Arg.Any<Guid>(), Arg.Any<Guid>());
			customerService.DidNotReceive()
				.CreateCustomerRelationShip(Arg.Any<CreateCustomerRelationshipEvent>());
		}
		#endregion

		#region Delete customer Relationship
		[Theory]
		[MemberData(nameof(CustomerTestDataGenerator.GetDeleteCustomerRelationshipTestData),
			MemberType = typeof(CustomerTestDataGenerator))]
		public void DeleteCustomerRelationship_GivenInput_ExpectedStatusCodeWithMessage(
			Guid parentUid, Guid childUid, bool isParentChildSame, bool deleteCustomerRelationStatus,
			bool hasAccountUid, Guid accountUid, string deleteType, bool accountExists,
			bool deleteAccountCustomerRelationshipStatus, bool createAccountCustomerRelationshipStatus,
			bool isCustDealerRelExists, int statusCode, string message)
		{
			//Arrange
			var actionUtc = DateTime.UtcNow;
			if (!isParentChildSame)
			{
				customerService.DeleteCustomerRelationShip(Arg.Is(parentUid), Arg.Is(childUid),
					hasAccountUid ? Arg.Is(accountUid) : Arg.Is((Guid?)null), Arg.Is(actionUtc))
					.Returns(deleteCustomerRelationStatus);
			}
			if (hasAccountUid)
			{
				DbAccount accountData = accountExists ? new DbAccount() : null;
				accountService.GetAccount(Arg.Any<Guid>()).Returns(accountData);
				if (isParentChildSame)
				{
					accountService.DeleteAccountCustomerRelationShip(Arg.Is(parentUid), Arg.Is(childUid)
					, Arg.Is(accountData), Arg.Is(actionUtc))
					.Returns(deleteAccountCustomerRelationshipStatus);
				}
				else
				{
					accountService.CreateAccountCustomerRelationShip(Arg.Is(parentUid), Arg.Is(childUid),
						Arg.Is(accountData), Arg.Is(actionUtc), Arg.Is(deleteType))
						.Returns(createAccountCustomerRelationshipStatus);
				}
			}

			customerService.IsCustomerRelationShipAlreadyExists(
				Arg.Is(parentUid.ToString()), Arg.Is(childUid.ToString()))
				.Returns(isCustDealerRelExists);

			//Act
			var response = customerController.DeleteCustomerRelationship(
				parentUid, childUid, actionUtc, deleteType, accountUid);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(statusCode, responseData.StatusCode);
			if (statusCode != (int)HttpStatusCode.OK)
			{
				Assert.Equal(message, responseData.Value);
			}
		}

		[Fact]
		public void DeleteCustomerRelationship_AccountThrowsException_InternalServerError()
		{
			//Arrange
			var parentUid = Guid.NewGuid();
			var childUid = Guid.NewGuid();
			var accountUid = Guid.NewGuid();
			var actionUtc = DateTime.UtcNow;
			var type = "Remove";
			customerService.DeleteCustomerRelationShip(Arg.Is(parentUid), Arg.Is(childUid),
				Arg.Is(accountUid), Arg.Is(actionUtc))
				.Returns(x => throw new Exception());
			customerService.IsCustomerRelationShipAlreadyExists(
				Arg.Is(parentUid.ToString()), Arg.Is(childUid.ToString()))
				.Returns(true);

			//Act
			var response = customerController.DeleteCustomerRelationship(
				parentUid, childUid, actionUtc, type, accountUid);

			//Assert
			Assert.NotNull(response);

			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(500, responseData.StatusCode);
			Assert.Contains("System.Exception", responseData.Value.ToString());
			accountService.DidNotReceive().GetAccount(Arg.Is(accountUid));
			accountService.DidNotReceive()
				.CreateAccountCustomerRelationShip(Arg.Any<Guid>(), Arg.Any<Guid>(),
				Arg.Any<DbAccount>(), Arg.Any<DateTime>(), Arg.Any<string>());
			accountService.DidNotReceive()
				.DeleteAccountCustomerRelationShip(Arg.Any<Guid>(), Arg.Any<Guid>(),
				Arg.Any<DbAccount>(), Arg.Any<DateTime>());
			customerService.Received(1).DeleteCustomerRelationShip(Arg.Is(parentUid), Arg.Is(childUid),
				Arg.Is(accountUid), Arg.Is(actionUtc));
			customerService.Received(1).IsCustomerRelationShipAlreadyExists(Arg.Is(parentUid.ToString()),
				Arg.Is(childUid.ToString()));
		}
		#endregion

		#region Get Associated Customers
		[Fact]
		public void GetAssociatedCustomers_ValidUser_ReturnsCustomers()
		{
			//Arrange
			var customer = new DbCustomer
			{
				CustomerUID = Guid.NewGuid(),
				CustomerName = "CUS01",
				fk_CustomerTypeID = 0
			};

			customerService.GetAssociatedCustomersbyUserUid(Arg.Any<Guid>())
				.Returns(new List<DbCustomer> { customer });

			//Act
			var response = customerController.GetAssociatedCustomers(JWTAssertion);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(200, responseData.StatusCode);

			var customerResponse = responseData.Value;
			Assert.IsType<CustomerListSuccessResponse>(customerResponse);
			Assert.Equal(HttpStatusCode.OK, customerResponse.Status);
			//Assert.Equal("GetAssociatedCustomers_Success", customerResponse.Metadata.Message);
			Assert.Equal("Customers retrieved successfully", customerResponse.Metadata.Message);

			Assert.NotEmpty(customerResponse.Customers);
			Assert.Equal(customer.CustomerUID, customerResponse.Customers[0].CustomerUID);
			Assert.Equal(customer.CustomerName, customerResponse.Customers[0].CustomerName);
			Assert.IsType<APIEnums.CustomerType>(customerResponse.Customers[0].CustomerType);
			Assert.Equal(APIEnums.CustomerType.Customer, customerResponse.Customers[0].CustomerType);
		}

		[Fact]
		public void GetAssociatedCustomers_InvalidUser_ReturnsBadRequest()
		{
			//Act
			var response = customerController.GetAssociatedCustomers("");

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(400, responseData.StatusCode);
		}

		[Fact]
		public void GetAssociatedCustomers_InvalidUser_ReturnsInternalServerError()
		{
			//Act
			var response = customerController.GetAssociatedCustomers("akgiaeycgudag");

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(500, responseData.StatusCode);
		}
		#endregion

		#region Get Associated Dealers Customer
		[Fact]
		public void GetAssociatedDealersCustomer_ValidUser_ReturnsCustomers()
		{
			//Arrange
			var dealer = new DbCustomer
			{
				CustomerUID = Guid.NewGuid(),
				CustomerName = "DEA01",
				fk_CustomerTypeID = 1
			};
			var dealerCustomer = new DbCustomer
			{
				CustomerUID = Guid.NewGuid(),
				CustomerName = "CUS01",
				fk_CustomerTypeID = 0
			};

			customerService.GetAssociatedCustomersbyUserUid(Arg.Any<Guid>())
				.Returns(new List<DbCustomer> { dealer });
			customerService.GetAssociatedCustomersForDealer(dealer.CustomerUID)
				.Returns(new List<DbCustomer> { dealerCustomer });

			//Act
			var response = customerController.GetAssociatedDealersCustomer(JWTAssertion);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(200, responseData.StatusCode);

			var customerResponse = responseData.Value;
			Assert.IsType<List<CustomerClientModel>>(customerResponse);
			Assert.NotEmpty(customerResponse);
			Assert.Equal(dealerCustomer.CustomerUID, customerResponse[0].CustomerUID);
			Assert.Equal(dealerCustomer.CustomerName, customerResponse[0].CustomerName);
			Assert.Equal("CUSTOMER", customerResponse[0].CustomerType);
		}

		[Fact]
		public void GetAssociatedDealersCustomer_ValidUser_ReturnsEmptyCustomers()
		{
			//Arrange
			var dealer = new DbCustomer
			{
				CustomerUID = Guid.NewGuid(),
				CustomerName = "DEA01",
				fk_CustomerTypeID = 1
			};

			customerService.GetAssociatedCustomersbyUserUid(Arg.Any<Guid>())
				.Returns(new List<DbCustomer> { dealer });
			customerService.GetAssociatedCustomersForDealer(dealer.CustomerUID)
				.Returns(new List<DbCustomer> { });

			//Act
			var response = customerController.GetAssociatedDealersCustomer(JWTAssertion);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(200, responseData.StatusCode);

			var customerResponse = responseData.Value;
			Assert.IsType<List<CustomerClientModel>>(customerResponse);
			Assert.Empty(customerResponse);
		}

		[Fact]
		public void GetAssociatedDealersCustomer_InvalidUser_ReturnsBadRequest()
		{
			//Act
			var response = customerController.GetAssociatedCustomers("");

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(400, responseData.StatusCode);
			//TODO : assert the response Messages.JWTTokenEmpty
		}

		[Fact]
		public void GetAssociatedDealersCustomer_InvalidUser_ReturnsInternalServerError()
		{
			//Arrange
			customerService.GetAssociatedCustomersbyUserUid(Arg.Any<Guid>())
				.Returns(x => throw new Exception());

			//Act
			var response = customerController.GetAssociatedCustomers(JWTAssertion);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(500, responseData.StatusCode);
		}
		#endregion

		#region Get Customer Data For CustomerUIDs
		[Fact]
		public void GetCustomerDataForCustomerUIDs_ValidUIDs_ReturnsCustomers()
		{
			//Arrange
			var customer = new DbCustomer
			{
				CustomerUID = Guid.NewGuid(),
				CustomerName = "Cus01",
				fk_CustomerTypeID = 0
			};
			var customerUids = new Guid[] { customer.CustomerUID, Guid.NewGuid() };
			customerService.GetCustomerByCustomerGuids(customerUids)
				.Returns(new List<DbCustomer> { customer });

			//Act
			var response = customerController.GetCustomerDataForCustomerUIDs(customerUids);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(200, responseData.StatusCode);

			var customerResponse = responseData.Value;
			Assert.IsType<List<CustomerClientModel>>(customerResponse);
			Assert.NotEmpty(customerResponse);
			Assert.Single(customerResponse);
			Assert.Equal(customer.CustomerUID, customerResponse[0].CustomerUID);
			Assert.Equal(customer.CustomerName, customerResponse[0].CustomerName);
			Assert.Equal("Customer", customerResponse[0].CustomerType);
		}

		[Fact]
		public void GetCustomerDataForCustomerUIDs_GetCustomerException_ReturnsBadRequest()
		{
			//Arrange
			var customerUids = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
			customerService.GetCustomerByCustomerGuids(customerUids)
				.Returns(x => throw new Exception());

			//Act
			var response = customerController.GetCustomerDataForCustomerUIDs(customerUids);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(400, responseData.StatusCode);
		}
		#endregion

		#region Get Customer Search
		[Fact]
		public void GetCustomersForCustomerNames_ValidCustomer_ReturnsCustomerResponse()
		{
			//Arrange
			var filterText = "CAT";
			var customer = Tuple.Create(new DbCustomer
			{
				CustomerUID = Guid.NewGuid(),
				CustomerName = "Cus01",
				fk_CustomerTypeID = 0,
				NetworkDealerCode = "NDC01"
			}, new DbAccount { NetworkCustomerCode = "NCC01" });
			customerService.GetCustomersByNameSearch(Arg.Is(filterText), Arg.Is(20))
				.Returns(new List<Tuple<DbCustomer, DbAccount>> { customer });

			//Act
			var response = customerController.GetCustomersForCustomerNames(filterText);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(200, responseData.StatusCode);

			var customerResponse = responseData.Value;
			Assert.IsType<List<CustomerResponse>>(customerResponse);
			Assert.NotEmpty(customerResponse);
			Assert.Single(customerResponse);
			Assert.Equal(customer.Item1.CustomerUID, customerResponse[0].CustomerUID);
			Assert.Equal(customer.Item1.CustomerName, customerResponse[0].CustomerName);
			Assert.Equal("Customer", customerResponse[0].CustomerType);
			Assert.Equal(customer.Item1.NetworkDealerCode, customerResponse[0].NetworkDealerCode);
			Assert.Equal(customer.Item2.NetworkCustomerCode, customerResponse[0].NetworkCustomerCode);
		}

		[Fact]
		public void GetCustomersForCustomerNames_GetCustomersByNameException_ReturnsBadRequest()
		{
			//Arrange
			var filterText = "CAT";
			customerService.GetCustomersByNameSearch(Arg.Is(filterText), Arg.Is(20))
				.Returns(x => throw new Exception());

			//Act
			var response = customerController.GetCustomersForCustomerNames(filterText);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(400, responseData.StatusCode);
		}
		#endregion

		#region Get Asset Customers
		[Theory]
		[MemberData(nameof(CustomerTestDataGenerator.GetAssetCustomerDetailTestData),
			MemberType = typeof(CustomerTestDataGenerator))]
		public void GetAssetCustomersByAssetUID_ValidAsset_ReturnsAssetCustomerResponse(
			AssetCustomerDetail assetCustomerDetail, string customerType, string parentCustomerType)
		{
			//Arrange
			var assetUid = Guid.NewGuid();
			customerService.GetAssetCustomerByAssetGuid(Arg.Is(assetUid))
				.Returns(new List<AssetCustomerDetail> { assetCustomerDetail });

			//Act
			var response = customerController.GetAssetCustomersByAssetUID(assetUid);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(200, responseData.StatusCode);

			var customerResponse = responseData.Value;
			Assert.IsAssignableFrom<IEnumerable<AssetCustomerResponse>>(customerResponse);
			Assert.NotEmpty(customerResponse);
			Assert.Single(customerResponse);

			foreach (AssetCustomerResponse item in customerResponse)
			{
				Assert.Equal(assetCustomerDetail.CustomerUID, item.CustomerUID);
				Assert.Equal(assetCustomerDetail.CustomerName, item.CustomerName);
				Assert.Equal(customerType, item.CustomerType);
				Assert.Equal(assetCustomerDetail.ParentName, item.ParentName);
				Assert.Equal(assetCustomerDetail.ParentCustomerUID, item.ParentCustomerUID);
				Assert.Equal(parentCustomerType, item.ParentCustomerType);
			}
		}

		[Fact]
		public void GetAssetCustomersByAssetUID_GetCustomerAssetException_ReturnsBadRequestResponse()
		{
			//Arrange
			var assetUid = Guid.NewGuid();
			customerService.GetAssetCustomerByAssetGuid(Arg.Is(assetUid))
				.Returns(x => throw new Exception());

			//Act
			var response = customerController.GetAssetCustomersByAssetUID(assetUid);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(400, responseData.StatusCode);
		}
		#endregion

		#region Get Account Hierarchy
		[Fact]
		public void GetAccountHierarchy_ValidUser_ReturnsHierarchyResponse()
		{
			//Arrange
			var customerUid = Guid.NewGuid().ToString();
			var userUid = Guid.NewGuid().ToString();
			var hierarchyInfo = new CustomerHierarchyInfo
			{
				UserUID = userUid,
				Customers = new List<CustomerHierarchyNode>
				{
					new CustomerHierarchyNode
					{
						CustomerUID = customerUid,
						CustomerCode = "1234CU01",
						DisplayName = "CUSTOMER01 (1234CU01)",
						Name = "CUSTOMER01",
						CustomerType = "Customer"
					}
				}
			};
			customerService.GetHierarchyInformationForUser(Arg.Any<string>(), Arg.Is(customerUid), Arg.Is(true))
				.Returns(hierarchyInfo);

			//Act
			var response = customerController.GetAccountHierarchy(JWTAssertion, customerUid, true);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(200, responseData.StatusCode);

			var customerResponse = responseData.Value;
			Assert.IsType<CustomerHierarchyInfo>(customerResponse);
			Assert.Equal(hierarchyInfo.UserUID, customerResponse.UserUID);

			var customers = customerResponse.Customers;
			Assert.NotEmpty(customerResponse.Customers);
			Assert.Single(customerResponse.Customers);
			Assert.IsType<List<CustomerHierarchyNode>>(customers);

			Assert.Equal(hierarchyInfo.Customers[0].CustomerUID, customers[0].CustomerUID);
			Assert.Equal(hierarchyInfo.Customers[0].CustomerType, customers[0].CustomerType);
			Assert.Equal(hierarchyInfo.Customers[0].Name, customers[0].Name);
			Assert.Equal(hierarchyInfo.Customers[0].DisplayName, customers[0].DisplayName);
			Assert.Equal(hierarchyInfo.Customers[0].CustomerCode, customers[0].CustomerCode);
		}

		[Fact]
		public void GetAccountHierarchy_InValidUserUid_ReturnsBadRequestResponse()
		{
			//Act
			var response = customerController.GetAccountHierarchy(null);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(400, responseData.StatusCode);
			Assert.Equal("JWT token is empty", responseData.Value);
		}

		[Fact]
		public void GetAccountHierarchy_HierarchyInformationThrowsException_ReturnsInternalServerError()
		{
			//Arrange
			var userUid = Guid.NewGuid().ToString();
			customerService.GetHierarchyInformationForUser(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>())
				.Returns(x => throw new Exception());

			//Act
			var response = customerController.GetAccountHierarchy(JWTAssertion);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(500, responseData.StatusCode);
			Assert.Contains("System.Exception", responseData.Value.ToString());
		}
		#endregion

		#region GetAccountHierarchy For TargetUser
		[Fact]
		public void GetAccountHierarchyForTargetUser_ValidUser_ReturnsHierarchyResponse()
		{
			//Arrange
			var customerUid = Guid.NewGuid().ToString();
			var userUid = Guid.NewGuid().ToString();
			var hierarchyInfo = new CustomerHierarchyInfo
			{
				UserUID = userUid,
				Customers = new List<CustomerHierarchyNode>
				{
					new CustomerHierarchyNode
					{
						CustomerUID = customerUid,
						CustomerCode = "1234CU01",
						DisplayName = "CUSTOMER01 (1234CU01)",
						Name = "CUSTOMER01",
						CustomerType = "Customer"
					}
				}
			};
			customerService.GetHierarchyInformationForUser(Arg.Any<string>())
				.Returns(hierarchyInfo);

			//Act
			var response = customerController.GetAccountHierarchyForTargetUser(userUid);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(200, responseData.StatusCode);

			var customerResponse = responseData.Value;
			Assert.IsType<CustomerHierarchyInfo>(customerResponse);
			Assert.Equal(hierarchyInfo.UserUID, customerResponse.UserUID);

			var customers = customerResponse.Customers;
			Assert.NotEmpty(customerResponse.Customers);
			Assert.Single(customerResponse.Customers);
			Assert.IsType<List<CustomerHierarchyNode>>(customers);

			Assert.Equal(hierarchyInfo.Customers[0].CustomerUID, customers[0].CustomerUID);
			Assert.Equal(hierarchyInfo.Customers[0].CustomerType, customers[0].CustomerType);
			Assert.Equal(hierarchyInfo.Customers[0].Name, customers[0].Name);
			Assert.Equal(hierarchyInfo.Customers[0].DisplayName, customers[0].DisplayName);
			Assert.Equal(hierarchyInfo.Customers[0].CustomerCode, customers[0].CustomerCode);
		}

		[Fact]
		public void GetAccountHierarchyForTargetUser_InValidUserUid_ReturnsBadRequestResponse()
		{
			//Act
			var userUid = "kskhaiuh-kek";
			var response = customerController.GetAccountHierarchyForTargetUser(userUid);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(400, responseData.StatusCode);
			Assert.Equal(Messages.InvalidUserUid, responseData.Value);
		}

		[Fact]
		public void GetAccountHierarchyForTargetUser_HierarchyInformationThrowsException_ReturnsInternalServerError()
		{
			//Arrange
			var userUid = Guid.NewGuid().ToString();
			customerService.GetHierarchyInformationForUser(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>())
				.Returns(x => throw new Exception());

			//Act
			var response = customerController.GetAccountHierarchyForTargetUser(userUid);

			//Assert
			Assert.NotNull(response);
			dynamic responseData = (IStatusCodeActionResult)response;
			Assert.NotNull(responseData);
			Assert.Equal(500, responseData.StatusCode);
			Assert.Contains("System.Exception", responseData.Value.ToString());
		}
		#endregion

		//TODO : Validate all objects in single memberdata
		//[Theory]
		//[MemberData(nameof(CustomerTestDataGenerator.GetEventsForRequiredFieldValidation),
		//	MemberType = typeof(CustomerTestDataGenerator))]
		//public void CustomerEventIsInValid<T>(T customerEvent, string messages, int messageCount)
		//{
		//	//Arrange
		//	var validationResults = new List<ValidationResult>();
		//	List<string> expectedMessages = messages.Split('|').ToList();

		//	//Act
		//	var result = Validator.TryValidateObject(customerEvent,
		//		new ValidationContext(customerEvent, null, null), validationResults, true);

		//	//Assert
		//	Assert.False(result);
		//	Assert.NotEmpty(validationResults);
		//	Assert.Equal(messageCount, validationResults.Count);
		//	validationResults?.ForEach((m) =>
		//	{
		//		Assert.Contains(m.ErrorMessage, messages);
		//	});
		//}
	}
}
