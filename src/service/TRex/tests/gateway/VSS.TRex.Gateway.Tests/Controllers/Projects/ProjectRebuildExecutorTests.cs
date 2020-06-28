using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.AWS.TransferProxy;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Models.Projects;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Executors.Project;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.Projects
{
  public class ProjectRebuildExecutorTests : IClassFixture<DILoggingFixture>
  {
    [Theory]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", false, TransferProxyType.TAGFiles)]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", true, TransferProxyType.TAGFiles)]
    public void ProjectRebuildRequestValidation_HappyPath(string projectUid, bool archiveTagFiles, TransferProxyType dataOrigin)
    {
      var request = new ProjectRebuildRequest(Guid.Parse(projectUid), dataOrigin, archiveTagFiles);
      request.Validate();
    }

    [Theory]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", false, TransferProxyType.DesignImport, -1, "Data origin must be ''TAGFiles''*")]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", true, TransferProxyType.Temporary, -1, "Data origin must be ''TAGFiles''*")]
    public void ProjectRebuildRequestValidation_Errors(string projectUid, bool archiveTagFiles, TransferProxyType dataOrigin, int expectedCode, string expectedMessage)
    {
      var request = new ProjectRebuildRequest(Guid.Parse(projectUid), dataOrigin, archiveTagFiles);

      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      ex.GetResult.Code.Should().Be(expectedCode);
      ex.GetResult.Message.Should().Match(expectedMessage);
    }

    [Fact]
    public void Creation()
    {
      var executor = new ProjectRebuildExecutor();
      executor.Should().NotBeNull();
    }

    [Fact]
    public void CastFailure_Sync()
    {
      var executor = new ProjectRebuildExecutor();
      var ex = Assert.Throws<ServiceException>(() => executor.Process(new object()));
      ex.GetResult.Code.Should().Be(-3);
      ex.GetResult.Message.Should().Match("ProjectRebuildRequest cast failed.");
    }

    [Fact]
    public async void CastFailure_Async()
    {
      var executor = new ProjectRebuildExecutor();
      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await executor.ProcessAsync(new object()));
      ex.GetResult.Code.Should().Be(-3);
      ex.GetResult.Message.Should().Match("ProjectRebuildRequest cast failed.");
    }

    [Fact]
    public void CallsRebuildManager_Sync()
    {
      var mockManager = new Mock<ISiteModelRebuilderManager>();
      mockManager.Setup(x => x.Rebuild(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<TransferProxyType>())).Returns(true);

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton(mockManager.Object))
        .Complete();

      var executor = new ProjectRebuildExecutor(DIContext.Obtain<IConfigurationStore>(), DIContext.Obtain<ILoggerFactory>(), new Mock<IServiceExceptionHandler>().Object);
      executor.Process(new ProjectRebuildRequest(Guid.NewGuid(), TransferProxyType.TAGFiles, false));

      mockManager.Verify(x => x.Rebuild(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<TransferProxyType>()), Times.Once);
    }

    [Fact]
    public async void CallsRebuildManager_Async()
    {
      var mockManager = new Mock<ISiteModelRebuilderManager>();
      mockManager.Setup(x => x.Rebuild(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<TransferProxyType>())).Returns(true);

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton(mockManager.Object))
        .Complete();

      var executor = new ProjectRebuildExecutor(DIContext.Obtain<IConfigurationStore>(), DIContext.Obtain<ILoggerFactory>(), new Mock<IServiceExceptionHandler>().Object);
      await executor.ProcessAsync(new ProjectRebuildRequest(Guid.NewGuid(), TransferProxyType.TAGFiles, false));

      mockManager.Verify(x => x.Rebuild(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<TransferProxyType>()), Times.Once);
    }
  }
}
