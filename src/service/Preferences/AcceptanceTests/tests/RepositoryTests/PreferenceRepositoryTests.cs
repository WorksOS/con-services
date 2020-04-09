using System;
using System.Collections.Generic;
using CCSS.Productivity3D.Preferences.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Repositories;
using VSS.Serilog.Extensions;
using VSS.VisionLink.Interfaces.Events.Preference.Interfaces;
using Serilog;

using Dapper;
using System.Linq;
using VSS.VisionLink.Interfaces.Events.Preference;
using VSS.ConfigurationStore;

namespace RepositoryTests
{
  [TestClass]
  public class PreferenceRepositoryTests
  {
    private IServiceProvider ServiceProvider;
    private IConfigurationStore ConfigStore;
    private PreferenceRepository PrefRepo;
    
    [TestInitialize]
    public void Init()
    {
      ServiceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("CCSS.Preference.Repository.Tests.log")))    
        .AddTransient<IRepository<IPreferenceEvent>, PreferenceRepository>()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddMemoryCache()
        //.AddSingleton<IDataCache, InMemoryDataCache>()
        .BuildServiceProvider();

      ConfigStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      PrefRepo = ServiceProvider.GetRequiredService<IRepository<IPreferenceEvent>>() as PreferenceRepository;
      
      Assert.IsNotNull(ServiceProvider.GetService<ILoggerFactory>());
    }

    [TestMethod]
    public void PreferenceKeySchemaExists()
    {
      const string tableName = "PreferenceKey";
      List<string> columnNames = new List<string>
      {
        "PreferenceKeyID", "PreferenceKeyUID", "KeyName", "InsertUTC", "UpdateUTC"
      };

      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void UserPreferenceSchemaExists()
    {
      const string tableName = "UserPreference";
      List<string> columnNames = new List<string>
      {
        "UserPreferenceID", "UserUID", "fk_PreferenceKeyID", "Value", "SchemaVersion", "InsertUTC", "UpdateUTC"
      };

      CheckSchema(tableName, columnNames);
    }

    #region preference-key
    [TestMethod]
    public void CreatePreferenceKey_HappyPath()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);
    }

    [TestMethod]
    public void CreatePreferenceKey_DuplicateKeyName()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);

      var prefEvent2 = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = prefEvent.PreferenceKeyName,
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent2, "Preference key with duplicate name should not be created", 0);
    }


    [TestMethod]
    public void CreatePreferenceKey_DuplicateKeyUid()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);

      var prefEvent2 = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = "another key",
        PreferenceKeyUID = prefEvent.PreferenceKeyUID
      };

      WriteEventToDb(prefEvent2, "Preference key with duplicate UID should not be created", 0);
    }

   
    [TestMethod]
    public void UpdatePreferenceKey_HappyPath()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);

      var prefEvent2 = new UpdatePreferenceKeyEvent
      {
        PreferenceKeyName = $"updated key {DateTime.Now.Ticks}",
        PreferenceKeyUID = prefEvent.PreferenceKeyUID
      };

      WriteEventToDb(prefEvent2);
    }

    [TestMethod]
    public void UpdatePreferenceKey_DuplicateKeyName()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);

      var prefEvent2 = new UpdatePreferenceKeyEvent
      {
        PreferenceKeyName = prefEvent.PreferenceKeyName,
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent2, "Preference key with duplicate name should not be updated", 0);
    }

    [TestMethod]
    public void UpdatePreferenceKey_NoExisting()
    {
      var prefEvent = new UpdatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent, "Non-existent preference key should not be updated", 0);
    }

    [TestMethod]
    public void DeletePreferenceKey_HappyPath()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);

      var prefEvent2 = new DeletePreferenceKeyEvent
      {
        PreferenceKeyUID = prefEvent.PreferenceKeyUID
      };

      WriteEventToDb(prefEvent2);
    }

    [TestMethod]
    public void DeletePreferenceKey_NoExisting()
    {
      var prefEvent = new DeletePreferenceKeyEvent
      {
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent, "Non-existent preference key should not be deleted", 0);
    }


    [TestMethod]
    public void GetPreferenceKey_HappyPath()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);

      //Get by key name
      var task = PrefRepo.GetPreferenceKey(prefKeyName: prefEvent.PreferenceKeyName);
      task.Wait();
      var result = task.Result;
      Assert.IsNotNull(result);
      Assert.AreEqual(prefEvent.PreferenceKeyName, result.KeyName); 
      Assert.AreEqual(prefEvent.PreferenceKeyUID.ToString(), result.PreferenceKeyUID);

      //Get by key UID
      task = PrefRepo.GetPreferenceKey(prefEvent.PreferenceKeyUID);
      task.Wait();
      result = task.Result;
      Assert.IsNotNull(result);
      Assert.AreEqual(prefEvent.PreferenceKeyName, result.KeyName);
      Assert.AreEqual(prefEvent.PreferenceKeyUID.ToString(), result.PreferenceKeyUID);

      //Get by both key name and UID
      task = PrefRepo.GetPreferenceKey(prefEvent.PreferenceKeyUID, prefEvent.PreferenceKeyName);
      task.Wait();
      result = task.Result;
      Assert.IsNotNull(result);
      Assert.AreEqual(prefEvent.PreferenceKeyName, result.KeyName);
      Assert.AreEqual(prefEvent.PreferenceKeyUID.ToString(), result.PreferenceKeyUID);
    }

    [TestMethod]
    public void GetPreferenceKey_NoExisting()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);

      var keyName2 = "some other preference key";
      var keyUid2 = Guid.NewGuid();
      //Get by key name
      var task = PrefRepo.GetPreferenceKey(prefKeyName: keyName2);
      task.Wait();
      var result = task.Result;
      Assert.IsNull(result);

      //Get by key UID
      task = PrefRepo.GetPreferenceKey(keyUid2);
      task.Wait();
      result = task.Result;
      Assert.IsNull(result);

      //Get by both key name and UID
      task = PrefRepo.GetPreferenceKey(keyUid2, keyName2);
      task.Wait();
      result = task.Result;
      Assert.IsNull(result);
    }

    #endregion

    #region user-preference

    [TestMethod]
    public void CreateUserPreference_HappyPath()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);

      var userPrefEvent = new CreateUserPreferenceEvent
      {
        PreferenceKeyName = prefEvent.PreferenceKeyName,
        PreferenceKeyUID = prefEvent.PreferenceKeyUID,
        PreferenceJson = "some json here",
        SchemaVersion = "1.0",
        UserUID = Guid.NewGuid()
      };

      WriteEventToDb(userPrefEvent);
    }

    [TestMethod]
    public void CreateUserPreference_WithExisting()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);

      var userPrefEvent = new CreateUserPreferenceEvent
      {
        PreferenceKeyName = prefEvent.PreferenceKeyName,
        PreferenceKeyUID = prefEvent.PreferenceKeyUID,
        PreferenceJson = "some json here",
        SchemaVersion = "1.0",
        UserUID = Guid.NewGuid()
      };

      WriteEventToDb(userPrefEvent);

      var userPrefEvent2 = new CreateUserPreferenceEvent
      {
        PreferenceKeyName = userPrefEvent.PreferenceKeyName,
        PreferenceKeyUID = userPrefEvent.PreferenceKeyUID,
        PreferenceJson = "some different json here",
        SchemaVersion = "1.0",
        UserUID = userPrefEvent.UserUID
      };

      WriteEventToDb(userPrefEvent2, "Existing user preference should not be created", 0);
    }

    [TestMethod]
    public void UpdateUserPreference_HappyPath()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);

      var userPrefEvent = new CreateUserPreferenceEvent
      {
        PreferenceKeyName = prefEvent.PreferenceKeyName,
        PreferenceKeyUID = prefEvent.PreferenceKeyUID,
        PreferenceJson = "some json here",
        SchemaVersion = "1.0",
        UserUID = Guid.NewGuid()
      };

      WriteEventToDb(userPrefEvent);

      var userPrefEvent2 = new UpdateUserPreferenceEvent
      {
        PreferenceKeyName = prefEvent.PreferenceKeyName,
        PreferenceKeyUID = prefEvent.PreferenceKeyUID,
        PreferenceJson = "some changed json here",
        SchemaVersion = "1.0",
        UserUID = userPrefEvent.UserUID
      };

      WriteEventToDb(userPrefEvent2);
    }

    [TestMethod]
    public void UpdateUserPreference_NoExisting()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);

      var userPrefEvent = new UpdateUserPreferenceEvent
      {
        PreferenceKeyName = prefEvent.PreferenceKeyName,
        PreferenceKeyUID = prefEvent.PreferenceKeyUID,
        PreferenceJson = "some json here",
        SchemaVersion = "1.0",
        UserUID = Guid.NewGuid()
      };

      WriteEventToDb(userPrefEvent, "Non-existent user preference should not be updated", 0);
    }

    [TestMethod]
    public void DeleteUserPreference_HappyPath()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);

      var userPrefEvent = new CreateUserPreferenceEvent
      {
        PreferenceKeyName = prefEvent.PreferenceKeyName,
        PreferenceKeyUID = prefEvent.PreferenceKeyUID,
        PreferenceJson = "some json here",
        SchemaVersion = "1.0",
        UserUID = Guid.NewGuid()
      };

      WriteEventToDb(userPrefEvent);

      var userPrefEvent2 = new DeleteUserPreferenceEvent
      {
        PreferenceKeyName = prefEvent.PreferenceKeyName,
        PreferenceKeyUID = prefEvent.PreferenceKeyUID,
        UserUID = userPrefEvent.UserUID
      };

      WriteEventToDb(userPrefEvent2);
    }

    [TestMethod]
    public void DeleteUserPreference_NoExisting()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);

      var userPrefEvent = new DeleteUserPreferenceEvent
      {
        PreferenceKeyName = prefEvent.PreferenceKeyName,
        PreferenceKeyUID = prefEvent.PreferenceKeyUID,
        UserUID = Guid.NewGuid()
      };

      WriteEventToDb(userPrefEvent, "Non-existent user preference should not be deleted", 0);
    }


    [TestMethod]
    public void GetUserPreference_HappyPath()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);

      var userPrefEvent = new CreateUserPreferenceEvent
      {
        PreferenceKeyName = prefEvent.PreferenceKeyName,
        PreferenceKeyUID = prefEvent.PreferenceKeyUID,
        PreferenceJson = "some json here",
        SchemaVersion = "1.0",
        UserUID = Guid.NewGuid()
      };

      WriteEventToDb(userPrefEvent);

      var task = PrefRepo.GetUserPreference(userPrefEvent.UserUID, userPrefEvent.PreferenceKeyName);
      task.Wait();
      var result = task.Result;
      Assert.IsNotNull(result);
      Assert.AreEqual(userPrefEvent.PreferenceKeyName, result.KeyName);
      Assert.AreEqual(userPrefEvent.PreferenceKeyUID.ToString(), result.PreferenceKeyUID);
      Assert.AreEqual(userPrefEvent.PreferenceJson, result.PreferenceJson);
      Assert.AreEqual(userPrefEvent.SchemaVersion, result.SchemaVersion);
    }

    [TestMethod]
    public void GetUserPreference_NoExisting()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);

      var userPrefEvent = new CreateUserPreferenceEvent
      {
        PreferenceKeyName = prefEvent.PreferenceKeyName,
        PreferenceKeyUID = prefEvent.PreferenceKeyUID,
        PreferenceJson = "some json here",
        SchemaVersion = "1.0",
        UserUID = Guid.NewGuid()
      };

      WriteEventToDb(userPrefEvent);

      var task = PrefRepo.GetUserPreference(Guid.NewGuid(), prefEvent.PreferenceKeyName);
      task.Wait();
      var result = task.Result;
      Assert.IsNull(result);
    }

    [TestMethod]
    public void UserPreferenceExistsForKey_Exists()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);

      var userPrefEvent = new CreateUserPreferenceEvent
      {
        PreferenceKeyName = prefEvent.PreferenceKeyName,
        PreferenceKeyUID = prefEvent.PreferenceKeyUID,
        PreferenceJson = "some json here",
        SchemaVersion = "1.0",
        UserUID = Guid.NewGuid()
      };

      WriteEventToDb(userPrefEvent);

      var task = PrefRepo.UserPreferenceExistsForKey(prefEvent.PreferenceKeyUID);
      task.Wait();
      var result = task.Result;
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void UserPreferenceExistsForKey_DoesNotExist()
    {
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = $"some key {DateTime.Now.Ticks}",
        PreferenceKeyUID = Guid.NewGuid()
      };

      WriteEventToDb(prefEvent);

      var userPrefEvent = new CreateUserPreferenceEvent
      {
        PreferenceKeyName = prefEvent.PreferenceKeyName,
        PreferenceKeyUID = prefEvent.PreferenceKeyUID,
        PreferenceJson = "some json here",
        SchemaVersion = "1.0",
        UserUID = Guid.NewGuid()
      };

      WriteEventToDb(userPrefEvent);

      var task = PrefRepo.UserPreferenceExistsForKey(Guid.NewGuid());
      task.Wait();
      var result = task.Result;
      Assert.IsFalse(result);
    }
    #endregion

    #region privates

    [TestMethod]
    private void CheckSchema(string tableName, List<string> columnNames)
    {
      using (var connection = new MySqlConnection(this.ConfigStore.GetConnectionString("VSPDB")))
      {
        try
        {
          connection.Open();

          //Check table exists
          var table = connection.Query(GetQuery(tableName, true)).FirstOrDefault();
          Assert.IsNotNull(table, "Missing " + tableName + " table schema");
          Assert.AreEqual(tableName, table.TABLE_NAME, "Wrong table name");

          //Check table columns exist
          var columns = connection.Query(GetQuery(tableName, false)).ToList();
          Assert.IsNotNull(columns, "Missing " + tableName + " table columns");
          Assert.AreEqual(columnNames.Count, columns.Count, "Wrong number of " + tableName + " columns");
          foreach (var columnName in columnNames)
            Assert.IsNotNull(columns.Find(c => c.COLUMN_NAME == columnName), "Missing " + columnName + " column in " + tableName + " table");
        }
        finally
        {
          connection.Close();
        }
      }
    }

    private string GetQuery(string tableName, bool selectTable)
    {
      string what = selectTable ? "TABLE_NAME" : "COLUMN_NAME";
      var query = string.Format("SELECT {0} FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{1}' AND TABLE_NAME ='{2}'",
        what, this.ConfigStore.GetValueString("MYSQL_DATABASE_NAME"), tableName);
      return query;
    }

    private void WriteEventToDb(IPreferenceEvent prefEvent, string errorMessage = "Preference event not written", int returnCode = 1)
    {
      var task = PrefRepo.StoreEvent(prefEvent);
      task.Wait();

      Assert.AreEqual(returnCode, task.Result, errorMessage);
    }

    #endregion privates
  }
}
