using System;
using System.Collections.Generic;
using KafkaModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.KafkaModel;
using VSS.MasterData.WebAPI.Transactions;
using VSS.MasterData.WebAPI.Utilities.Helpers;
using VSS.MasterData.WebAPI.Utilities.Extensions;
using Xunit;

namespace VSS.MasterData.WebAPI.PreferenceRepository.Tests
{
	public class PreferenceServiceTests
	{
		private readonly IConfiguration configuration;
		private readonly ILogger logger;
		private readonly ITransactions transaction;
		private IPreferenceService preferenceService;

		public PreferenceServiceTests()
		{
			configuration = Substitute.For<IConfiguration>();
			logger = Substitute.For<ILogger>();
			transaction = Substitute.For<ITransactions>();
			configuration["PreferenceKafkaTopicNames"] = "VSS.Interfaces.Events.Preference.IPreferenceEvent";
			configuration["TopicSuffix"] = "-Test";
			preferenceService = new PreferenceService(configuration, logger, transaction);
			string[] topics = configuration["PreferenceKafkaTopicNames"].Split(',');
			transaction.Execute(Arg.Any<List<Action>>()).Returns(x =>
			{
				foreach (var action in x.Arg<List<Action>>())
				{
					action();
				}
				return true;
			});
		}

		#region Create UserPrefernce
		[Fact]
		public void TestCreateUserPreference_ValidInput_True()
		{
			//Arrange
			CreateUserPreferenceEvent createUser = new CreateUserPreferenceEvent()
			{
				PreferenceKeyUID = new Guid("2ccfe195-81ec-4aed-ba31-fc7d794cb75f"),
				UserUID = new Guid("85293f9c-b713-4e95-a5ed-b36beb2c4aa2"),
				PreferenceKeyName = "Key",
				PreferenceJson = "Json",
				SchemaVersion = "v1",
				ActionUTC = DateTime.UtcNow
			};
			transaction.Get<PreferenceKeyDto>(Arg.Any<string>()).Returns(x => { return new List<PreferenceKeyDto>() { new PreferenceKeyDto() { PreferenceKeyID = 10, PreferenceKeyName = "Key", PreferenceKeyUID = createUser.PreferenceKeyUID.Value } }; });

			//Act
			bool? result = preferenceService.CreateUserPreference(createUser);

			//Assert
			Assert.True(result);
			transaction.Received(1).Upsert<DbUserPreference>(Arg.Is<DbUserPreference>(x => x.fk_UserUID == createUser.UserUID));
			transaction.Received(1).Publish(Arg.Is<KafkaMessage>(x => x.Key == createUser.UserUID.ToString()));
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
		}

		[Fact]
		public void TestCreateUserPreference_ValidInputWithoutSchema_True()
		{
			//Arrange
			CreateUserPreferenceEvent createUser = new CreateUserPreferenceEvent()
			{
				PreferenceKeyUID = new Guid("2ccfe195-81ec-4aed-ba31-fc7d794cb75f"),
				UserUID = new Guid("fd4b9a47-4b67-40b9-b106-89047c88fa43"),
				PreferenceKeyName = "Key",
				PreferenceJson = "Json",
				ActionUTC = DateTime.UtcNow
			};
			transaction.Get<PreferenceKeyDto>(Arg.Any<string>()).Returns(x => { return new List<PreferenceKeyDto>() { new PreferenceKeyDto() { PreferenceKeyID = 9, PreferenceKeyName = "Key" ,PreferenceKeyUID=createUser.PreferenceKeyUID.Value} }; });

			//Act
			bool? result = preferenceService.CreateUserPreference(createUser);

			//Assert
			Assert.True(result);
			transaction.Received(1).Upsert<DbUserPreference>(Arg.Is<DbUserPreference>(x => (x.fk_UserUID == createUser.UserUID && x.SchemaVersion == "1.0")));
			transaction.Received(1).Publish(Arg.Is<KafkaMessage>(x => x.Key == createUser.UserUID.ToString()));
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
		}

		[Fact]
		public void TestCreateUserPreference_InvalidPreference_ReturnNull()
		{
			//Arrange
			CreateUserPreferenceEvent createUser = new CreateUserPreferenceEvent()
			{
				PreferenceKeyUID = new Guid("2ccfe195-81ec-4aed-ba31-fc7d794cb75f"),
				UserUID = new Guid("fd4b9a47-4b67-40b9-b106-89047c88fa43"),
				PreferenceKeyName = "Key",
				PreferenceJson = "Json",
				ActionUTC = DateTime.UtcNow
			};
			transaction.Get<PreferenceKeyDto>(Arg.Any<string>()).Returns(x => { return new List<PreferenceKeyDto>(); });

			//Act
			bool? result = preferenceService.CreateUserPreference(createUser);

			//Assert
			Assert.Null(result);
			transaction.Received(1).Get<PreferenceKeyDto>(Arg.Any<string>());
			transaction.Received(0).Upsert<DbUserPreference>(Arg.Any<DbUserPreference>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
			transaction.Received(0).Execute(Arg.Any<List<Action>>());
		}
		#endregion

		#region Update UserPreference
		[Fact]
		public void TestUpdateUserPreference_ValidInput_True()
		{
			//Arrange
			UpdateUserPreferenceEvent updateUser = new UpdateUserPreferenceEvent()
			{
				PreferenceKeyUID = new Guid("194a92e4-7ec9-45b6-b34d-c62bde33b381"),
				PreferenceKeyName = "Key",
				UserUID = new Guid("fd4b9a47-4b67-40b9-b106-89047c88fa43"),
				PreferenceJson = "Json",
				SchemaVersion = "2.0",
				ActionUTC = DateTime.UtcNow
			};
			var dbUserPreference = new DbUserPreference()
			{
				fk_PreferenceKeyID = 10,
				fk_UserUID = updateUser.UserUID,
				PreferenceValue = "Json",
				SchemaVersion = "1.0",
				InsertUTC = DateTime.UtcNow.AddDays(-1),
				UpdateUTC = DateTime.UtcNow
			};
			transaction.Get<DbUserPreference>(Arg.Any<string>()).Returns(x => { return new List<DbUserPreference>() { dbUserPreference }; });
			transaction.Get<PreferenceKeyDto>(Arg.Any<string>()).Returns(x => { return new List<PreferenceKeyDto>() { new PreferenceKeyDto() { PreferenceKeyName = "Key", PreferenceKeyUID = updateUser.PreferenceKeyUID.Value } }; });

			//Act
			bool? result = preferenceService.UpdateUserPreference(updateUser);

			//Assert
			Assert.True(result);
			transaction.Received(1).Get<PreferenceKeyDto>(Arg.Any<string>());
			transaction.Received(1).Get<DbUserPreference>(Arg.Any<string>());
			transaction.Received(1).Upsert(Arg.Is<DbUserPreference>(x => x.fk_UserUID == updateUser.UserUID && x.SchemaVersion == updateUser.SchemaVersion && x.PreferenceValue == updateUser.PreferenceJson));
			transaction.Received(1).Publish(Arg.Is<KafkaMessage>(x => x.Key == updateUser.UserUID.ToString()));
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
		}

		[Fact]
		public void TestUpdateUserPreference_ValidInputWithoutSchema_True()
		{
			//Arrange
			UpdateUserPreferenceEvent updateUser = new UpdateUserPreferenceEvent()
			{
				PreferenceKeyUID = new Guid("194a92e4-7ec9-45b6-b34d-c62bde33b381"),
				UserUID = new Guid("fd4b9a47-4b67-40b9-b106-89047c88fa43"),
				PreferenceKeyName = "Key",
				PreferenceJson = "Json",
				ActionUTC = DateTime.UtcNow
			};
			var dbUserPreference = new DbUserPreference()
			{
				fk_PreferenceKeyID = 10,
				fk_UserUID = updateUser.UserUID,
				PreferenceValue = "Json",
				SchemaVersion = "2.0",
				InsertUTC = DateTime.UtcNow.AddDays(-1),
				UpdateUTC = DateTime.UtcNow
			};
			transaction.Get<PreferenceKeyDto>(Arg.Any<string>()).Returns(x => { return new List<PreferenceKeyDto>() { new PreferenceKeyDto() { PreferenceKeyName="Key",PreferenceKeyUID=updateUser.PreferenceKeyUID.Value} }; });
			transaction.Get<DbUserPreference>(Arg.Any<string>()).Returns(x => { return new List<DbUserPreference>() { dbUserPreference }; });

			//Act
			bool? result = preferenceService.UpdateUserPreference(updateUser);

			//Assert
			Assert.True(result);
			transaction.Received(1).Get<PreferenceKeyDto>(Arg.Any<string>());
			transaction.Received(1).Upsert(Arg.Is<DbUserPreference>(x => (x.fk_UserUID == updateUser.UserUID && x.PreferenceValue == updateUser.PreferenceJson && x.SchemaVersion == dbUserPreference.SchemaVersion)));
			transaction.Received(1).Publish(Arg.Is<KafkaMessage>(x => x.Key == updateUser.UserUID.ToString()));
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
		}

		[Fact]
		public void TestUpdateUserPreference_InvalidPreference_ReturnNull()
		{
			//Arrange
			UpdateUserPreferenceEvent updateUser = new UpdateUserPreferenceEvent()
			{
				PreferenceKeyUID = new Guid("194a92e4-7ec9-45b6-b34d-c62bde33b381"),
				UserUID = new Guid("fd4b9a47-4b67-40b9-b106-89047c88fa43"),
				PreferenceKeyName = "Key",
				PreferenceJson = "Json",
				ActionUTC = DateTime.UtcNow
			};
			transaction.Get<PreferenceKeyDto>(Arg.Any<string>()).Returns(x => { return new List<PreferenceKeyDto>() { }; });
			//Act
			bool? result = preferenceService.UpdateUserPreference(updateUser);

			//Assert
			Assert.Null(result);
			transaction.Received(1).Get<PreferenceKeyDto>(Arg.Any<string>());
			transaction.Received(0).Upsert<DbUserPreference>(Arg.Any<DbUserPreference>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
			transaction.Received(0).Execute(Arg.Any<List<Action>>());
		}
		#endregion

		#region Delete UserPreference
		[Fact]
		public void TestDeleteUserPreference_ValidInput_True()
		{
			//Arrange
			DeleteUserPreferenceEvent deleteUser = new DeleteUserPreferenceEvent()
			{
				PreferenceKeyUID = new Guid("85293f9c-b713-4e95-a5ed-b36beb2c4aa2"),
				PreferenceKeyName = "Key",
				UserUID = new Guid("fd4b9a47-4b67-40b9-b106-89047c88fa43"),
				ActionUTC = DateTime.UtcNow
			};
			var dbUserPreference = new DbUserPreference()
			{
				fk_PreferenceKeyID = 10,
				fk_UserUID = deleteUser.UserUID,
				PreferenceValue = "Json",
				SchemaVersion = "1.0",
				InsertUTC = DateTime.UtcNow.AddDays(-1),
				UpdateUTC = DateTime.UtcNow
			};
			transaction.Get<DbUserPreference>(Arg.Any<string>()).Returns(x => { return new List<DbUserPreference>() { dbUserPreference }; });

			//Act
			bool? result = preferenceService.DeleteUserPreference(deleteUser);

			//Assert
			Assert.True(result);
			transaction.Received(1).Get<DbUserPreference>(Arg.Any<string>());
			transaction.Received(1).Delete(Arg.Is<string>(x => x.Contains(deleteUser.UserUID.ToStringAndWrapWithUnhex()) && x.Contains(" = " + dbUserPreference.fk_PreferenceKeyID)));
			transaction.Received(1).Publish(Arg.Is<KafkaMessage>(x => x.Key == deleteUser.UserUID.ToString()));
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
		}

		[Fact]
		public void TestDeleteUserPreference_PreferenceNotExist_ReturnNull()
		{
			//Arrange
			DeleteUserPreferenceEvent deleteUser = new DeleteUserPreferenceEvent()
			{
				PreferenceKeyUID = new Guid("85293f9c-b713-4e95-a5ed-b36beb2c4aa2"),
				PreferenceKeyName = "Key",
				UserUID = new Guid("fd4b9a47-4b67-40b9-b106-89047c88fa43"),
				ActionUTC = DateTime.UtcNow
			};
			transaction.Get<DbUserPreference>(Arg.Any<string>()).Returns(x => { return new List<DbUserPreference>(); });

			//Act
			bool? result = preferenceService.DeleteUserPreference(deleteUser);

			//Assert
			Assert.Null(result);
			transaction.Received(1).Get<DbUserPreference>(Arg.Any<string>());
			transaction.Received(0).Delete(Arg.Any<string>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
			transaction.Received(0).Execute(Arg.Any<List<Action>>());
		}
		#endregion

		#region Create PreferenceKey
		[Fact]
		public void TestCreatePreferenceKey_ValidInput_True()
		{
			//Arrange
			CreatePreferenceKeyEvent createPreference = new CreatePreferenceKeyEvent()
			{
				PreferenceKeyUID = new Guid("2ae9019e-b840-4121-8639-366eecac91c1"),
				PreferenceKeyName = "Key",
				ActionUTC = DateTime.UtcNow
			};
			transaction.Get<PreferenceKeyDto>(Arg.Any<string>()).Returns(x => { return new List<PreferenceKeyDto>() { null }; });

			//Act
			bool? result = preferenceService.CreatePreferenceKey(createPreference);

			//Assert
			Assert.True(result);
			transaction.Received(1).Upsert(Arg.Is<DbPreferenceKey>(x => x.PreferenceKeyUID == createPreference.PreferenceKeyUID && x.PreferenceKeyName == createPreference.PreferenceKeyName));
			transaction.Received(1).Publish(Arg.Is<KafkaMessage>(x => x.Key == createPreference.PreferenceKeyUID.ToString()));
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
		}
		#endregion

		#region Update PreferenceKey
		[Fact]
		public void TestUpdatePreferenceKey_ValidInput_True()
		{
			//Arrange
			UpdatePreferenceKeyEvent updatePreference = new UpdatePreferenceKeyEvent()
			{
				PreferenceKeyUID = new Guid("2ae9019e-b840-4121-8639-366eecac91c1"),
				PreferenceKeyName = "Key",
				ActionUTC = DateTime.UtcNow
			};
			var dbPreferenceKey = new PreferenceKeyDto()
			{
				PreferenceKeyUID = new Guid("2ae9019e-b840-4121-8639-366eecac91c1"),
				PreferenceKeyName = "Key",
				InsertUTC = DateTime.UtcNow.AddDays(-1),
				UpdateUTC = DateTime.UtcNow
			};
			transaction.Get<PreferenceKeyDto>(Arg.Is<string>(x=>x.Contains("PreferenceKeyUID"))).Returns(x => { return new List<PreferenceKeyDto>() { dbPreferenceKey }; });

			//Act
			bool? result = preferenceService.UpdatePreferenceKey(updatePreference);

			//Assert
			Assert.True(result);
			transaction.Received(2).Get<PreferenceKeyDto>(Arg.Any<string>());
			transaction.Received(1).Upsert(Arg.Is<DbPreferenceKey>(x => x.PreferenceKeyUID == updatePreference.PreferenceKeyUID && x.PreferenceKeyName == updatePreference.PreferenceKeyName));
			transaction.Received(1).Publish(Arg.Is<KafkaMessage>(x => x.Key == updatePreference.PreferenceKeyUID.ToString()));
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
		}

		[Fact]
		public void TestUpdatePreferenceKey_InvalidPreference_ReturnFalse()
		{
			//Arrange
			UpdatePreferenceKeyEvent updatePreference = new UpdatePreferenceKeyEvent()
			{
				PreferenceKeyUID = new Guid("2ae9019e-b840-4121-8639-366eecac91c1"),
				PreferenceKeyName = "Key",
				ActionUTC = DateTime.UtcNow
			};
			transaction.Get<PreferenceKeyDto>(Arg.Any<string>()).Returns(x => { return new List<PreferenceKeyDto>(); });

			//Act
			bool? result = preferenceService.UpdatePreferenceKey(updatePreference);

			//Assert
			Assert.Null(result);
			transaction.Received(1).Get<PreferenceKeyDto>(Arg.Any<string>());
			transaction.Received(0).Upsert<DbPreferenceKey>(Arg.Any<DbPreferenceKey>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
			transaction.Received(0).Execute(Arg.Any<List<Action>>());
		}
		#endregion

		#region Delete PreferenceKey
		[Fact]
		public void TestDeletePreferenceKey_ValidInput_True()
		{
			//Arrange
			DeletePreferenceKeyEvent deleteUser = new DeletePreferenceKeyEvent()
			{
				PreferenceKeyUID = new Guid("1762f4cd-1322-4be7-96b7-bb4f8b8ff510"),
				ActionUTC = DateTime.UtcNow
			};
			var dbPreferenceKey = new PreferenceKeyDto()
			{
				PreferenceKeyUID = deleteUser.PreferenceKeyUID,
				PreferenceKeyName = "Key",
				PreferenceKeyID = 10,
				InsertUTC = DateTime.UtcNow.AddDays(-1),
				UpdateUTC = DateTime.UtcNow
			};
			transaction.Get<PreferenceKeyDto>(Arg.Any<string>()).Returns(x => { return new List<PreferenceKeyDto>() { dbPreferenceKey }; });

			//Act
			bool? result = preferenceService.DeletePreferenceKey(deleteUser);

			//Assert
			Assert.True(result);
			transaction.Received(1).Get<PreferenceKeyDto>(Arg.Any<string>());
			transaction.Received(1).Delete(Arg.Is<string>(x => x.Contains(deleteUser.PreferenceKeyUID.ToStringAndWrapWithUnhex())));
			transaction.Received(1).Publish(Arg.Is<KafkaMessage>(x => x.Key == deleteUser.PreferenceKeyUID.ToString()));
			transaction.Received(1).Execute(Arg.Any<List<Action>>());
		}

		[Fact]
		public void TestDeletePreferenceKey_PreferenceNotExist_ReturnNull()
		{
			//Arrange
			DeletePreferenceKeyEvent deleteUser = new DeletePreferenceKeyEvent()
			{
				PreferenceKeyUID = new Guid("1762f4cd-1322-4be7-96b7-bb4f8b8ff510"),
				ActionUTC = DateTime.UtcNow
			};
			transaction.Get<PreferenceKeyDto>(Arg.Any<string>()).Returns(x => { return new List<PreferenceKeyDto>(); });

			//Act
			bool? result = preferenceService.DeletePreferenceKey(deleteUser);

			//Assert
			Assert.Null(result);
			transaction.Received(1).Get<PreferenceKeyDto>(Arg.Any<string>());
			transaction.Received(0).Delete(Arg.Any<string>());
			transaction.Received(0).Publish(Arg.Any<KafkaMessage>());
			transaction.Received(0).Execute(Arg.Any<List<Action>>());
		}
		#endregion

		#region GetPreferenceKey
		[Fact]
		public void TestGetPreferenceKey_ValidInput_ReturnsPreference()
		{
			//Arrange
			transaction.Get<PreferenceKeyDto>(Arg.Any<string>()).Returns(x => { return new List<PreferenceKeyDto>() { new PreferenceKeyDto() { PreferenceKeyID = 7 } }; });

			//Act
			PreferenceKeyDto preferenceKey = preferenceService.GetPreferenceKey(new Guid("85293f9c-b713-4e95-a5ed-b36beb2c4aa2"), "Key");

			//Assert
			Assert.Equal(7, preferenceKey.PreferenceKeyID);
		}

		[Fact]
		public void TestGetPreferenceKey_ValidInputWithoutGuid_ReturnsPreference()
		{
			//Arrange
			transaction.Get<PreferenceKeyDto>(Arg.Any<string>()).Returns(x => { return new List<PreferenceKeyDto>() { new PreferenceKeyDto() { PreferenceKeyID = 7 } }; });

			//Act
			PreferenceKeyDto preferenceKey = preferenceService.GetPreferenceKey(null, "Key");

			//Assert
			Assert.Equal(7, preferenceKey.PreferenceKeyID);
			transaction.Received(1).Get<PreferenceKeyDto>(Arg.Is<string>(x => !x.Contains("PreferenceKeyUID") && x.Contains("PreferenceKeyName")));
		}

		[Fact]
		public void TestGetPreferenceKey_ValidInputWithoutKeyName_ReturnsPreference()
		{
			//Arrange
			transaction.Get<PreferenceKeyDto>(Arg.Any<string>()).Returns(x => { return new List<PreferenceKeyDto>() { new PreferenceKeyDto() { PreferenceKeyID = 7 } }; });

			//Act
			PreferenceKeyDto preferenceKey = preferenceService.GetPreferenceKey(new Guid("40c61e39-60f3-4bf6-a2ee-846b595103a4"), null);

			//Assert
			Assert.Equal(7, preferenceKey.PreferenceKeyID);
			transaction.Received(1).Get<PreferenceKeyDto>(Arg.Is<string>(x => x.Contains("PreferenceKeyUID") && !x.Contains("PreferenceKeyName")));
		}
		#endregion

		#region GetUserPreferencesForUser
		[Fact]
		public void TestGetUserPreferencesForUser_ValidInput_ReturnsPreferences()
		{
			//Arrange
			Guid userUid = new Guid("e96d6ded-826e-4cfc-a04e-a07963ba2a80");
			string keyName = "Key1";
			string version = "1.0";
			List<UserPreference> userPreferenceList = new List<UserPreference>
			{
				new UserPreference { PreferenceJson = "JSON1", PreferenceKeyName = "Key1", SchemaVersion = "1.0" },
			};
			transaction.Get<UserPreference>(Arg.Any<string>()).Returns(userPreferenceList);

			//Act
			var resultList = preferenceService.GetUserPreferencesForUser(userUid, keyName, version);


			//Assert
			Assert.Equal(userPreferenceList, resultList);
			transaction.Received(1).Get<UserPreference>(Arg.Is<string>(x => x.Contains(keyName) && x.Contains(version)));
		}

		[Fact]
		public void TestGetUserPreferencesForUser_ValidInputWithoutKeyName_ReturnsPreferences()
		{
			//Arrange
			Guid userUid = new Guid("e96d6ded-826e-4cfc-a04e-a07963ba2a80");
			string keyName = null;
			string version = "1.0";
			List<UserPreference> userPreferenceList = new List<UserPreference>
			{
				new UserPreference { PreferenceJson = "JSON1", PreferenceKeyName = "Key1", SchemaVersion = "1.0" },
				new UserPreference { PreferenceJson = "JSON2", PreferenceKeyName = "Key2", SchemaVersion = "1.0" }
			};
			transaction.Get<UserPreference>(Arg.Any<string>()).Returns(userPreferenceList);

			//Act
			var resultList = preferenceService.GetUserPreferencesForUser(userUid, keyName, version);


			//Assert
			Assert.Equal(userPreferenceList, resultList);
			transaction.Received(1).Get<UserPreference>(Arg.Is<string>(x => x.Contains(version)));
		}

		[Fact]
		public void TestGetUserPreferencesForUser_ValidInputWithoutVersion_ReturnsPreferences()
		{
			//Arrange
			Guid userUid = new Guid("e96d6ded-826e-4cfc-a04e-a07963ba2a80");
			string keyName = "Key1";
			string version = null;
			List<UserPreference> userPreferenceList = new List<UserPreference>
			{
				new UserPreference { PreferenceJson = "JSON1", PreferenceKeyName = "Key1", SchemaVersion = "1.0" },
				new UserPreference { PreferenceJson = "JSON2", PreferenceKeyName = "Key2", SchemaVersion = "1.0" }
			};
			transaction.Get<UserPreference>(Arg.Any<string>()).Returns(userPreferenceList);

			//Act
			var resultList = preferenceService.GetUserPreferencesForUser(userUid, keyName, version);


			//Assert
			Assert.Equal(userPreferenceList, resultList);
			transaction.Received(1).Get<UserPreference>(Arg.Is<string>(x => x.Contains(keyName)));
		}

		[Fact]
		public void TestGetUserPreferencesForUser_ValidInputWithoutVersionAndKeyName_ReturnsPreferences()
		{
			//Arrange
			Guid userUid = new Guid("e96d6ded-826e-4cfc-a04e-a07963ba2a80");
			string keyName = null;
			string version = null;
			List<UserPreference> userPreferenceList = new List<UserPreference>
			{
				new UserPreference { PreferenceJson = "JSON1", PreferenceKeyName = "Key1", SchemaVersion = "1.0" },
				new UserPreference { PreferenceJson = "JSON2", PreferenceKeyName = "Key2", SchemaVersion = "1.0" }
			};
			transaction.Get<UserPreference>(Arg.Any<string>()).Returns(userPreferenceList);

			//Act
			var resultList = preferenceService.GetUserPreferencesForUser(userUid, keyName, version);


			//Assert
			Assert.Equal(userPreferenceList, resultList);
			transaction.Received(1).Get<UserPreference>(Arg.Is<string>(x => !x.Contains(" and up.SchemaVersion") && !x.Contains(" and upk.PreferenceKeyName")));
		}
		#endregion
	}
}