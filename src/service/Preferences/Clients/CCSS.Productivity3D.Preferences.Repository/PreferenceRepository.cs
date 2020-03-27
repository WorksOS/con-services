using System.Threading.Tasks;
using CCSS.Productivity3D.Preferences.Abstractions.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.Preference;
using VSS.VisionLink.Interfaces.Events.Preference.Interfaces;
using PrefKeyDataModel = CCSS.Productivity3D.Preferences.Abstractions.Models.Database.PreferenceKey;
using UserPrefDataModel = CCSS.Productivity3D.Preferences.Abstractions.Models.Database.UserPreference;
using System.Linq;
using System;
using CCSS.Productivity3D.Preferences.Abstractions.Models.Database;

namespace CCSS.Productivity3D.Preferences.Repository
{
  public class PreferenceRepository : RepositoryBase, IRepository<IPreferenceEvent>, IPreferenceRepository
  {
    public PreferenceRepository(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore,
      logger)
    {
      Log = logger.CreateLogger<PreferenceRepository>();
    }

    #region preference-store
    public async Task<int> StoreEvent(IPreferenceEvent evt)
    {
      var upsertedCount = 0;
      if (evt == null)
      {
        Log.LogWarning("Unsupported preference event type");
        return 0;
      }

      Log.LogDebug($"Event type is {evt.GetType()}");
      if (evt is CreatePreferenceKeyEvent)
      {
        var prefEvent = (CreatePreferenceKeyEvent)evt;
        var prefKey = new PrefKeyDataModel
        {
          PreferenceKeyUID = prefEvent.PreferenceKeyUID.ToString(),
          KeyName = prefEvent.PreferenceKeyName,
        };
        upsertedCount = await UpsertPreferenceKeyDetail(prefKey, PreferenceEventType.CreatePreferenceKeyEvent);
      }
      else if (evt is UpdatePreferenceKeyEvent)
      {

      }
      else if (evt is DeletePreferenceKeyEvent)
      {

      }
      else if (evt is CreateUserPreferenceEvent)
      {
        var prefEvent = (CreateUserPreferenceEvent)evt;
        upsertedCount = await StoreUserPreference(PreferenceEventType.CreateUserPreferenceEvent, prefEvent.PreferenceKeyUID, prefEvent.PreferenceKeyName,
          prefEvent.UserUID.ToString(), prefEvent.SchemaVersion, prefEvent.PreferenceJson);
      }
      else if (evt is UpdateUserPreferenceEvent)
      {
        var prefEvent = (UpdateUserPreferenceEvent)evt;
        upsertedCount = await StoreUserPreference(PreferenceEventType.UpdateUserPreferenceEvent, prefEvent.PreferenceKeyUID, prefEvent.PreferenceKeyName,
          prefEvent.UserUID.ToString(), prefEvent.SchemaVersion, prefEvent.PreferenceJson);
      }
      else if (evt is DeleteUserPreferenceEvent)
      {
        var prefEvent = (DeleteUserPreferenceEvent)evt;
        upsertedCount = await StoreUserPreference(PreferenceEventType.DeleteUserPreferenceEvent, prefEvent.PreferenceKeyUID, prefEvent.PreferenceKeyName,
          prefEvent.UserUID.ToString());
      }
      
      return upsertedCount;
    }

    private async Task<int> StoreUserPreference(PreferenceEventType eventType, Guid? prefKeyUID, string prefKeyName, string userUID, string schemaVersion=null, string prefJson=null)
    {
      if (!prefKeyUID.HasValue || string.IsNullOrEmpty(prefKeyName))
      {
        Log.LogWarning("Missing PreferenceKeyUID or PreferenceKeyName in user preference create, update or delete");
        return 0;
      }
      var prefKey = await GetPreferenceKey(prefKeyUID.Value, prefKeyName);
      if (prefKey == null) return 0;
      
      var userPref = new UserPrefDataModel
      {
        PreferenceKeyID = prefKey.PreferenceKeyID,
        SchemaVersion = schemaVersion,
        UserUID = userUID,
        PreferenceJson = prefJson
      };
      return await UpsertUserPreference(userPref, eventType);   
    }

    private async Task<int> UpsertUserPreference(UserPrefDataModel userPref, PreferenceEventType eventType)
    {
      var upsertedCount = 0;
      var existing = (await QueryWithAsyncPolicy<UserPrefDataModel>
      (@"SELECT UserPreferenceID, UserUID, fk_PreferenceKeyID, Value, SchemaVersion
              FROM UserPreference
              WHERE fk_PreferenceKeyID = @PreferenceKeyID
                AND UserUID = @UserUID",
        new { userPref.PreferenceKeyID, userPref.UserUID }
      )).FirstOrDefault();

      if (eventType == PreferenceEventType.CreateUserPreferenceEvent)
        upsertedCount = await CreateUserPreference(userPref, existing);

      if (eventType == PreferenceEventType.UpdateUserPreferenceEvent)
        upsertedCount = await UpdateUserPreference(userPref, existing);

      if (eventType == PreferenceEventType.DeleteUserPreferenceEvent)
        upsertedCount = await DeleteUserPreference(userPref, existing);
      return upsertedCount;
    }

    private async Task<int> CreateUserPreference(UserPrefDataModel userPref, UserPrefDataModel existing)
    {
      Log.LogDebug($"PreferenceRepository/CreateUserPreference: user preference={JsonConvert.SerializeObject(userPref)}))')");

      var upsertedCount = 0;
      if (existing == null)
      { 
        var insert = @"INSERT UserPreference
                (UserUID, fk_PreferenceKeyID, Value, SchemaVersion)
                  VALUES
                (@UserUID, @PreferenceKeyID, @PreferenceJson, @SchemaVersion)";

        upsertedCount = await ExecuteWithAsyncPolicy(insert, userPref);
        Log.LogDebug($"PreferenceRepository/CreateUserPreference: inserted {upsertedCount} rows");
      } 
      else
      {
        Log.LogDebug("PreferenceRepository/CreateUserPreference: No action as user preference already exists.");
      }
      
      return upsertedCount;
    }

    private async Task<int> UpdateUserPreference(UserPrefDataModel userPref, UserPrefDataModel existing)
    {
      Log.LogDebug($"PreferenceRepository/UpdateUserPreference: user preference={JsonConvert.SerializeObject(userPref)}))')");

      var upsertedCount = 0;
      if (existing != null)
      {
        var update = $@"UPDATE UserPreference
                SET Value = @PreferenceJson, 
                  SchemaVersion = @SchemaVersion
                WHERE UserPreferenceID = @UserPreferenceID";

        upsertedCount = await ExecuteWithAsyncPolicy(update, userPref);
        Log.LogDebug($"PreferenceRepository/UpdateUserPreference: upserted {upsertedCount} rows");
      }
      else
      {
        Log.LogDebug($"PreferenceRepository/UpdateUserPreference: No user preference found to update");
      }
      
      return upsertedCount;
    }

    private async Task<int> DeleteUserPreference(UserPrefDataModel userPref, UserPrefDataModel existing)
    {
      Log.LogDebug($"PreferenceRepository/DeleteUserPreference: user preference={JsonConvert.SerializeObject(userPref)}))')");

      var upsertedCount = 0;
      if (existing != null)
      {  
        const string delete =
          @"DELETE FROM UserPreference
                WHERE UserPreferenceID = @UserPreferenceID";
        upsertedCount = await ExecuteWithAsyncPolicy(delete, userPref);
        Log.LogDebug($"PreferenceRepository/DeleteUserPreference: deleted {upsertedCount} rows");
      }
      else
      {
        Log.LogDebug($"PreferenceRepository/DeleteUserPreference: No user preference found to delete");
      }

      return upsertedCount;
    }

    #endregion

    #region preference-get
    /// <summary>
    /// Gets a preference key by Uid and key name
    /// </summary>
    public async Task<PrefKeyDataModel> GetPreferenceKey(Guid prefUID, string prefKeyName)
    {
      var prefKey = (await QueryWithAsyncPolicy<PrefKeyDataModel>(@"SELECT 
                PreferenceKeyID, PreferenceKeyUID, KeyName
              FROM PreferenceKey 
              WHERE PreferenceKeyUID = @PreferenceKeyUID 
                AND KeyName = @KeyName",
        new { PreferenceKeyUID = prefUID, KeyName  = prefKeyName })).FirstOrDefault();
      return prefKey;
    }
    #endregion
  }
}
