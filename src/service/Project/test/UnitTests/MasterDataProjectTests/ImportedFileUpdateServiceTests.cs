using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Project.WebAPI.Common.Services;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity.Push.Models.Notifications.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Serilog.Extensions;
using Xunit;

namespace VSS.MasterData.ProjectTests
{
  public class ImportedFileUpdateServiceTests
  {
    private readonly IServiceProvider ServiceProvider;
    private readonly Mock<IProjectRepository> projectMock = new Mock<IProjectRepository>();

    public ImportedFileUpdateServiceTests()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Project.WebApi.log"));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.AddSingleton(new Mock<IConfigurationStore>().Object);
      serviceCollection.AddSingleton(new Mock<IServiceResolution>().Object);
      serviceCollection.AddSingleton(new Mock<IServiceExceptionHandler>().Object);
      serviceCollection.AddSingleton(projectMock.Object);
      serviceCollection.AddTransient<INotificationHubClient, NotificationHubClient>(); // We are testing this will call the ImportedFileUpdateService 

      ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public void EnsureUpdateZoomLevelsIsCalled()
    {
      var notificationHub = ServiceProvider.GetService<INotificationHubClient>() as NotificationHubClient;
      Assert.NotNull(notificationHub);

      var rasterTileNotificationParameters = new RasterTileNotificationParameters
      {
        FileUid = Guid.Parse("ED279023-6A51-45B7-B4D0-2A5BF1ECA60C")
      };
      
      // We don't want a file to be returned, we just want to validate this is called
      projectMock
        .Setup(m => m.GetImportedFile(It.IsAny<string>()))
        .Returns(Task.FromResult<ImportedFile>(null));

      // We have to make sure this class is loaded into the Assemblies List, or else the notification hub won't find it
      var service = ActivatorUtilities.CreateInstance<ImportedFileUpdateService>(ServiceProvider);
      Assert.NotNull(service); 

      // Generate an event, that will trigger a call to project repo for the file
      var tasks = notificationHub.ProcessNotificationAsTasks(new ProjectFileRasterTilesGeneratedNotification(rasterTileNotificationParameters));

      // Ensure the tasks complete
      Task.WaitAll(tasks.ToArray());

      projectMock.Verify(m =>
        m.GetImportedFile(
          It.Is<string>(s => Guid.Parse(s) == rasterTileNotificationParameters.FileUid)), Times.Once);

    }
  }
}
