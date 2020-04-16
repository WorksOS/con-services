using System;
using System.Threading.Tasks;
using CCSS.Productivity3D.Preferences.Common.Executors;
using CSS.Productivity3D.Preferences.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.Preference;
using Xunit;
using VSS.Common.Exceptions;
using System.Net;
using Moq;
using PrefKeyDataModel = CCSS.Productivity3D.Preferences.Abstractions.Models.Database.PreferenceKey;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using CCSS.Productivity3D.Preferences.Abstractions.ResultsHandling;

namespace CCSS.Productivity3D.Preferences.Tests.Executors
{
  public class CreatePreferenceKeyExecutorTests : UnitTestsDIFixture<CreatePreferenceKeyExecutorTests>
  {
    public CreatePreferenceKeyExecutorTests()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    [Fact]
    public async Task CreatePreferenceKeyExecutor_DuplicateKeyName()
    {
      const string keyName= "some key";  

      var prefKeyDatabase = new PrefKeyDataModel
      {
        KeyName = keyName,
        PreferenceKeyUID = Guid.NewGuid().ToString(),
        PreferenceKeyID = 12345
      };
      mockPrefRepo.Setup(p => p.GetPreferenceKey(null, keyName))
              .ReturnsAsync(prefKeyDatabase);
     
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = keyName,
        PreferenceKeyUID = Guid.NewGuid()
      };

      var executor = RequestExecutorContainerFactory.Build<CreatePreferenceKeyExecutor>
       (Logger, ServiceExceptionHandler, mockPrefRepo.Object);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await executor.ProcessAsync(prefEvent));
      Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
      var result = ex.GetResult;
      Assert.Equal(2004, result.Code);
      Assert.Equal($"Duplicate preference key name. {keyName}", result.Message);
    }

    [Fact]
    public async Task CreatePreferenceKeyExecutor_DuplicateKeyUID()
    {
      var keyUid = Guid.NewGuid();

      var prefKeyDatabase = new PrefKeyDataModel
      {
        KeyName = "some key",
        PreferenceKeyUID = keyUid.ToString(),
        PreferenceKeyID = 12345
      };
      mockPrefRepo.Setup(p => p.GetPreferenceKey(keyUid, null))
              .ReturnsAsync(prefKeyDatabase);

      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = "some other key",
        PreferenceKeyUID = keyUid
      };

      var executor = RequestExecutorContainerFactory.Build<CreatePreferenceKeyExecutor>
       (Logger, ServiceExceptionHandler, mockPrefRepo.Object);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await executor.ProcessAsync(prefEvent));
      Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
      var result = ex.GetResult;
      Assert.Equal(2005, result.Code);
      Assert.Equal($"Duplicate preference key UID. {keyUid}", result.Message);
    }

    [Fact]
    public async Task CreatePreferenceKeyExecutor_HappyPath()
    {
      const string keyName = "some key";
      var keyUid = Guid.NewGuid();
  
      mockPrefRepo.Setup(p => p.GetPreferenceKey(null, keyName))
              .ReturnsAsync((PrefKeyDataModel)null);
      mockPrefRepo.Setup(p => p.GetPreferenceKey(keyUid, null))
              .ReturnsAsync((PrefKeyDataModel)null);
     
      var prefEvent = new CreatePreferenceKeyEvent
      {
        PreferenceKeyName = keyName,
        PreferenceKeyUID = keyUid
      };
      mockPrefRepo.Setup(p => p.StoreEvent(prefEvent))
        .ReturnsAsync(1);
      var prefKeyDatabase = new PrefKeyDataModel
      {
        KeyName = keyName,
        PreferenceKeyUID = keyUid.ToString(),
        PreferenceKeyID = 12345
      };
      mockPrefRepo.Setup(p => p.GetPreferenceKey(keyUid, keyName))
             .ReturnsAsync(prefKeyDatabase);

      var executor = RequestExecutorContainerFactory.Build<CreatePreferenceKeyExecutor>
       (Logger, ServiceExceptionHandler, mockPrefRepo.Object);
      var result = await executor.ProcessAsync(prefEvent) as PreferenceKeyV1Result;
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
      Assert.Equal(prefEvent.PreferenceKeyName, result.PreferenceKeyName);
      Assert.Equal(prefEvent.PreferenceKeyUID, result.PreferenceKeyUID);
    }
  }
}
