using KafkaModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.MasterData.WebAPI.Customer.KafkaModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Transactions;
using VSS.MasterData.WebAPI.Utilities.Extensions;

namespace VSS.MasterData.WebAPI.CustomerRepository
{
	public class UserCustomerService : CustomerService, IUserCustomerService
	{
		private readonly ITransactions transaction;
		private static List<string> CustomerTopics;
		private readonly ILogger logger;

		public UserCustomerService(ITransactions transaction, IConfiguration configuration, ILogger logger)
			: base(transaction, configuration, logger)
		{
			this.transaction = transaction;
			this.logger = logger;
			CustomerTopics = configuration["CustomerTopicNames"]
				.Split(',')
				.Select(t => t + configuration["TopicSuffix"])
				.ToList();
		}

		public DbUserCustomer GetCustomerUser(Guid customerUid, Guid userUid)
		{
			var getUserCustomerQuery = "SELECT UserCustomerID,fk_UserUID,fk_CustomerUID,fk_CustomerID," +
										"LastUserUTC FROM md_customer_CustomerUser WHERE fk_CustomerUID = {0} AND fk_UserUID = {1};";
			return transaction.Get<DbUserCustomer>(string.Format(getUserCustomerQuery,
				customerUid.ToStringAndWrapWithUnhex(),
				userUid.ToStringAndWrapWithUnhex()))?.FirstOrDefault();
		}

		public IEnumerable<DbUserCustomer> GetUsersForCustomer(Guid customerUid, List<Guid> userUids)
		{
			List<string> userGuids = userUids.Select(g => $"0x{g.ToStringWithoutHyphens()}").ToList();
			var getUserCustomerQuery = "SELECT UserCustomerID,fk_UserUID,fk_CustomerUID,fk_CustomerID," +
										"LastUserUTC FROM md_customer_CustomerUser WHERE fk_CustomerUID = {0} AND fk_UserUID IN ({1});";
			return transaction.Get<DbUserCustomer>(string.Format(getUserCustomerQuery,
				customerUid.ToStringAndWrapWithUnhex(),
				string.Join(",", userGuids)));
		}

		public bool AssociateCustomerUser(AssociateCustomerUserEvent associateCustomerUser)
		{
			try
			{
				var customer = GetCustomer(associateCustomerUser.CustomerUID);
				if (customer?.CustomerID > 0)
				{
					List<KafkaMessage> messages = CustomerTopics
						?.Select(topic => new KafkaMessage
						{
							Key = associateCustomerUser.CustomerUID.ToString(),
							Message = new { AssociateCustomerUserEvent = associateCustomerUser },
							Topic = topic
						})
						?.ToList();

					var userCustomer = new DbUserCustomer
					{
						fk_CustomerID = customer.CustomerID,
						fk_CustomerUID = associateCustomerUser.CustomerUID,
						fk_UserUID = associateCustomerUser.UserUID,
						LastUserUTC = DateTime.UtcNow
					};

					var actions = new List<Action>
					{
						() => transaction.Upsert(userCustomer),
						() => transaction.Publish(messages)
					};
					return transaction.Execute(actions);
				}

				return false;
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while associating user for customer : {ex.Message}, {ex.StackTrace}");
				throw;
			}
		}

		public bool DissociateCustomerUser(DissociateCustomerUserEvent dissociateCustomerUser)
		{
			try
			{
				List<KafkaMessage> messages = CustomerTopics
					?.Select(topic => new KafkaMessage
					{
						Key = dissociateCustomerUser.CustomerUID.ToString(),
						Message = new { DissociateCustomerUserEvent = dissociateCustomerUser },
						Topic = topic
					})
					?.ToList();

				var deleteQuery = string.Format("DELETE FROM md_customer_CustomerUser " +
												"WHERE fk_CustomerUID = {0} AND fk_UserUID = {1};",
					dissociateCustomerUser.CustomerUID.ToStringAndWrapWithUnhex(),
					dissociateCustomerUser.UserUID.ToStringAndWrapWithUnhex());

				var actions = new List<Action>
				{
					() => transaction.Delete(deleteQuery),
					() => transaction.Publish(messages)
				};
				return transaction.Execute(actions);
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while dissociating user for customer : {ex.Message}, {ex.StackTrace}");
				throw;
			}
		}

		public bool BulkDissociateCustomerUser(
			Guid customerUid, List<Guid> userUids, DateTime actionUtc)
		{
			try
			{
				IEnumerable<DbUserCustomer> userCustomers = GetUsersForCustomer(customerUid, userUids);
				if (userCustomers?.Count() > 0)
				{
					var dateNow = DateTime.UtcNow;

					List<KafkaMessage> messages = new List<KafkaMessage>();
					userCustomers.ToList().ForEach(uc =>
					{
						messages.AddRange(CustomerTopics
						?.Select(topic => new KafkaMessage
						{
							Key = customerUid.ToString(),
							Message = new
							{
								DissociateCustomerUserEvent = new DissociateCustomerUserEvent
								{
									CustomerUID = customerUid,
									UserUID = uc.fk_UserUID,
									ActionUTC = actionUtc,
									ReceivedUTC = dateNow
								}
							},
							Topic = topic
						}));
					});

					IEnumerable<int> userCustomerIDs = userCustomers.Select(uc => uc.UserCustomerID);
					var deleteQuery = string.Format(
						"DELETE FROM md_customer_CustomerUser WHERE UserCustomerID IN ({0});",
						string.Join(",", userCustomerIDs));
					var actions = new List<Action>
					{
						() => transaction.Delete(deleteQuery),
						() => transaction.Publish(messages)
					};
					return transaction.Execute(actions);
				}
				return false;
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while bulk dissociating users for customer : {ex.Message}, {ex.StackTrace}");
				throw;
			}
		}
	}
}