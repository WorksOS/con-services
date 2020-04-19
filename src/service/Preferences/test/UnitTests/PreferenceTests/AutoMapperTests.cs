using System;
using CCSS.Productivity3D.Preferences.Abstractions.ResultsHandling;
using CCSS.Productivity3D.Preferences.Common.Models;
using CSS.Productivity3D.Preferences.Common.Utilities;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.VisionLink.Interfaces.Events.Preference;
using Xunit;
using PrefKeyDataModel = CCSS.Productivity3D.Preferences.Abstractions.Models.Database.PreferenceKey;
using UserPrefKeyDataModel = CCSS.Productivity3D.Preferences.Abstractions.Models.Database.UserPreferenceKey;

namespace CCSS.Productivity3D.Preferences.Tests
{
  public class AutoMapperTests
  {
    [Fact]
    public void AssertConfigurationIsValid()
    {
      var exception = Record.Exception(() => AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid());
      Assert.Null(exception);
    }

    [Fact]
    public void MapUserPrefDatabaseModelToResult()
    {
      var userPref = new UserPrefKeyDataModel
      {
        KeyName = "some preference key",
        PreferenceKeyUID = Guid.NewGuid().ToString(),
        PreferenceJson = "some json string here",
        SchemaVersion = "1.0"
      };

      var result = AutoMapperUtility.Automapper.Map<UserPreferenceV1Result>(userPref);
      Assert.Equal(userPref.KeyName, result.PreferenceKeyName);
      Assert.Equal(userPref.PreferenceJson, result.PreferenceJson);
      Assert.Equal(userPref.PreferenceKeyUID, result.PreferenceKeyUID.ToString());
      Assert.Equal(userPref.SchemaVersion, result.SchemaVersion);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
    }

    [Fact]
    public void MapPrefKeyDatabaseModelToResult()
    {
      var prefKey = new PrefKeyDataModel
      {
        KeyName = "some preference key",
        PreferenceKeyUID = Guid.NewGuid().ToString(),
      };

      var result = AutoMapperUtility.Automapper.Map<PreferenceKeyV1Result>(prefKey);
      Assert.Equal(prefKey.KeyName, result.PreferenceKeyName);
      Assert.Equal(prefKey.PreferenceKeyUID, result.PreferenceKeyUID.ToString());
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
    }

    [Fact]
    public void MapUserPrefRequestToCreateUserPrefEvent()
    {
      var request = new UpsertUserPreferenceRequest
      {
        TargetUserUID = Guid.NewGuid(),
        PreferenceKeyUID = Guid.NewGuid(),
        PreferenceKeyName = "some preference key",
        PreferenceJson = "some json string here",
        SchemaVersion = "1.0"
      };

      var result = AutoMapperUtility.Automapper.Map<CreateUserPreferenceEvent>(request);
      Assert.Equal(request.TargetUserUID, result.UserUID);
      Assert.Equal(request.PreferenceKeyUID, result.PreferenceKeyUID);
      Assert.Equal(request.PreferenceKeyName, result.PreferenceKeyName);
      Assert.Equal(request.PreferenceJson, result.PreferenceJson);
      Assert.Equal(request.SchemaVersion, result.SchemaVersion);
    }

    [Fact]
    public void MapUserPrefRequestToUpdateUserPrefEvent()
    {
      var request = new UpsertUserPreferenceRequest
      {
        TargetUserUID = Guid.NewGuid(),
        PreferenceKeyUID = Guid.NewGuid(),
        PreferenceKeyName = "some preference key",
        PreferenceJson = "some json string here",
        SchemaVersion = "1.0"
      };

      var result = AutoMapperUtility.Automapper.Map<UpdateUserPreferenceEvent>(request);
      Assert.Equal(request.TargetUserUID, result.UserUID);
      Assert.Equal(request.PreferenceKeyUID, result.PreferenceKeyUID);
      Assert.Equal(request.PreferenceKeyName, result.PreferenceKeyName);
      Assert.Equal(request.PreferenceJson, result.PreferenceJson);
      Assert.Equal(request.SchemaVersion, result.SchemaVersion);
    }
  }
}
