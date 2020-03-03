using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.Customer.KafkaModel;

namespace VSS.MasterData.WebAPI.CustomerRepository.Tests.TestDataGenerators
{
	public class CustomerRepositoryTestDataGenerator
	{
		/// <summary>
		/// AssociateCustomerUserEvent payload
		/// Is Valid Customer
		/// Transaction Status
		/// Upsert calls count
		/// Publish calls count
		/// Has Exception
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
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow
				},
				true,
				true,
				1,
				1,
				false
			};
			yield return new object[]
			{
				new AssociateCustomerUserEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow
				},
				false,
				false,
				0,
				0,
				false
			};
			yield return new object[]
			{
				new AssociateCustomerUserEvent
				{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow
				},
				false,
				false,
				0,
				0,
				true
			};
		}

		/// <summary>
		/// DissociateCustomerUserEvent payload
		/// Transaction Status
		/// Delete calls count
		/// Publish calls count
		/// Has Exception
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetDissociateCustomerUserTestData()
		{
			yield return new object[]
			{
				new DissociateCustomerUserEvent
				{
					UserUID = Guid.NewGuid(),
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow
				},
				true,
				1,
				1,
				false
			};
			yield return new object[]
			{
				new DissociateCustomerUserEvent
				{
					UserUID = Guid.NewGuid(),
					CustomerUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow
				},
				false,
				0,
				0,
				true
			};
		}

		/// <summary>
		/// Input payload
		/// Has Valid Customer
		/// Transaction Status
		/// Upsert calls count
		/// Publish calls count
		/// Has Exception
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetCreateUserCustomerRelationshipTestData()
		{
			yield return new object[] {
				new CreateUserCustomerRelationshipEvent{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					JobTitle = "JT01",
					JobType = "JTy01",
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow,
					CreatedByUserUID = new Guid()
				},
				true,
				true,
				1,
				1,
				false
			};
			yield return new object[] {
				new CreateUserCustomerRelationshipEvent{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					JobTitle = "JT02",
					JobType = "JTy02",
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow,
				},
				true,
				true,
				1,
				1,
				false
			};
			yield return new object[] {
				new CreateUserCustomerRelationshipEvent{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					JobTitle = "JT03",
					JobType = "JTy03",
					CreatedByUserUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow,
				},
				false,
				false,
				0,
				0,
				false
			};
			yield return new object[] {
				new CreateUserCustomerRelationshipEvent{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					JobTitle = "JT04",
					JobType = "JTy04",
					CreatedByUserUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow,
				},
				false,
				false,
				0,
				0,
				true
			};
			yield return new object[] {
				new CreateUserCustomerRelationshipEvent{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					JobTitle = "",
					JobType = "",
					CreatedByUserUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow,
				},
				true,
				true,
				1,
				1,
				false
			};
			yield return new object[] {
				new CreateUserCustomerRelationshipEvent{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					CreatedByUserUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow,
				},
				true,
				true,
				1,
				1,
				false
			};
		}

		/// <summary>
		/// Input payload
		/// Transaction status
		/// Upsert calls count
		/// Publish calls count
		/// Has Exception
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetUpdateUserCustomerRelationshipTestData()
		{
			yield return new object[] {
				new UpdateUserCustomerRelationshipEvent{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					JobTitle = "JT01",
					JobType = "JTy01",
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow,
				},
				true,
				0,
				1,
				false
			};
			yield return new object[] {
				new UpdateUserCustomerRelationshipEvent{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					JobTitle = "JT01",
					JobType = "JTy01",
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow,
				},
				false,
				0,
				1,
				true
			};
			yield return new object[] {
				new UpdateUserCustomerRelationshipEvent{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					JobTitle = "",
					JobType = "",
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow,
				},
				true,
				0,
				1,
				false
			};
			yield return new object[] {
				new UpdateUserCustomerRelationshipEvent{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow,
				},
				true,
				0,
				1,
				false
			};
		}


		/// <summary>
		/// Input payload
		/// Transaction Status
		/// Upsert calls count
		/// Delete calls count
		/// Publish calls count
		/// Has Exception
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<object[]> GetDeleteUserCustomerRelationshipTestData()
		{
			yield return new object[] {
				new DeleteUserCustomerRelationshipEvent{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow
				},
				true,
				0,
				1,
				1,
				false
			};
			yield return new object[] {
				new DeleteUserCustomerRelationshipEvent{
					CustomerUID = Guid.NewGuid(),
					UserUID = Guid.NewGuid(),
					ActionUTC = DateTime.UtcNow,
					ReceivedUTC = DateTime.UtcNow
				},
				false,
				0,
				1,
				0,
				true
			};
		}
	}
}
