using KafkaModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.KafkaModel;
using VSS.MasterData.WebAPI.Transactions;
using VSS.MasterData.WebAPI.Utilities.Extensions;

namespace VSS.MasterData.WebAPI.PreferenceRepository
{
	public class PreferenceService : IPreferenceService
	{
		private readonly IConfiguration configuration;
		private readonly ILogger logger;
		private readonly ITransactions transaction;

		private string[] topics;

		public PreferenceService(IConfiguration configuration, ILogger logger, ITransactions transaction)
		{
			this.configuration = configuration;
			this.logger = logger;
			this.transaction = transaction;
			topics = configuration["PreferenceKafkaTopicNames"].Split(',').Select(x => x + configuration["TopicSuffix"])
				.ToArray();
		}

		public bool? CreateUserPreference(CreateUserPreferenceEvent userPreference)
		{
			var preference = GetPreferenceKey(userPreference.PreferenceKeyUID, userPreference.PreferenceKeyName);

			if (preference == null || userPreference.PreferenceKeyUID.HasValue &&
				!preference.PreferenceKeyUID.Equals(userPreference.PreferenceKeyUID))
			{
				return null;
			}

			if (string.IsNullOrEmpty(userPreference.SchemaVersion))
			{
				userPreference.SchemaVersion = "1.0";
			}

			var currentUtc = DateTime.UtcNow;
			var insertUserPreference = new DbUserPreference()
			{
				fk_UserUID = userPreference.UserUID,
				fk_PreferenceKeyID = preference.PreferenceKeyID,
				PreferenceValue = userPreference.PreferenceJson,
				SchemaVersion = userPreference.SchemaVersion,
				InsertUTC = currentUtc,
				UpdateUTC = currentUtc
			};

			var kafkaMessage = new KafkaMessage()
			{
				Key = userPreference.UserUID.ToString(),
				Message = new { CreateUserPreferenceEvent = userPreference }
			};

			var actions = new List<Action>();
			actions.Add(() => transaction.Upsert<DbUserPreference>(insertUserPreference));
			actions.Add(() => topics.ToList().ForEach(topic =>
			{
				kafkaMessage.Topic = topic;
				transaction.Publish(kafkaMessage);
			}));

			return transaction.Execute(actions);
		}

		public bool? UpdateUserPreference(UpdateUserPreferenceEvent userPreference)
		{
			var preference = GetPreferenceKey(userPreference.PreferenceKeyUID, userPreference.PreferenceKeyName);

			if (preference == null || userPreference.PreferenceKeyUID.HasValue &&
				!preference.PreferenceKeyUID.Equals(userPreference.PreferenceKeyUID))
			{
				return null;
			}

			var dbUserPreference = GetUserPreference(userPreference.UserUID.Value, userPreference.PreferenceKeyUID,
				userPreference.PreferenceKeyName);
			if (dbUserPreference == null)
			{
				throw new Exception("UserPreference does not Exist");
			}

			dbUserPreference.PreferenceValue = userPreference.PreferenceJson;
			if (userPreference.SchemaVersion != null)
			{
				dbUserPreference.SchemaVersion = userPreference.SchemaVersion;
			}
			else
			{
				userPreference.SchemaVersion = dbUserPreference.SchemaVersion;
			}

			dbUserPreference.UpdateUTC = DateTime.UtcNow;

			var kafkaMessage = new KafkaMessage()
			{
				Key = userPreference.UserUID.ToString(),
				Message = new { UpdateUserPreferenceEvent = userPreference }
			};

			var actions = new List<Action>();
			actions.Add(() => transaction.Upsert<DbUserPreference>(dbUserPreference));
			actions.Add(() => topics.ToList().ForEach(topic =>
			{
				kafkaMessage.Topic = topic;
				transaction.Publish(kafkaMessage);
			}));

			return transaction.Execute(actions);
		}

		public bool? DeleteUserPreference(DeleteUserPreferenceEvent userPreference)
		{
			var dbUserPreference = GetUserPreference(userPreference.UserUID, userPreference.PreferenceKeyUID,
				userPreference.PreferenceKeyName);
			if (dbUserPreference == null)
			{
				return null;
			}

			var deleteQuery =
				string.Format(
					"Delete from md_preference_PreferenceUser where fk_UserUID = {0} and fk_PreferenceKeyID = {1}",
					userPreference.UserUID.ToStringAndWrapWithUnhex(), dbUserPreference.fk_PreferenceKeyID);


			var kafkaMessage = new KafkaMessage()
			{
				Key = userPreference.UserUID.ToString(),
				Message = userPreference
			};

			var actions = new List<Action>();
			actions.Add(() => transaction.Delete(deleteQuery));
			actions.Add(() => topics.ToList().ForEach(topic =>
			{
				kafkaMessage.Topic = topic;
				transaction.Publish(kafkaMessage);
			}));

			return transaction.Execute(actions);
		}

		public bool? CreatePreferenceKey(CreatePreferenceKeyEvent preferenceKey)
		{
			if (GetPreferenceKey(preferenceKey.PreferenceKeyUID) != null ||
				GetPreferenceKey(null, preferenceKey.PreferenceKeyName) != null)
			{
				return null;
			}

			var currentUtc = DateTime.UtcNow;
			var createPreferenceKey = new DbPreferenceKey()
			{
				PreferenceKeyName = preferenceKey.PreferenceKeyName,
				PreferenceKeyUID = preferenceKey.PreferenceKeyUID.Value,
				InsertUTC = currentUtc,
				UpdateUTC = currentUtc
			};
			var kafkaMessage = new KafkaMessage()
			{
				Key = preferenceKey.PreferenceKeyUID.ToString(),
				Message = new { CreateUserPreferenceKeyEvent = preferenceKey }
			};
			var actions = new List<Action>();
			actions.Add(() => transaction.Upsert<DbPreferenceKey>(createPreferenceKey));
			actions.Add(() => topics.ToList().ForEach(topic =>
			{
				kafkaMessage.Topic = topic;
				transaction.Publish(kafkaMessage);
			}));

			return transaction.Execute(actions);
		}

		public bool? UpdatePreferenceKey(UpdatePreferenceKeyEvent preferenceKey)
		{
			var preference = GetPreferenceKey(preferenceKey.PreferenceKeyUID);

			if (preference == null)
			{
				return null;
			}

			if (GetPreferenceKey(null, preferenceKey.PreferenceKeyName) != null)
			{
				throw new Exception("PreferenceKey Name Already Exist");
			}

			var updatePreferenceKey = new DbPreferenceKey()
			{
				PreferenceKeyName = preferenceKey.PreferenceKeyName,
				PreferenceKeyUID = preferenceKey.PreferenceKeyUID.Value,
				UpdateUTC = DateTime.UtcNow
			};
			var kafkaMessage = new KafkaMessage()
			{
				Key = preferenceKey.PreferenceKeyUID.ToString(),
				Message = new { UpdateUserPreferenceKeyEvent = preferenceKey }
			};
			var actions = new List<Action>();
			actions.Add(() => transaction.Upsert<DbPreferenceKey>(updatePreferenceKey));
			actions.Add(() => topics.ToList().ForEach(topic =>
			{
				kafkaMessage.Topic = topic;
				transaction.Publish(kafkaMessage);
			}));

			return transaction.Execute(actions);
		}

		public bool? DeletePreferenceKey(DeletePreferenceKeyEvent preferenceKey)
		{
			try
			{
				var preference = GetPreferenceKey(preferenceKey.PreferenceKeyUID);
				if (preference == null)
				{
					return null;
				}

				var deleteQuery = string.Format("Delete from md_preference_PreferenceKey where PreferenceKeyUID = {0}",
					preferenceKey.PreferenceKeyUID.ToStringAndWrapWithUnhex());

				var kafkaMessage = new KafkaMessage()
				{
					Key = preference.PreferenceKeyUID.ToString(),
					Message = new { DeleteUserPreferenceKey = preferenceKey }
				};

				var actions = new List<Action>();
				actions.Add(() => transaction.Delete(deleteQuery));
				actions.Add(() => topics.ToList().ForEach(topic =>
				{
					kafkaMessage.Topic = topic;
					transaction.Publish(kafkaMessage);
				}));

				return transaction.Execute(actions);
			}
			catch (MySqlException ex)
			{
				if (!ex.Message.Contains("foreign key constraint"))
				{
					throw ex;
				}
			}

			return false;
		}

		public bool DoesUserPreferenceExist(Guid userUID, Guid? preferenceKeyGuid, string preferenceKeyName)
		{
			if (GetUserPreference(userUID, preferenceKeyGuid, preferenceKeyName) != null)
			{
				return true;
			}

			return false;
		}

		public DbUserPreference GetUserPreference(Guid userUID, Guid? preferenceKeyGuid, string preferenceKeyName)
		{
			var isPreferenceExistQuery = string.Format("select *from md_preference_PreferenceUser PU " +
														"inner join md_preference_PreferenceKey PK on PK.PreferenceKeyID = PU.fk_PreferenceKeyID " +
														"and PU.fk_UserUID={0}", userUID.ToStringAndWrapWithUnhex());
			var queryBuilder = new StringBuilder(isPreferenceExistQuery);
			if (!string.IsNullOrEmpty(preferenceKeyName))
			{
				queryBuilder.Append(string.Format(" and PK.PreferenceKeyName = '{0}'", preferenceKeyName));
			}

			if (preferenceKeyGuid.HasValue)
			{
				queryBuilder.Append(string.Format(" and PK.PreferenceKeyUID = {0}",
					preferenceKeyGuid.Value.ToStringAndWrapWithUnhex()));
			}

			isPreferenceExistQuery = queryBuilder.ToString();
			return transaction.Get<DbUserPreference>(isPreferenceExistQuery).FirstOrDefault();
		}

		public List<UserPreference> GetUserPreferencesForUser(Guid userUID, string version, string keyName)
		{
			var basicQuery = string.Format(
				"select PK.PreferenceKeyName as `PreferenceKeyName`, PU.PreferenceValue as `PreferenceJson`, PK.PreferenceKeyUID as `PreferenceKeyUID`, PU.SchemaVersion as `SchemaVersion` from md_preference_PreferenceUser PU " +
				"join md_preference_PreferenceKey PK on PU.fk_PreferenceKeyID = PK.PreferenceKeyID where PU.fk_UserUID ={0}",
				userUID.ToStringAndWrapWithUnhex());
			var queryBuilder = new StringBuilder(basicQuery);
			if (!string.IsNullOrEmpty(version))
			{
				queryBuilder.Append(string.Format(" and PU.SchemaVersion = '{0}'", version));
			}

			if (!string.IsNullOrEmpty(keyName))
			{
				queryBuilder.AppendFormat(string.Format(" and PK.PreferenceKeyName = '{0}'", keyName));
			}

			var userPreferenceList = transaction.Get<UserPreference>(queryBuilder.ToString()).ToList();
			return userPreferenceList;
		}

		public PreferenceKeyDto GetPreferenceKey(Guid? preferenceKeyGuid, string preferenceKeyName = null)
		{
			var getPreferenceKeyQuery = "select *from md_preference_PreferenceKey  where ";
			var queryBuilder = new StringBuilder(getPreferenceKeyQuery);
			if (!string.IsNullOrEmpty(preferenceKeyName))
			{
				queryBuilder.Append(string.Format("PreferenceKeyName = '{0}'", preferenceKeyName));
			}
			else if (preferenceKeyGuid.HasValue)
			{
				queryBuilder.Append(string.Format("PreferenceKeyUID = {0}",
					preferenceKeyGuid.Value.ToStringAndWrapWithUnhex()));
			}

			getPreferenceKeyQuery = queryBuilder.ToString();
			return transaction.Get<PreferenceKeyDto>(getPreferenceKeyQuery).FirstOrDefault();
		}
	}
}