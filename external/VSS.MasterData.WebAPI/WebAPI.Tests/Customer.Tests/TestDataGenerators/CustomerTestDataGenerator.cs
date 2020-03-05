using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.Customer.KafkaModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Utilities;
using static VSS.MasterData.WebAPI.Customer.Tests.TestDataEnums;

namespace VSS.MasterData.WebAPI.Customer.Tests.TestDataGenerators
{
	public partial class CustomerTestDataGenerator
	{
		/// <summary>
		/// Create Customer Payload
		/// Customer Type
		/// Is Account Exists
		/// Is Customer Exists
		/// Create Account Status (true/false)
		/// Create Customer Status (true/false)
		/// Response Status Code
		/// Error Message
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetCreateCustomerTestData()
		{
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "Account01",
					BSSID = "BSSID01",
					NetworkCustomerCode = "NCC01",
					DealerAccountCode = "DAC01",
					DealerNetwork = "DN01",
					CustomerType = "Account",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Account,
				false,
				false,
				true,
				false,
				200,
				null
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "Account01",
					BSSID = "BSSID01",
					NetworkCustomerCode = "NCC01",
					DealerAccountCode = "DAC01",
					DealerNetwork = "DN01",
					CustomerType = "ACCOUNT",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Account,
				false,
				false,
				true,
				false,
				200,
				null
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "Account01",
					BSSID = "BSSID01",
					NetworkCustomerCode = "NCC01",
					DealerAccountCode = "DAC01",
					DealerNetwork = "DN01",
					CustomerType = "4",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Account,
				false,
				false,
				true,
				false,
				200,
				null
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "Account01",
					BSSID = "BSSID01",
					NetworkCustomerCode = "NCC01",
					DealerAccountCode = "DAC01",
					DealerNetwork = "DN01",
					CustomerType = "account",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Account,
				false,
				false,
				true,
				false,
				200,
				null
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "Account02",
					BSSID = "BSSID02",
					NetworkCustomerCode = "NCC02",
					DealerAccountCode = "DAC02",
					DealerNetwork = "DN02",
					CustomerType = "ABC",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.None,
				false,
				false,
				false,
				false,
				400,
				Messages.InvalidCustomerType
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "Account03",
					BSSID = "BSSID03",
					NetworkCustomerCode = "NCC03",
					DealerAccountCode = "DAC03",
					DealerNetwork = "DN03",
					CustomerType = "Account",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Account,
				true,
				false,
				false,
				false,
				400,
				Messages.AccountAlreadyExists
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "Customer01",
					BSSID = "BSSID01",
					NetworkCustomerCode = "NCC01",
					DealerAccountCode = "DAC01",
					DealerNetwork = "None",
					NetworkDealerCode = "NDC01",
					CustomerType = "Customer",
					PrimaryContactEmail = "testemail@cc.com",
					FirstName = "ABC",
					LastName = "DEF",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Customer,
				false,
				true,
				false,
				false,
				400,
				Messages.CustomerAlreadyExists
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "Customer01",
					BSSID = "BSSID01",
					NetworkCustomerCode = "NCC01",
					DealerAccountCode = "DAC01",
					DealerNetwork = "None",
					NetworkDealerCode = "NDC01",
					CustomerType = "Customer",
					PrimaryContactEmail = "testemail@cc.com",
					FirstName = "ABC",
					LastName = "DEF",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Customer,
				false,
				false,
				false,
				true,
				200,
				null
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "Customer01",
					BSSID = "BSSID01",
					NetworkCustomerCode = "NCC01",
					DealerAccountCode = "DAC01",
					DealerNetwork = "None",
					NetworkDealerCode = "NDC01",
					CustomerType = "0",
					PrimaryContactEmail = "testemail@cc.com",
					FirstName = "ABC",
					LastName = "DEF",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Customer,
				false,
				false,
				false,
				true,
				200,
				null
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "Customer01",
					BSSID = "BSSID01",
					NetworkCustomerCode = "NCC01",
					DealerAccountCode = "DAC01",
					DealerNetwork = "None",
					NetworkDealerCode = "NDC01",
					CustomerType = "customer",
					PrimaryContactEmail = "testemail@cc.com",
					FirstName = "ABC",
					LastName = "DEF",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Customer,
				false,
				false,
				false,
				true,
				200,
				null
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "Customer01",
					BSSID = "BSSID01",
					NetworkCustomerCode = "NCC01",
					DealerAccountCode = "DAC01",
					DealerNetwork = "None",
					NetworkDealerCode = "NDC01",
					CustomerType = "CUSTOMER",
					PrimaryContactEmail = "testemail@cc.com",
					FirstName = "ABC",
					LastName = "DEF",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Customer,
				false,
				false,
				false,
				true,
				200,
				null
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "Customer01",
					BSSID = "BSSID01",
					NetworkCustomerCode = "NCC01",
					DealerAccountCode = "DAC01",
					DealerNetwork = "None",
					NetworkDealerCode = "NDC01",
					CustomerType = "Corporate",
					PrimaryContactEmail = "testemail@cc.com",
					FirstName = "ABC",
					LastName = "DEF",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Corporate,
				false,
				false,
				false,
				true,
				200,
				null
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "Customer01",
					BSSID = "BSSID01",
					NetworkCustomerCode = "NCC01",
					DealerAccountCode = "DAC01",
					DealerNetwork = "None",
					NetworkDealerCode = "NDC01",
					CustomerType = "Operations",
					PrimaryContactEmail = "testemail@cc.com",
					FirstName = "ABC",
					LastName = "DEF",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Operations,
				false,
				false,
				false,
				true,
				200,
				null
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "Customer01",
					BSSID = "BSSID01",
					NetworkCustomerCode = "NCC01",
					DealerAccountCode = "DAC01",
					DealerNetwork = "None",
					NetworkDealerCode = "NDC01",
					CustomerType = "Customer",
					PrimaryContactEmail = "testemail@cc.com",
					FirstName = "ABC",
					LastName = "DEF",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Customer,
				false,
				false,
				false,
				false,
				400,
				Messages.UnableToSaveToDb
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "Dealer01",
					BSSID = "BSSID01",
					NetworkCustomerCode = "NCC01",
					DealerAccountCode = "DAC01",
					DealerNetwork = "None",
					NetworkDealerCode = "NDC01",
					CustomerType = "Dealer",
					PrimaryContactEmail = "testemail@cc.com",
					FirstName = "ABC",
					LastName = "DEF",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Dealer,
				false,
				false,
				false,
				true,
				200,
				null
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "Dealer02",
					BSSID = "BSSID02",
					NetworkCustomerCode = "NCC02",
					DealerAccountCode = "DAC02",
					DealerNetwork = "None",
					NetworkDealerCode = "NDC02",
					CustomerType = "1",
					PrimaryContactEmail = "testemail@cc.com",
					FirstName = "ABC",
					LastName = "DEF",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Dealer,
				false,
				false,
				false,
				true,
				200,
				null
			};
		}

		/// <summary>
		/// Update Customer Payload
		/// Customer Type
		/// Is Account Exists
		/// Is Customer Exists
		/// Update Account Status (true/false)
		/// Update Customer Status (true/false)
		/// Response Status Code
		/// Error Message
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetUpdateCustomerTestData()
		{
			yield return new object[]
			{
				new UpdateCustomerEvent
				{
					CustomerName = "Account01",
					BSSID = "BSSID01",
					NetworkCustomerCode = "NCC01",
					DealerAccountCode = "DAC01",
					DealerNetwork = "DN01",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Account,
				true,
				false,
				true,
				false,
				200,
				null
			};
			yield return new object[]
			{
				new UpdateCustomerEvent
				{
					CustomerName = "Account02",
					BSSID = "BSSID02",
					NetworkCustomerCode = "NCC02",
					DealerAccountCode = "DAC02",
					DealerNetwork = "DN02",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Account,
				false,
				false,
				false,
				false,
				400,
				Messages.CustomerDoesntExist
			};
			yield return new object[]
			{
				new UpdateCustomerEvent
				{
					CustomerName = "Customer01",
					BSSID = "BSSID01",
					NetworkCustomerCode = "NCC01",
					DealerAccountCode = "DAC01",
					DealerNetwork = "None",
					NetworkDealerCode = "NDC01",
					PrimaryContactEmail = "testemail@cc.com",
					FirstName = "ABC",
					LastName = "DEF",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Customer,
				false,
				true,
				false,
				true,
				200,
				null
			};
			yield return new object[]
			{
				new UpdateCustomerEvent
				{
					CustomerName = "Customer01",
					BSSID = "BSSID01",
					NetworkCustomerCode = "NCC01",
					DealerAccountCode = "DAC01",
					DealerNetwork = "None",
					NetworkDealerCode = "NDC01",
					PrimaryContactEmail = "testemail@cc.com",
					FirstName = "ABC",
					LastName = "DEF",
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Customer,
				false,
				true,
				false,
				false,
				400,
				Messages.UnableToSaveToDb
			};
			yield return new object[]
			{
				new UpdateCustomerEvent
				{
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				CustomerType.Customer,
				false,
				true,
				false,
				false,
				400,
				Messages.NoDataToUpdate
			};
		}

		/// <summary>
		/// Customer UID
		/// ActionUTC
		/// Customer Type
		/// Is Account Exists
		/// Is Customer Exists
		/// Delete Account Status (true/false)
		/// Delete Customer Status (true/false)
		/// Response Status Code
		/// Error Message
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetDeleteCustomerTestData()
		{
			yield return new object[]
			{
				Guid.NewGuid(),
				DateTime.UtcNow,
				CustomerType.Account,
				true,
				false,
				true,
				false,
				200,
				null
			};
			yield return new object[]
			{
				Guid.NewGuid(),
				DateTime.UtcNow,
				CustomerType.Customer,
				false,
				true,
				false,
				true,
				200,
				null
			};
			yield return new object[]
			{
				Guid.NewGuid(),
				DateTime.UtcNow,
				CustomerType.Account,
				false,
				false,
				false,
				false,
				400,
				Messages.CustomerDoesntExist
			};
			yield return new object[]
			{
				Guid.NewGuid(),
				DateTime.UtcNow,
				CustomerType.Account,
				true,
				false,
				false,
				false,
				400,
				Messages.UnableToSaveToDb
			};
		}

		/// <summary>
		/// Associate Customer Asset Payload
		/// Is Customer Exists
		/// Is Customer Asset RelationshipType Exists
		/// Associate Asset Customer Status (true/false)
		/// Status code
		/// Error Message
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetAssociateAssetCustomerTestData()
		{
			yield return new object[]
			{
				new AssociateCustomerAssetEvent
				{
					CustomerUID = Guid.NewGuid(),
					AssetUID = Guid.NewGuid(),
					RelationType = "Owner",
					ActionUTC = DateTime.UtcNow
				},
				true,
				false,
				true,
				200,
				null
			};
			yield return new object[]
			{
				new AssociateCustomerAssetEvent
				{
					CustomerUID = Guid.NewGuid(),
					AssetUID = Guid.NewGuid(),
					RelationType = "Owner",
					ActionUTC = DateTime.UtcNow
				},
				true,
				true,
				false,
				409,
				Messages.AssociationAlreadyExists
			};
			yield return new object[]
			{
				new AssociateCustomerAssetEvent
				{
					CustomerUID = Guid.NewGuid(),
					AssetUID = Guid.NewGuid(),
					RelationType = "ABC",
					ActionUTC = DateTime.UtcNow
				},
				true,
				false,
				false,
				400,
				Messages.InvalidRelationshipType
			};
			yield return new object[]
			{
				new AssociateCustomerAssetEvent
				{
					CustomerUID = Guid.NewGuid(),
					AssetUID = Guid.NewGuid(),
					RelationType = "Owner",
					ActionUTC = DateTime.UtcNow
				},
				false,
				false,
				false,
				400,
				Messages.CustomerDoesntExist
			};
			yield return new object[]
			{
				new AssociateCustomerAssetEvent
				{
					CustomerUID = Guid.NewGuid(),
					AssetUID = Guid.NewGuid(),
					RelationType = "Owner",
					ActionUTC = DateTime.UtcNow
				},
				true,
				false,
				false,
				400,
				Messages.UnableToSaveToDb
			};
		}

		/// <summary>
		/// Dissociate Customer Asset Payload
		/// Is Customer Exists
		/// Is Customer Asset Exists
		/// Dissociate Asset Customer Status (true/false)
		/// Status code
		/// Error Message
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetDissociateAssetCustomerTestData()
		{
			yield return new object[]
			{
				new DissociateCustomerAssetEvent
				{
					CustomerUID = Guid.NewGuid(),
					AssetUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				true,
				true,
				true,
				200,
				null
			};
			yield return new object[]
			{
				new DissociateCustomerAssetEvent
				{
					CustomerUID = Guid.NewGuid(),
					AssetUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				false,
				false,
				false,
				400,
				Messages.CustomerDoesntExist
			};
			yield return new object[]
			{
				new DissociateCustomerAssetEvent
				{
					CustomerUID = Guid.NewGuid(),
					AssetUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				true,
				false,
				false,
				400,
				Messages.CustomerAssetDoesntExist
			};
			yield return new object[]
			{
				new DissociateCustomerAssetEvent
				{
					CustomerUID = Guid.NewGuid(),
					AssetUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				true,
				true,
				false,
				400,
				Messages.UnableToSaveToDb
			};
		}

		/// <summary>
		/// AssetCustomerDetail object
		/// Customer type text
		/// Parent Customer type text
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetAssetCustomerDetailTestData()
		{
			yield return new object[]
			{
				new AssetCustomerDetail
				{
					CustomerName = "Cus01",
					CustomerUID = Guid.NewGuid(),
					CustomerType = 0,
					ParentCustomerType = 1,
					ParentCustomerUID = Guid.NewGuid(),
					ParentName = "DEA01"
				},
				"Customer",
				"Dealer"
			};
			yield return new object[]
			{
				new AssetCustomerDetail
				{
					CustomerName = "Cus02",
					CustomerUID = Guid.NewGuid(),
					CustomerType = -1,
					ParentCustomerType = -1
				},
				null,
				null
			};
		}

		/// <summary>
		/// Event payload
		/// Validation messages (| separated)
		/// Validation message count
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetEventsForRequiredFieldValidation()
		{
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "CUS01",
					DealerNetwork = "DN01",
					ActionUTC = DateTime.UtcNow,
					CustomerUID = Guid.NewGuid()
				},
				"The CustomerType field is required.",
				1
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "CUS01",
					CustomerType = "Customer",
					ActionUTC = DateTime.UtcNow
				},
				"The CustomerUID field is required.|The DealerNetwork field is required.",
				2
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerName = "CUS01",
					CustomerType = "Customer",
					CustomerUID = Guid.NewGuid(),
					DealerNetwork = "DN01"
				},
				"The ActionUTC field is required.",
				1
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerType = "Customer",
					CustomerUID = Guid.NewGuid(),
					DealerNetwork = "DN01",
					ActionUTC = DateTime.UtcNow
				},
				"The CustomerName field is required.",
				1
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerType = "Customer",
					CustomerUID = Guid.Empty,
					DealerNetwork = "DN01",
					ActionUTC = DateTime.MinValue,
					CustomerName = "CusNam01"
				},
				"Required field values must be valid.",
				1
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerType = "",
					DealerNetwork = "",
					CustomerName = ""
				},
				"The CustomerName field is required.|The CustomerType field is required.|" +
				"The DealerNetwork field is required.|The CustomerUID field is required.|" +
				"The ActionUTC field is required.",
				5
			};
			yield return new object[]
			{
				new CreateCustomerEvent
				{
					CustomerType = null,
					DealerNetwork = null,
					CustomerName = null
				},
				"The CustomerName field is required.|The CustomerType field is required.|" +
				"The DealerNetwork field is required.|The CustomerUID field is required.|" +
				"The ActionUTC field is required.",
				5
			};
		}
	}
}
