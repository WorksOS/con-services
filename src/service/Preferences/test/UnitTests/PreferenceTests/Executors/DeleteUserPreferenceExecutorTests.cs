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
  public class DeleteUserPreferenceExecutorTests : UnitTestsDIFixture<DeleteUserPreferenceExecutorTests>
  {
    public DeleteUserPreferenceExecutorTests()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    [Fact]
    public async Task DeleteUserPreferenceExecutor_NoExisting()
    {
      var prefEvent = new DeleteUserPreferenceEvent
      {
        PreferenceKeyName = "some key",
        PreferenceKeyUID = Guid.NewGuid(),
        UserUID = Guid.NewGuid(),
      };
      mockPrefRepo.Setup(p => p.StoreEvent(prefEvent))
        .ReturnsAsync(0);

      var executor = RequestExecutorContainerFactory.Build<DeleteUserPreferenceExecutor>
       (Logger, ServiceExceptionHandler, mockPrefRepo.Object);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await executor.ProcessAsync(prefEvent));
      Assert.Equal(HttpStatusCode.InternalServerError, ex.Code);
      var result = ex.GetResult;
      Assert.Equal(2012, result.Code);
      Assert.Equal("Unable to delete user preference. ", result.Message);
    }

    [Fact]
    public async Task DeleteUserPreferenceExecutor_HappyPath()
    {
      var prefEvent = new DeleteUserPreferenceEvent
      {
        PreferenceKeyName = "some key",
        PreferenceKeyUID = Guid.NewGuid(),
        UserUID = Guid.NewGuid(),
      };
      mockPrefRepo.Setup(p => p.StoreEvent(prefEvent))
        .ReturnsAsync(1);

      var executor = RequestExecutorContainerFactory.Build<DeleteUserPreferenceExecutor>
       (Logger, ServiceExceptionHandler, mockPrefRepo.Object);
      var result = await executor.ProcessAsync(prefEvent);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
    }
  }
}
