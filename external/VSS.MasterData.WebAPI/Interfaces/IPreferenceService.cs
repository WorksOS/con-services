using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.KafkaModel;

namespace VSS.MasterData.WebAPI.Interfaces
{
	public interface IPreferenceService
	{
		bool DoesUserPreferenceExist(Guid userUID, Guid? preferenceKeyGuid, string preferenceKeyName);

		PreferenceKeyDto GetPreferenceKey(Guid? preferenceKeyGuid, string preferenceKeyName);

		bool? CreateUserPreference(CreateUserPreferenceEvent userPreference);

		bool? UpdateUserPreference(UpdateUserPreferenceEvent userPreference);

		bool? DeleteUserPreference(DeleteUserPreferenceEvent userPreference);

		List<UserPreference> GetUserPreferencesForUser(Guid userUID, string version, string keyName);

		bool? CreatePreferenceKey(CreatePreferenceKeyEvent preferenceKey);

		bool? UpdatePreferenceKey(UpdatePreferenceKeyEvent preferenceKey);

		bool? DeletePreferenceKey(DeletePreferenceKeyEvent preferenceKey);
	}
}