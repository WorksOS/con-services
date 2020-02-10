using KafkaModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.Customer.KafkaModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Transactions;
using VSS.MasterData.WebAPI.Utilities.Extensions;
using VSS.MasterData.WebAPI.Utilities.Helpers;
using static VSS.MasterData.WebAPI.Utilities.Enums.Enums;
using CustomerEnum = VSS.MasterData.WebAPI.Utilities.Enums;

namespace VSS.MasterData.WebAPI.CustomerRepository
{
	public class AccountService : IAccountService
	{
		private readonly ITransactions transaction;
		private static string AccountTopic;
		private readonly ILogger logger;

		public AccountService(ITransactions transaction, IConfiguration configuration, ILogger logger)
		{
			this.transaction = transaction;
			this.logger = logger;
			AccountTopic = configuration["AccountTopicName"] + configuration["TopicSuffix"] ?? string.Empty;
		}

		public DbAccount GetAccount(Guid accountUid)
		{
			var getAccountQuery = "select CustomerAccountID,CustomerAccountUID,BSSID,AccountName," +
								"NetworkCustomerCode,DealerAccountCode,fk_ParentCustomerUID,fk_ChildCustomerUID,RowUpdatedUTC " +
								"from md_customer_CustomerAccount where CustomerAccountUID = {0}";
			return transaction.Get<DbAccount>(string.Format(getAccountQuery,
				accountUid.ToStringAndWrapWithUnhex()))?.FirstOrDefault();
		}

		public bool CreateAccount(CreateCustomerEvent createAccount)
		{
			try
			{
				FieldHelper.ReplaceEmptyFieldsByNull(createAccount);

				var message = new KafkaMessage
				{
					Key = createAccount.CustomerUID.ToString(),
					Topic = AccountTopic,
					Message = new
					{
						AccountEvent = new AccountEvent
						{
							AccountName = createAccount.CustomerName,
							AccountUID = createAccount.CustomerUID,
							Action = Operation.Create.ToString(),
							BSSID = createAccount.BSSID,
							DealerAccountCode = createAccount.DealerAccountCode,
							NetworkCustomerCode = createAccount.NetworkCustomerCode,
							ActionUTC = createAccount.ActionUTC,
							ReceivedUTC = createAccount.ReceivedUTC
						}
					}
				};

				var accountObj = new DbAccount
				{
					CustomerAccountUID = createAccount.CustomerUID,
					BSSID = createAccount.BSSID,
					AccountName = createAccount.CustomerName,
					NetworkCustomerCode = createAccount.NetworkCustomerCode,
					DealerAccountCode = createAccount.DealerAccountCode,
					RowUpdatedUTC = DateTime.UtcNow
				};

				var actions = new List<Action>()
				{
					() => transaction.Upsert(accountObj),
					() => transaction.Publish(message)
				};
				return transaction.Execute(actions);
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while creating account customer : {ex.Message}, {ex.StackTrace}");
				throw;
			}
		}

		public bool UpdateAccount(UpdateCustomerEvent updateAccount, DbAccount accountDetails)
		{
			try
			{
				var accountEvent = new AccountEvent
				{
					AccountName = updateAccount.CustomerName,
					AccountUID = updateAccount.CustomerUID,
					Action = Operation.Update.ToString(),
					BSSID = updateAccount.BSSID,
					DealerAccountCode = updateAccount.DealerAccountCode,
					NetworkCustomerCode = updateAccount.NetworkCustomerCode,
					fk_ParentCustomerUID = accountDetails.fk_ParentCustomerUID ?? null,
					fk_ChildCustomerUID = accountDetails.fk_ChildCustomerUID ?? null,
					ActionUTC = updateAccount.ActionUTC,
					ReceivedUTC = updateAccount.ReceivedUTC
				};

				if (!FieldHelper.IsValidValuesFilled(accountEvent, accountDetails, logger))
				{
					logger.LogError("DB Object expects typeOf IDbTable");
				}

				FieldHelper.ReplaceEmptyFieldsByNull(accountEvent);

				var message = new KafkaMessage
				{
					Key = updateAccount.CustomerUID.ToString(),
					Topic = AccountTopic,
					Message = new { AccountEvent = accountEvent }
				};

				var accountObj = new DbAccount
				{
					CustomerAccountID = accountDetails.CustomerAccountID,
					CustomerAccountUID = accountEvent.AccountUID,
					BSSID = accountEvent.BSSID,
					AccountName = accountEvent.AccountName,
					NetworkCustomerCode = accountEvent.NetworkCustomerCode,
					DealerAccountCode = accountEvent.DealerAccountCode,
					fk_ChildCustomerUID = accountDetails.fk_ChildCustomerUID ?? null,
					fk_ParentCustomerUID = accountDetails.fk_ParentCustomerUID ?? null,
					RowUpdatedUTC = DateTime.UtcNow
				};

				var actions = new List<Action>()
				{
					() => transaction.Upsert(accountObj),
					() => transaction.Publish(message)
				};
				return transaction.Execute(actions);
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while updating account customer : {ex.Message}, {ex.StackTrace}");
				throw;
			}
		}

		public bool DeleteAccount(Guid customerAccountUid, DateTime actionUtc, DbAccount accountDetails)
		{
			try
			{
				var message = new KafkaMessage
				{
					Key = customerAccountUid.ToString(),
					Topic = AccountTopic,
					Message = new
					{
						AccountEvent = new AccountEvent
						{
							AccountName = accountDetails.AccountName,
							AccountUID = customerAccountUid,
							Action = Operation.Delete.ToString(),
							BSSID = accountDetails.BSSID,
							DealerAccountCode = accountDetails.DealerAccountCode,
							NetworkCustomerCode = accountDetails.NetworkCustomerCode,
							fk_ParentCustomerUID = null,
							fk_ChildCustomerUID = null,
							ActionUTC = actionUtc,
							ReceivedUTC = DateTime.UtcNow
						}
					}
				};

				var deleteQuery = string.Format("DELETE FROM md_customer_CustomerAccount " +
												"WHERE CustomerAccountUID = {0};",
					customerAccountUid.ToStringAndWrapWithUnhex());

				var actions = new List<Action>()
				{
					() => transaction.Delete(deleteQuery),
					() => transaction.Publish(message)
				};
				return transaction.Execute(actions);
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while deleting customer account : {ex.Message}, {ex.StackTrace}");
				throw;
			}
		}

		public bool CreateAccountCustomerRelationShip(CreateCustomerRelationshipEvent customerRelationship,
													DbAccount accountDetails)
		{
			try
			{
				var message = new KafkaMessage
				{
					Key = customerRelationship.AccountCustomerUID.ToString(),
					Topic = AccountTopic,
					Message = new
					{
						AccountEvent = new AccountEvent()
						{
							AccountName = accountDetails.AccountName,
							AccountUID = accountDetails.CustomerAccountUID,
							Action = Operation.Update.ToString(),
							BSSID = accountDetails.BSSID,
							DealerAccountCode = accountDetails.DealerAccountCode,
							NetworkCustomerCode = accountDetails.NetworkCustomerCode,
							fk_ParentCustomerUID = customerRelationship.ParentCustomerUID,
							fk_ChildCustomerUID = customerRelationship.ChildCustomerUID,
							ActionUTC = customerRelationship.ActionUTC,
							ReceivedUTC = DateTime.UtcNow
						}
					}
				};

				var accountobj = new DbAccount
				{
					CustomerAccountID = accountDetails.CustomerAccountID,
					CustomerAccountUID = accountDetails.CustomerAccountUID,
					BSSID = accountDetails.BSSID,
					AccountName = accountDetails.AccountName,
					NetworkCustomerCode = accountDetails.NetworkCustomerCode,
					DealerAccountCode = accountDetails.DealerAccountCode,
					fk_ChildCustomerUID = customerRelationship.ChildCustomerUID,
					fk_ParentCustomerUID = customerRelationship.ParentCustomerUID ?? null,
					RowUpdatedUTC = DateTime.UtcNow
				};

				var actions = new List<Action>
				{
					() => transaction.Upsert(accountobj),
					() => transaction.Publish(message)
				};
				return transaction.Execute(actions);
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while creating account customer relationship : {ex.Message}, {ex.StackTrace}");
				throw;
			}
		}

		public bool CreateAccountCustomerRelationShip(Guid parentCustomerUID, Guid childCustomerUID,
													DbAccount accountDetails, DateTime actionUTC, string deleteType)
		{
			try
			{
				var message = new KafkaMessage
				{
					Key = accountDetails.CustomerAccountUID.ToString(),
					Topic = AccountTopic,
					Message = new
					{
						AccountEvent = new AccountEvent()
						{
							AccountName = accountDetails.AccountName,
							AccountUID = accountDetails.CustomerAccountUID,
							Action = Operation.Delete.ToString(),
							BSSID = accountDetails.BSSID,
							DealerAccountCode = accountDetails.DealerAccountCode,
							NetworkCustomerCode = accountDetails.NetworkCustomerCode,
							fk_ParentCustomerUID =
								deleteType.ToLower() == CustomerEnum.DeleteType.RemoveDealer.ToString().ToLower()
									? childCustomerUID
									: parentCustomerUID,
							fk_ChildCustomerUID =
								deleteType.ToLower() == CustomerEnum.DeleteType.RemoveDealer.ToString().ToLower()
									? childCustomerUID
									: parentCustomerUID,
							ActionUTC = actionUTC,
							ReceivedUTC = DateTime.UtcNow
						}
					}
				};

				var accountobj = new DbAccount
				{
					CustomerAccountID = accountDetails.CustomerAccountID,
					CustomerAccountUID = accountDetails.CustomerAccountUID,
					BSSID = accountDetails.BSSID,
					AccountName = accountDetails.AccountName,
					NetworkCustomerCode = accountDetails.NetworkCustomerCode,
					DealerAccountCode = accountDetails.DealerAccountCode,
					fk_ChildCustomerUID =
						deleteType.ToLower() == CustomerEnum.DeleteType.RemoveDealer.ToString().ToLower()
							? childCustomerUID
							: parentCustomerUID,
					fk_ParentCustomerUID =
						deleteType.ToLower() == CustomerEnum.DeleteType.RemoveDealer.ToString().ToLower()
							? childCustomerUID
							: parentCustomerUID,
					RowUpdatedUTC = DateTime.UtcNow
				};

				var actions = new List<Action>
				{
					() => transaction.Upsert(accountobj),
					() => transaction.Publish(message)
				};
				return transaction.Execute(actions);
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while creating account customer relationship : {ex.Message}, {ex.StackTrace}");
				throw;
			}
		}

		public bool DeleteAccountCustomerRelationShip(Guid parentCustomerUID, Guid childCustomerUID,
													DbAccount accountDetails, DateTime actionUTC)
		{
			try
			{
				var accountMessage = new KafkaMessage
				{
					Key = accountDetails.CustomerAccountUID.ToString(),
					Topic = AccountTopic,
					Message = new
					{
						AccountEvent = new AccountEvent()
						{
							AccountName = accountDetails.AccountName,
							AccountUID = accountDetails.CustomerAccountUID,
							Action = Operation.Delete.ToString(),
							BSSID = accountDetails.BSSID,
							DealerAccountCode = accountDetails.DealerAccountCode,
							NetworkCustomerCode = accountDetails.NetworkCustomerCode,
							fk_ParentCustomerUID = parentCustomerUID,
							fk_ChildCustomerUID = childCustomerUID,
							ActionUTC = actionUTC,
							ReceivedUTC = DateTime.UtcNow
						}
					}
				};

				var accountobj = new DbAccount
				{
					CustomerAccountID = accountDetails.CustomerAccountID,
					CustomerAccountUID = accountDetails.CustomerAccountUID,
					BSSID = accountDetails.BSSID,
					AccountName = accountDetails.AccountName,
					NetworkCustomerCode = accountDetails.NetworkCustomerCode,
					DealerAccountCode = accountDetails.DealerAccountCode,
					fk_ChildCustomerUID = null,
					fk_ParentCustomerUID = null,
					RowUpdatedUTC = DateTime.UtcNow
				};

				var actions = new List<Action>
				{
					() => transaction.Upsert(accountobj),
					() => transaction.Publish(accountMessage)
				};
				return transaction.Execute(actions);
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while deleting account customer relationship : {ex.Message}, {ex.StackTrace}");
				throw;
			}
		}
	}
}