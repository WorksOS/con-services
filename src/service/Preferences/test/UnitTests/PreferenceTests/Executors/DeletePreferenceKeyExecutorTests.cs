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

namespace CCSS.Productivity3D.Preferences.Tests.Executors
{
  public class DeletePreferenceKeyExecutorTests : UnitTestsDIFixture<DeletePreferenceKeyExecutorTests>
  {
    public DeletePreferenceKeyExecutorTests()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    [Fact]
    public async Task DeletePreferenceKeyExecutor_UserPreferenceExists()
    {
      var keyUid = Guid.NewGuid();

      mockPrefRepo.Setup(p => p.UserPreferenceExistsForKey(keyUid))
              .ReturnsAsync(true);

      var prefEvent = new DeletePreferenceKeyEvent
      {
        PreferenceKeyUID = keyUid
      };

      var executor = RequestExecutorContainerFactory.Build<DeletePreferenceKeyExecutor>
       (Logger, ServiceExceptionHandler, mockPrefRepo.Object);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await executor.ProcessAsync(prefEvent));
      Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
      var result = ex.GetResult;
      Assert.Equal(2006, result.Code);
      Assert.Equal($"Cannot delete preference key as user preferences exist. {keyUid}", result.Message);
    }

    [Fact]
    public async Task DeletePreferenceKeyExecutor_MissingKeyUID()
    {
      var keyUid = Guid.NewGuid();

     mockPrefRepo.Setup(p => p.UserPreferenceExistsForKey(keyUid))
              .ReturnsAsync(false);

      var prefEvent = new DeletePreferenceKeyEvent
      {
        PreferenceKeyUID = keyUid
      };
      mockPrefRepo.Setup(p => p.StoreEvent(prefEvent))
        .ReturnsAsync(0);

      var executor = RequestExecutorContainerFactory.Build<DeletePreferenceKeyExecutor>
       (Logger, ServiceExceptionHandler, mockPrefRepo.Object);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await executor.ProcessAsync(prefEvent));
      Assert.Equal(HttpStatusCode.InternalServerError, ex.Code);
      var result = ex.GetResult;
      Assert.Equal(2007, result.Code);
      Assert.Equal("Unable to delete preference key. ", result.Message);
    }

    [Fact]
    public async Task DeletePreferenceKeyExecutor_HappyPath()
    {
      var keyUid = Guid.NewGuid();

      mockPrefRepo.Setup(p => p.UserPreferenceExistsForKey(keyUid))
             .ReturnsAsync(false);

      var prefEvent = new DeletePreferenceKeyEvent
      {
        PreferenceKeyUID = keyUid
      };
      mockPrefRepo.Setup(p => p.StoreEvent(prefEvent))
        .ReturnsAsync(1);

      var executor = RequestExecutorContainerFactory.Build<DeletePreferenceKeyExecutor>
       (Logger, ServiceExceptionHandler, mockPrefRepo.Object);
      var result = await executor.ProcessAsync(prefEvent);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
    }
  }
}
