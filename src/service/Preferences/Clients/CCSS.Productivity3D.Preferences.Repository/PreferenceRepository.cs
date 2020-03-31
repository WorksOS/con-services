using System.Threading.Tasks;
using CCSS.Productivity3D.Preferences.Abstractions.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.Preference;
using VSS.VisionLink.Interfaces.Events.Preference.Interfaces;
using PrefKeyDataModel = CCSS.Productivity3D.Preferences.Abstractions.Models.Database.PreferenceKey;
using UserPrefDataModel = CCSS.Productivity3D.Preferences.Abstractions.Models.Database.UserPreference;
using UserPrefKeyDataModel = CCSS.Productivity3D.Preferences.Abstractions.Models.Database.UserPreferenceKey;
using System.Linq;
using System;
using CCSS.Productivity3D.Preferences.Abstractions.Models.Database;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CCSS.Productivity3D.Preferences.Repository
{
  /// <summary>
  /// Repository for preferences.
  /// </summary>
  public class PreferenceRepository : RepositoryBase, IRepository<IPreferenceEvent>, IPreferenceRepository
  {
    public PreferenceRepository(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore,
      logger)
    {
      Log = logger.CreateLogger<PreferenceRepository>();
    }

    #region preference-store
    /// <summary>
    /// Create, update and delete for preference keys and user preferences.
    /// </summary>
    /// <param name="evt"></param>
    /// <returns></returns>
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
        upsertedCount = await UpsertPreferenceKey(PreferenceEventType.CreatePreferenceKeyEvent, prefEvent.PreferenceKeyUID, prefEvent.PreferenceKeyName);
      }
      else if (evt is UpdatePreferenceKeyEvent)
      {
        var prefEvent = (UpdatePreferenceKeyEvent)evt;
        upsertedCount = await UpsertPreferenceKey(PreferenceEventType.UpdatePreferenceKeyEvent, prefEvent.PreferenceKeyUID, prefEvent.PreferenceKeyName); 
      }
      else if (evt is DeletePreferenceKeyEvent)
      {
        var prefEvent = (DeletePreferenceKeyEvent)evt;
        upsertedCount = await UpsertPreferenceKey(PreferenceEventType.DeletePreferenceKeyEvent, prefEvent.PreferenceKeyUID, null);
      }
      else if (evt is CreateUserPreferenceEvent)
      {
        var prefEvent = (CreateUserPreferenceEvent)evt;
        upsertedCount = await UpsertUserPreference(PreferenceEventType.CreateUserPreferenceEvent, prefEvent.PreferenceKeyUID, prefEvent.PreferenceKeyName,
          prefEvent.UserUID, prefEvent.SchemaVersion, prefEvent.PreferenceJson);
      }
      else if (evt is UpdateUserPreferenceEvent)
      {
        var prefEvent = (UpdateUserPreferenceEvent)evt;
        //Historical: Event types are from Kafka. we now create the events so the UserUID will always be set.
        upsertedCount = await UpsertUserPreference(PreferenceEventType.UpdateUserPreferenceEvent, prefEvent.PreferenceKeyUID, prefEvent.PreferenceKeyName,
          prefEvent.UserUID.Value, prefEvent.SchemaVersion, prefEvent.PreferenceJson);
      }
      else if (evt is DeleteUserPreferenceEvent)
      {
        var prefEvent = (DeleteUserPreferenceEvent)evt;
        //Historical: Event types are from Kafka. we now create the events so the UserUID will always be set.
        upsertedCount = await UpsertUserPreference(PreferenceEventType.DeleteUserPreferenceEvent, prefEvent.PreferenceKeyUID, prefEvent.PreferenceKeyName,
          prefEvent.UserUID.Value);
      }
      
      return upsertedCount;
    }

    /// <summary>
    /// Create, update and delete of a preference key
    /// </summary>
    private async Task<int> UpsertPreferenceKey(PreferenceEventType eventType, Guid? prefKeyUID, string prefKeyName)
    {    
      if (eventType == PreferenceEventType.CreatePreferenceKeyEvent || eventType == PreferenceEventType.UpdatePreferenceKeyEvent)
      {
        //Check name doesn't already exist as it must be unique
        if (await GetPreferenceKey(prefKeyName: prefKeyName) != null)
        {
          Log.LogDebug($"PreferenceRepository/StorePreferenceKey: No action as preference key name already exists");
          return 0;
        }
      }
      var upsertedCount = 0;
      var existing = await GetPreferenceKey(prefKeyUID);
     
      var prefKey = new PrefKeyDataModel
      {
        KeyName = prefKeyName,
        PreferenceKeyUID = prefKeyUID?.ToString(),
        PreferenceKeyID = existing?.PreferenceKeyID ?? 0
      };

      if (eventType == PreferenceEventType.CreatePreferenceKeyEvent)
        upsertedCount = await CreatePreferenceKey(prefKey, existing);

      if (eventType == PreferenceEventType.UpdatePreferenceKeyEvent)
        upsertedCount = await UpdatePreferenceKey(prefKey, existing);

      if (eventType == PreferenceEventType.DeletePreferenceKeyEvent)
        upsertedCount = await DeletePreferenceKey(prefKey, existing);
      return upsertedCount;
    }

    /// <summary>
    /// SQL for creating a preference key
    /// </summary>
    private async Task<int> CreatePreferenceKey(PrefKeyDataModel prefKey, PrefKeyDataModel existing)
    {
      Log.LogDebug($"PreferenceRepository/CreatePreferenceKey: preference key={JsonConvert.SerializeObject(prefKey)}))')");

      var upsertedCount = 0;
      if (existing == null)
      {
        var insert = @"INSERT PreferenceKey
              (PreferenceKeyUID, KeyName)
                VALUES
              (@PreferenceKeyUID, @KeyName)";

        upsertedCount = await ExecuteWithAsyncPolicy(insert, prefKey);
        Log.LogDebug($"PreferenceRepository/CreatePreferenceKey: inserted {upsertedCount} rows");
      }
      else
      {
        Log.LogDebug("PreferenceRepository/CreatePreferenceKey: No action as preference key already exists.");
      }
      
      return upsertedCount;
    }

    /// <summary>
    /// SQL for updating a preference key
    /// </summary>
    private async Task<int> UpdatePreferenceKey(PrefKeyDataModel prefKey, PrefKeyDataModel existing)
    {
      Log.LogDebug($"PreferenceRepository/UpdatePreferenceKey: preference key={JsonConvert.SerializeObject(prefKey)}))')");

      var upsertedCount = 0;
      if (existing != null)
      {
        var update = $@"UPDATE PreferenceKey
              SET KeyName = @KeyName
              WHERE PreferenceKeyID = @PreferenceKeyID";

        upsertedCount = await ExecuteWithAsyncPolicy(update, prefKey);
        Log.LogDebug($"PreferenceRepository/UpdatePreferenceKey: upserted {upsertedCount} rows");
      }
      else
      {
        Log.LogDebug($"PreferenceRepository/UpdatePreferenceKey: No preference key found to update");
      }
     
      return upsertedCount;
    }

     /// <summary>
    /// SQL for deleting a preference key
    /// </summary>
    private async Task<int> DeletePreferenceKey(PrefKeyDataModel prefKey, PrefKeyDataModel existing)
    {
      Log.LogDebug($"PreferenceRepository/DeletePreferenceKey: user preference={JsonConvert.SerializeObject(prefKey)}))')");

      var upsertedCount = 0;
      if (existing != null)
      {
        const string delete =
          @"DELETE FROM PreferenceKey
              WHERE PreferenceKeyID = @PreferenceKeyID";
        upsertedCount = await ExecuteWithAsyncPolicy(delete, prefKey);
        Log.LogDebug($"PreferenceRepository/DeletePreferenceKey: deleted {upsertedCount} rows");
      }
      else
      {
        Log.LogDebug($"PreferenceRepository/DeletePreferenceKey: No preference key found to delete");
      }
    
      return upsertedCount;
    }


    /// <summary>
    /// Create, update or delete a user preference.
    /// </summary>
    private async Task<int> UpsertUserPreference(PreferenceEventType eventType, Guid? prefKeyUID, string prefKeyName, Guid userUID, string schemaVersion=null, string prefJson=null)
    {
      // Look up the preference key to get the foreign key for user preferences
      var prefKey = await GetPreferenceKey(prefKeyUID, prefKeyName);
      if (prefKey == null) return 0;
      
      var userPref = new UserPrefDataModel
      {
        PreferenceKeyID = prefKey.PreferenceKeyID,
        SchemaVersion = schemaVersion,
        UserUID = userUID,
        PreferenceJson = prefJson
      };

      var upsertedCount = 0;
      var existing = await GetUserPreference(userUID, userPref.PreferenceKeyID);
      
      if (eventType == PreferenceEventType.CreateUserPreferenceEvent)
        upsertedCount = await CreateUserPreference(userPref, existing);

      if (eventType == PreferenceEventType.UpdateUserPreferenceEvent)
        upsertedCount = await UpdateUserPreference(userPref, existing);

      if (eventType == PreferenceEventType.DeleteUserPreferenceEvent)
        upsertedCount = await DeleteUserPreference(userPref, existing);
      return upsertedCount;  
    }


    /// <summary>
    /// SQL for creating a user preference
    /// </summary>
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

    /// <summary>
    /// SQL for updating a user preference
    /// </summary>
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

    /// <summary>
    /// SQL for deleting a user preference
    /// </summary>
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
    /// Gets a preference key by Uid and/or key name
    /// </summary>
    public async Task<PrefKeyDataModel> GetPreferenceKey(Guid? prefKeyUID=null, string prefKeyName=null)
    {
      if (!prefKeyUID.HasValue && string.IsNullOrEmpty(prefKeyName))
      {
        return null;
      }

      string whereClause;
      object args;
      if (prefKeyUID.HasValue && !string.IsNullOrEmpty(prefKeyName))
      {
        whereClause = $"PreferenceKeyUID = @PreferenceKeyUID AND KeyName = @KeyName";
        args = new { PreferenceKeyUID = prefKeyUID, KeyName = prefKeyName };
      } 
      else if (prefKeyUID.HasValue)
      {
        whereClause = $"PreferenceKeyUID = @PreferenceKeyUID";
        args = new { PreferenceKeyUID = prefKeyUID };
      } 
      else //!string.IsNullOrEmpty(prefKeyName)
      {
        whereClause = $"KeyName = @KeyName";
        args = new { KeyName = prefKeyName };
      }
      var select = $"SELECT PreferenceKeyID, PreferenceKeyUID, KeyName FROM PreferenceKey WHERE {whereClause}";
              
      var prefKey = (await QueryWithAsyncPolicy<PrefKeyDataModel>(select, args)).FirstOrDefault();
      return prefKey;
    }

    /// <summary>
    /// Get a user preference by user Uid and key ID
    /// </summary>
    public async Task<UserPrefDataModel> GetUserPreference(Guid userUID, long prefKeyID)
    {
      var userPref = (await QueryWithAsyncPolicy<UserPrefDataModel>
      (@"SELECT UserPreferenceID, UserUID, fk_PreferenceKeyID AS PreferenceKeyID, Value, SchemaVersion
              FROM UserPreference
              WHERE fk_PreferenceKeyID = @PreferenceKeyID
                AND UserUID = @UserUID",
        new { PreferenceKeyID=prefKeyID, UserUID=userUID.ToString() }
      )).FirstOrDefault();
      return userPref;
    }

    /// <summary>
    /// Get a user preference by user Uid and key name
    /// </summary>
    public async Task<UserPrefKeyDataModel> GetUserPreference(Guid userUID, string prefKeyName)
    {
      var userPrefKey = (await QueryWithAsyncPolicy<UserPrefKeyDataModel>
      (@"SELECT Value, SchemaVersion, KeyName, PreferenceKeyUID
              FROM UserPreference up 
              INNER JOIN PreferenceKey pk ON up.fk_PreferenceKeyID = pk.PreferenceKeyID
              WHERE UserUID = @UserUID AND KeyName = @KeyName",
        new { UserUID = userUID.ToString(), KeyName = prefKeyName }
      )).FirstOrDefault();
      return userPrefKey;
    }

    /// <summary>
    /// Checks if there are any user preferencs for the given key UID
    /// </summary>
    public async Task<bool> UserPreferenceExistsForKey(Guid prefKeyUID)
    {
      var userPrefKeys = (await QueryWithAsyncPolicy<UserPrefKeyDataModel>
      (@"SELECT Value, SchemaVersion, KeyName, PreferenceKeyUID
              FROM UserPreference up 
              INNER JOIN PreferenceKey pk ON up.fk_PreferenceKeyID = pk.PreferenceKeyID
              WHERE PreferenceKeyUID = @PreferenceKeyUID",
        new { PreferenceKeyUID = prefKeyUID }
      )).ToList();
      return userPrefKeys != null && userPrefKeys.Count > 0;
    }

    //todoMaaverick: Do we need a getUserPreferences to get all for a user. I think we always get by KeyName.

    #endregion
  }
}
