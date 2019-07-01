using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Exceptions;
using VSS.Pegasus.Client;
using VSS.Pegasus.Client.Models;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob.Models;
using VSS.Productivity3D.Scheduler.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Scheduler.Jobs.Tests
{
  [TestClass]
  public class DxfTileGenerationJobTests
  {
    private ILoggerFactory loggerFactory;
    private IServiceProvider serviceProvider;

    [TestInitialize]
    public void TestInitialize()
    {
      var services = new ServiceCollection();
      serviceProvider = services
        .AddLogging()
        .BuildServiceProvider();

      loggerFactory = serviceProvider.GetService<ILoggerFactory>();
    }

    [TestMethod]
    public void CanSetupJob() => CreateJobWithMocks().Setup(null);

    [TestMethod]
    public void CanTearDownJob() => CreateJobWithMocks().TearDown(null);

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task CanRunJobSuccess(bool enableDxfTileGeneration)
    {
      var request = new DxfTileGenerationRequest
      {
        CustomerUid = Guid.NewGuid(),
        ProjectUid = Guid.NewGuid(),
        ImportedFileUid = Guid.NewGuid(),
        DataOceanRootFolder = "some folder",
        FileName = "a dxf file",
        DcFileName = "a dc file",
        DxfUnitsType = DxfUnitsType.Meters
      };

      var obj = JObject.Parse(JsonConvert.SerializeObject(request));
      var configStore = new Mock<IConfigurationStore>();

      configStore.Setup(x => x.GetValueBool("SCHEDULER_ENABLE_DXF_TILE_GENERATION"))
                 .Returns(enableDxfTileGeneration);

      var mockPegasus = new Mock<IPegasusClient>();

      mockPegasus.Setup(x => x.GenerateDxfTiles(
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           DxfUnitsType.Meters,
                           It.IsAny<Dictionary<string, string>>()))
                 .ReturnsAsync(new TileMetadata());

      var mockNotification = new Mock<INotificationHubClient>();

      mockNotification.Setup(n => n.Notify(It.IsAny<ProjectFileRasterTilesGeneratedNotification>()))
                      .Returns(Task.FromResult(default(object)));

      var mockTPaaSAuth = new Mock<ITPaaSApplicationAuthentication>();

      mockTPaaSAuth.Setup(t => t.GetApplicationBearerToken())
                   .Returns("this is a dummy bearer token");

      var job = new DxfTileGenerationJob(configStore.Object, mockPegasus.Object, mockTPaaSAuth.Object, mockNotification.Object, loggerFactory);

      await job.Run(obj);

      var runTimes = enableDxfTileGeneration ? Times.Once() : Times.Never();

      // Verify based on the value of SCHEDULER_ENABLE_DXF_TILE_GENERATION the execution of GenerateDxfTiles().
      mockPegasus.Verify(x => x.GenerateDxfTiles(
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           DxfUnitsType.Meters,
                           It.IsAny<Dictionary<string, string>>()), runTimes);
    }

    [TestMethod]
    public async Task CanRunJobFailureMissingRequest() => await Assert.ThrowsExceptionAsync<ServiceException>(() => CreateJobWithMocks().Run(null));

    [TestMethod]
    public async Task CanRunJobFailureWrongRequest()
    {
      var obj = JObject.Parse(JsonConvert.SerializeObject(new JobRequest())); //any model which is not DxfTileGenerationRequest

      await Assert.ThrowsExceptionAsync<ServiceException>(() => CreateJobWithMocks().Run(obj));
    }

    private DxfTileGenerationJob CreateJobWithMocks()
    {
      var configStore = new Mock<IConfigurationStore>();
      var mockPegasus = new Mock<IPegasusClient>();
      var mockTPaaSAuth = new Mock<ITPaaSApplicationAuthentication>();
      var mockProvider = new Mock<IServiceProvider>();
      var mockConfig = new Mock<IConfigurationStore>();
      var mockPushProxy = new Mock<IServiceResolution>();
      var mockNotification = new Mock<NotificationHubClient>(mockProvider.Object, mockConfig.Object, mockPushProxy.Object, loggerFactory);

      return new DxfTileGenerationJob(configStore.Object, mockPegasus.Object, mockTPaaSAuth.Object, mockNotification.Object, loggerFactory);
    }
  }
}
