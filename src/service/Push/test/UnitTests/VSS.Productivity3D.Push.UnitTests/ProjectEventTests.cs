using System;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections.Internal;
using Microsoft.AspNetCore.SignalR.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Push.Abstractions.UINotifications;
using VSS.Productivity3D.Push.Clients.UINotification;
using VSS.Productivity3D.Push.Hubs;
using VSS.Serilog.Extensions;
using VSS.MasterData.Models.Models;
using VSS.Productivity.Push.Models.Notifications.Models;
using VSS.Productivity3D.Push.Hubs.Authentication;

namespace VSS.Productivity3D.Push.UnitTests
{
  public class ProjectEventTests
  {
    private readonly IServiceProvider _serviceProvider;

    public ProjectEventTests()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Push.UnitTests.log"));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging()
        .AddSingleton(loggerFactory)
        .AddSingleton(new Mock<IConfigurationStore>().Object)
        .AddSingleton(new Mock<IServiceResolution>().Object)
        .AddTransient<IProjectEventHub, ProjectEventHub>()
        .AddTransient<IProjectEventHubClient, ProjectEventHubClient>();

      _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public async Task ProjectEventClientReportFileImportCompleteMock()
    {
      var projectEventHubClient = new Mock<IProjectEventHubClient>();
      projectEventHubClient.Setup(c => c.FileImportIsComplete(It.IsAny<ImportedFileStatus>())).Returns(Task.CompletedTask);
      var clientObj = projectEventHubClient.Object;

      var importedFileStatus = new ImportedFileStatus(Guid.NewGuid(), Guid.NewGuid());
      await clientObj.FileImportIsComplete(importedFileStatus);
    }

    [Fact]
    public async Task ProjectEventClientReportFileImportComplete()
    {
      var projectEventHubClient = _serviceProvider.GetService<IProjectEventHubClient>();
      Assert.NotNull(projectEventHubClient);

      var importedFileStatus = new ImportedFileStatus(Guid.NewGuid(), Guid.NewGuid());
      await projectEventHubClient.FileImportIsComplete(importedFileStatus);
    }

    [Fact]
    public async Task ProjectEventPushHubStartMock()
    {
      var projectEventHub = new Mock<IProjectEventHub>();
      projectEventHub.Setup(p => p.StartProcessingProject(It.IsAny<Guid>())).Returns(Task.CompletedTask);
      var pushHubObj = projectEventHub.Object;

      await pushHubObj.StartProcessingProject(Guid.NewGuid());
    }

    [Fact]
    public async Task ProjectEventHubMock()
    {
      var projectUid = Guid.NewGuid();
      var groupName = $"ProjectEventHub-{projectUid}";
      var loggerFactory = _serviceProvider.GetService<ILoggerFactory>();

      var mockClients = new Mock<IHubCallerClients<IProjectEventClientHubContext>>();
      var mockProjectEventClientHubProxy = new Mock<IProjectEventClientHubContext>();
      var groups = new Mock<IProjectEventClientHubContext>();
      //var groupManager = new Mock<IGroupManager>();
      //groupManager.Setup(gm => gm.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));

      var importedFileStatus = new ImportedFileStatus(projectUid);
      mockClients.Setup(clients => clients.All).Returns(mockProjectEventClientHubProxy.Object);
      mockClients.Setup(clients => clients.Group(It.IsAny<string>())).Returns(mockProjectEventClientHubProxy.Object);
      groups.Setup(g => g.OnFileImportCompleted(importedFileStatus)).Verifiable();
      mockClients.Setup(clients => clients.Group(groupName)).Returns(groups.Object);

      var mockProjectProxy = new Mock<IProjectProxy>();
      var projectData = new ProjectData { ProjectUid = projectUid.ToString(), LegacyProjectId = new Random().Next() };
      var contextHeaders = new Dictionary<string, string>();
      mockProjectProxy.Setup(proxy => proxy.GetProjectForCustomer(It.IsAny<string>(), projectUid.ToString(), contextHeaders)).ReturnsAsync(projectData);
      var httpConnectionContext = new HttpConnectionContext(Guid.NewGuid().ToString(), loggerFactory.CreateLogger("ProjectEventTests"));
      httpConnectionContext.User = new PushPrincipal(new ClaimsIdentity(), Guid.NewGuid().ToString(), "customerName", "merino@vss.com", true, "3D Productivity", mockProjectProxy.Object, contextHeaders);

      var projectEventHub = new ProjectEventHub(_serviceProvider.GetService<ILoggerFactory>());
      projectEventHub.Clients = mockClients.Object;
      projectEventHub.Context =
        new DefaultHubCallerContext
        (new HubConnectionContext(httpConnectionContext, TimeSpan.MaxValue, loggerFactory, TimeSpan.MaxValue));


      // need to find a way to mock Groups which is an extension method, i.e. difficult to mock
      await Assert.ThrowsAsync<NullReferenceException>(async () => await projectEventHub.StartProcessingProject(projectUid).ConfigureAwait(false));
      mockProjectProxy.Verify(clients => clients.GetProjectForCustomer(It.IsAny<string>(), projectUid.ToString(), contextHeaders), Times.Once);
      await Assert.ThrowsAsync<NullReferenceException>(async () => await projectEventHub.SendImportedFileEventToClients(importedFileStatus).ConfigureAwait(false));

      var mockClientProxy = new Mock<IClientProxy>();
      mockClientProxy.Verify(
        clientProxy => clientProxy.SendCoreAsync(
          "OnFileImportCompleted",
          It.Is<object[]>(o => o != null && o.Length == 1 && ((object[])o[0]).Length == 1),
          default(CancellationToken)),
        Times.Never);
    }

  }
}
