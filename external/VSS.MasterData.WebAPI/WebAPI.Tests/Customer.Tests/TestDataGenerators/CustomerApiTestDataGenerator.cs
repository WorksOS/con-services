using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.Customer.KafkaModel;
using VSS.MasterData.WebAPI.Utilities;

namespace VSS.MasterData.WebAPI.Customer.Tests.TestDataGenerators
{
	public partial class CustomerTestDataGenerator
	{
		/// <summary>
		/// Create User Cutsomer Relationship Payload
		/// Is Customer exists
		/// Is Customer User exists
		/// Create Use Customer Status (true/false)
		/// Status Code
		/// Error Message
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetCreateUserCutsomerRelationshipTestData()
		{
			yield return new object[]
			{
				new CreateUserCustomerRelationshipEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					JobTitle = "XYZ",
					JobType = "ABC",
					ActionUTC = DateTime.UtcNow,
					CreatedByUserUID = Guid.NewGuid()
				},
				true,
				false,
				true,
				200,
				null
			};
			yield return new object[]
			{
				new CreateUserCustomerRelationshipEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					JobTitle = "XYZ",
					JobType = "ABC",
					ActionUTC = DateTime.UtcNow,
					CreatedByUserUID = Guid.NewGuid()
				},
				true,
				true,
				false,
				400,
				Messages.CustomerUserAlreadyExists
			};
			yield return new object[]
			{
				new CreateUserCustomerRelationshipEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					JobTitle = "XYZ",
					JobType = "ABC",
					ActionUTC = DateTime.UtcNow,
					CreatedByUserUID = Guid.NewGuid()
				},
				false,
				false,
				false,
				400,
				Messages.CustomerDoesntExist
			};
			yield return new object[]
			{
				new CreateUserCustomerRelationshipEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					JobTitle = "XYZ",
					JobType = "ABC",
					ActionUTC = DateTime.UtcNow,
					CreatedByUserUID = Guid.NewGuid()
				},
				true,
				false,
				false,
				400,
				Messages.UnableToSaveToDb
			};
		}

		/// <summary>
		/// Update User Cutsomer Relationship Payload
		/// Is Customer exists
		/// Is Customer User exists
		/// Update Use Customer Status (true/false)
		/// Status Code
		/// Error Message
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetUpdateUserCutsomerRelationshipTestData()
		{
			yield return new object[]
			{
				new UpdateUserCustomerRelationshipEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					JobTitle = "XYZ",
					JobType = "ABC",
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
				new UpdateUserCustomerRelationshipEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					JobTitle = "XYZ",
					JobType = "ABC",
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
				new UpdateUserCustomerRelationshipEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					JobTitle = "XYZ",
					JobType = "ABC",
					ActionUTC = DateTime.UtcNow
				},
				true,
				false,
				false,
				400,
				Messages.CustomerUserDoesntExist
			};
			yield return new object[]
			{
				new UpdateUserCustomerRelationshipEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					JobTitle = "XYZ",
					JobType = "ABC",
					ActionUTC = DateTime.UtcNow
				},
				true,
				true,
				false,
				400,
				Messages.PublishKafkaFailure
			};
		}

		/// <summary>
		/// Delete User Cutsomer Relationship Payload
		/// Is Customer User exists
		/// Delete User Customer Status (true/false)
		/// Status Code
		/// Error Message
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetDeleteUserCutsomerRelationshipTestData()
		{
			yield return new object[]
			{
				new DeleteUserCustomerRelationshipEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				true,
				true,
				200,
				null
			};
			yield return new object[]
			{
				new DeleteUserCustomerRelationshipEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				true,
				false,
				400,
				Messages.UnableToSaveToDb
			};
			yield return new object[]
			{
				new DeleteUserCustomerRelationshipEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				false,
				false,
				400,
				Messages.CustomerUserAssociationNotExists
			};
		}

		/// <summary>
		/// Associate Customer User Payload
		/// Is Customer exists
		/// Is Customer User exists
		/// Associate Customer User status (true/false)
		/// Status Code
		/// Error Message
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetAssociateCustomerUserTestData()
		{
			yield return new object[]
			{
				new AssociateCustomerUserEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
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
				new AssociateCustomerUserEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
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
				new AssociateCustomerUserEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				true,
				true,
				false,
				400,
				Messages.CustomerUserAlreadyExists
			};
			yield return new object[]
			{
				new AssociateCustomerUserEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
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
		/// Dissociate Customer User Payload
		/// Is Customer User exists
		/// Dissociate Customer User status (true/false)
		/// Status Code
		/// Error Message
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetDissociateCustomerUserTestData()
		{
			yield return new object[]
			{
				new DissociateCustomerUserEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				true,
				true,
				200,
				null
			};
			yield return new object[]
			{
				new DissociateCustomerUserEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				false,
				false,
				400,
				Messages.CustomerUserAssociationNotExists
			};
			yield return new object[]
			{
				new DissociateCustomerUserEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				true,
				false,
				400,
				Messages.UnableToSaveToDb
			};
		}

		/// <summary>
		/// Bulk Dissociate Customer User payload
		/// Dissociate status (true/false)
		/// Status code
		/// Error message
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetBulkDissociateCustomerUserTestData()
		{
			yield return new object[]
			{
				new BulkDissociateCustomerUserEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = new List<Guid>{ Guid.NewGuid() },
					ActionUTC = DateTime.UtcNow
				},
				true,
				200,
				null
			};
			yield return new object[]
			{
				new BulkDissociateCustomerUserEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = new List<Guid>{ Guid.NewGuid() },
					ActionUTC = DateTime.UtcNow
				},
				false,
				400,
				Messages.CustomerUsersDoesntExist
			};
		}

		/// <summary>
		/// CreateCustomerRelationship payload
		/// Is AccountCustomerUID valid
		/// Is Account exists
		/// Create Account Customer relationship status (true/false)
		/// Parent & Child customer UIDs same
		/// Has Customer relationships
		/// Create customer relationship status (true/false)
		/// Status Code
		/// Error Message
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetCreateCustomerRelationshipTestData()
		{
			yield return new object[]
			{
				new CreateCustomerRelationshipEvent
				{
					AccountCustomerUID = Guid.NewGuid(),
					ChildCustomerUID = Guid.NewGuid(),
					ParentCustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				true,
				true,
				true,
				false,
				false,
				true,
				200,
				null
			};
			yield return new object[]
			{
				new CreateCustomerRelationshipEvent
				{
					AccountCustomerUID = Guid.NewGuid(),
					ChildCustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				true,
				true,
				true,
				false,
				false,
				true,
				200,
				null
			};
			yield return new object[]
			{
				new CreateCustomerRelationshipEvent
				{
					ChildCustomerUID = Guid.NewGuid(),
					ParentCustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow
				},
				false,
				false,
				false,
				false,
				false,
				false,
				500,
				null
			};
			yield return new object[]
			{
				new CreateCustomerRelationshipEvent
				{
					ChildCustomerUID = Guid.Empty,
					ParentCustomerUID = Guid.Empty,
					ActionUTC = DateTime.UtcNow
				},
				false,
				false,
				false,
				false,
				false,
				false,
				400,
				Messages.BothParentChildCustomerEmpty
			};
			yield return new object[]
			{
				new CreateCustomerRelationshipEvent
				{
					ChildCustomerUID = Guid.Empty,
					ActionUTC = DateTime.UtcNow
				},
				false,
				false,
				false,
				false,
				false,
				false,
				400,
				Messages.BothParentChildCustomerEmpty
			};
		}

		/// <summary>
		/// Parent CustomerUid
		/// Child CustomerUid
		/// Is Parent & Child Same
		/// Delete Customer Relationship status (true/false)
		/// Has Account CustomerUid
		/// Account CustomerUid
		/// Delete Type
		/// Is Account exists
		/// Delete Account Customer Relationship status (true/false)
		/// Create Account Customer Relationship status (true/false)
		/// Is Dealer Customer Relationship exists
		/// Status Code
		/// Error Message
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetDeleteCustomerRelationshipTestData()
		{
			yield return new object[]
			{
				Guid.Empty,
				Guid.Empty,
				true,
				false,
				false,
				Guid.Empty,
				"Remove",
				false,
				false,
				false,
				false,
				400,
				Messages.InvalidCustomerUID
			};
			yield return new object[]
			{
				Guid.NewGuid(),
				Guid.NewGuid(),
				false,
				false,
				false,
				Guid.Empty,
				"Rem",
				false,
				false,
				false,
				true,
				400,
				Messages.InvalidDeleteType
			};
			yield return new object[]
			{
				Guid.NewGuid(),
				Guid.NewGuid(),
				false,
				false,
				false,
				Guid.Empty,
				"Rem",
				false,
				false,
				false,
				false,
				400,
				Messages.CustomerDealerRelationNotExists
			};
			yield return new object[]
			{
				Guid.NewGuid(),
				Guid.NewGuid(),
				false,
				false,
				false,
				Guid.Empty,
				"Remove",
				false,
				false,
				false,
				true,
				400,
				Messages.UnableToSaveToDb
			};
			yield return new object[]
			{
				Guid.NewGuid(),
				Guid.NewGuid(),
				false,
				true,
				false,
				Guid.Empty,
				"Remove",
				false,
				false,
				false,
				true,
				200,
				null
			};
			yield return new object[]
			{
				Guid.NewGuid(),
				Guid.NewGuid(),
				false,
				true,
				true,
				Guid.NewGuid(),
				"Remove",
				true,
				false,
				true,
				true,
				200,
				null
			};
			yield return new object[]
			{
				Guid.NewGuid(),
				Guid.NewGuid(),
				false,
				true,
				true,
				Guid.NewGuid(),
				"Remove",
				true,
				false,
				false,
				true,
				400,
				Messages.UnableToUpdateCustomerAccountInDb
			};
			yield return new object[]
			{
				new Guid("07078c24-4836-480e-97c7-96f58952ff90"),
				new Guid("07078c24-4836-480e-97c7-96f58952ff90"),
				true,
				false,
				true,
				Guid.NewGuid(),
				"Remove",
				true,
				true,
				false,
				false,
				200,
				null
			};
			yield return new object[]
			{
				new Guid("18904e6d-5bcf-48e5-a364-5a213cb52c8f"),
				new Guid("18904e6d-5bcf-48e5-a364-5a213cb52c8f"),
				true,
				false,
				true,
				Guid.NewGuid(),
				"Remove",
				true,
				false,
				false,
				false,
				400,
				Messages.UnableToDeleteCustomerAccountInDb
			};
		}
	}
}
