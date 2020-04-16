using System;
using System.Threading.Tasks;
using CCSS.Productivity3D.Preferences.Common.Executors;
using CSS.Productivity3D.Preferences.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.Preference;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Handlers;
using VSS.Common.Exceptions;
using System.Net;
using Moq;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using CCSS.Productivity3D.Preferences.Abstractions.ResultsHandling;
using UserPrefKeyDataModel = CCSS.Productivity3D.Preferences.Abstractions.Models.Database.UserPreferenceKey;


namespace CCSS.Productivity3D.Preferences.Tests.Executors
{
  public class UpdateUserPreferenceExecutorTests : UnitTestsDIFixture<UpdateUserPreferenceExecutorTests>
  {
    public UpdateUserPreferenceExecutorTests()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    [Fact]
    public async Task UpdateUserPreferenceExecutor_NoExisting()
    {
      var prefEvent = new UpdateUserPreferenceEvent
      {
        PreferenceKeyName = "some key",
        PreferenceKeyUID = Guid.NewGuid(),
        UserUID = Guid.NewGuid(),
        PreferenceJson = "some new json",
        SchemaVersion = "1.0"
      };
      mockPrefRepo.Setup(p => p.StoreEvent(prefEvent))
        .ReturnsAsync(0);

      var executor = RequestExecutorContainerFactory.Build<UpdateUserPreferenceExecutor>
       (Logger, ServiceExceptionHandler, mockPrefRepo.Object);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await executor.ProcessAsync(prefEvent));
      Assert.Equal(HttpStatusCode.InternalServerError, ex.Code);
      var result = ex.GetResult;
      Assert.Equal(2011, result.Code);
      Assert.Equal("Unable to update user preference. ", result.Message);
    }

    [Fact]
    public async Task UpdateUserPreferenceExecutor_HappyPath()
    {
      const string keyName = "some key";
      var userUid = Guid.NewGuid();

      var prefEvent = new UpdateUserPreferenceEvent
      {
        PreferenceKeyName = keyName,
        PreferenceKeyUID = Guid.NewGuid(),
        UserUID = userUid,
        PreferenceJson = "some new json",
        SchemaVersion = "1.0"
      };
      mockPrefRepo.Setup(p => p.StoreEvent(prefEvent))
        .ReturnsAsync(1);

      var userPref = new UserPrefKeyDataModel
      {
        KeyName = keyName,
        PreferenceKeyUID = prefEvent.PreferenceKeyUID.Value.ToString(),
        PreferenceJson = prefEvent.PreferenceJson,
        SchemaVersion = prefEvent.SchemaVersion
      };
      mockPrefRepo.Setup(p => p.GetUserPreference(userUid, keyName))
        .ReturnsAsync(userPref);

      var executor = RequestExecutorContainerFactory.Build<UpdateUserPreferenceExecutor>
       (Logger, ServiceExceptionHandler, mockPrefRepo.Object);
      var result = await executor.ProcessAsync(prefEvent) as UserPreferenceV1Result;
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
      Assert.Equal(prefEvent.SchemaVersion, result.SchemaVersion);
      Assert.Equal(prefEvent.PreferenceJson, result.PreferenceJson);
      Assert.Equal(prefEvent.PreferenceKeyName, result.PreferenceKeyName);
      Assert.Equal(prefEvent.PreferenceKeyUID, result.PreferenceKeyUID);
    }
  }
}
