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
using PrefKeyDataModel = CCSS.Productivity3D.Preferences.Abstractions.Models.Database.PreferenceKey;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using CCSS.Productivity3D.Preferences.Abstractions.ResultsHandling;

namespace CCSS.Productivity3D.Preferences.Tests.Executors
{
  public class UpdatePreferenceKeyExecutorTests : UnitTestsDIFixture<UpdatePreferenceKeyExecutorTests>
  {
    public UpdatePreferenceKeyExecutorTests()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    [Fact]
    public async Task UpdatePreferenceKeyExecutor_DuplicateKeyName()
    {
      const string keyName = "some key";

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var prefKeyDatabase = new PrefKeyDataModel
      {
        KeyName = keyName,
        PreferenceKeyUID = Guid.NewGuid(),
        PreferenceKeyID = 12345
      };
      mockPrefRepo.Setup(p => p.GetPreferenceKey(null, keyName))
              .ReturnsAsync(prefKeyDatabase);

      var prefEvent = new UpdatePreferenceKeyEvent
      {
        PreferenceKeyName = keyName,
        PreferenceKeyUID = Guid.NewGuid()
      };

      var executor = RequestExecutorContainerFactory.Build<UpdatePreferenceKeyExecutor>
       (logger, serviceExceptionHandler, mockPrefRepo.Object);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await executor.ProcessAsync(prefEvent));
      Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
      var result = ex.GetResult;
      Assert.Equal(2004, result.Code);
      Assert.Equal($"Duplicate preference key name. {keyName}", result.Message);
    }

    [Fact]
    public async Task UpdatePreferenceKeyExecutor_MissingKeyUID()
    {
      const string keyName = "some key";
      var keyUid = Guid.NewGuid();

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var prefKeyDatabase = new PrefKeyDataModel
      {
        KeyName = keyName,
        PreferenceKeyUID = keyUid,
        PreferenceKeyID = 12345
      };
      mockPrefRepo.Setup(p => p.GetPreferenceKey(null, keyName))
          .ReturnsAsync((PrefKeyDataModel)null);

      var prefEvent = new UpdatePreferenceKeyEvent
      {
        PreferenceKeyName = "some other key",
        PreferenceKeyUID = keyUid
      };
      mockPrefRepo.Setup(p => p.StoreEvent(prefEvent))
        .ReturnsAsync(0);

      var executor = RequestExecutorContainerFactory.Build<UpdatePreferenceKeyExecutor>
       (logger, serviceExceptionHandler, mockPrefRepo.Object);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await executor.ProcessAsync(prefEvent));
      Assert.Equal(HttpStatusCode.InternalServerError, ex.Code);
      var result = ex.GetResult;
      Assert.Equal(2003, result.Code);
      Assert.Equal("Unable to update preference key. ", result.Message);
    }

    [Fact]
    public async Task UpdatePreferenceKeyExecutor_HappyPath()
    {
      const string keyName = "some key";
      const string newKeyName = "some other key";
      var keyUid = Guid.NewGuid();

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
     
      mockPrefRepo.Setup(p => p.GetPreferenceKey(null, keyName))
         .ReturnsAsync((PrefKeyDataModel)null);

      var prefEvent = new UpdatePreferenceKeyEvent
      {
        PreferenceKeyName = newKeyName,
        PreferenceKeyUID = keyUid
      };
      mockPrefRepo.Setup(p => p.StoreEvent(prefEvent))
        .ReturnsAsync(1);
      var prefKeyDatabase = new PrefKeyDataModel
      {
        KeyName = newKeyName,
        PreferenceKeyUID = keyUid,
        PreferenceKeyID = 12345
      };
      mockPrefRepo.Setup(p => p.GetPreferenceKey(keyUid, newKeyName))
              .ReturnsAsync(prefKeyDatabase);

      var executor = RequestExecutorContainerFactory.Build<UpdatePreferenceKeyExecutor>
       (logger, serviceExceptionHandler, mockPrefRepo.Object);
      var result = await executor.ProcessAsync(prefEvent) as PreferenceKeyV1Result;
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
      Assert.Equal(prefEvent.PreferenceKeyName, result.PreferenceKeyName);
      Assert.Equal(prefEvent.PreferenceKeyUID, result.PreferenceKeyUID);
    }
  }
}
