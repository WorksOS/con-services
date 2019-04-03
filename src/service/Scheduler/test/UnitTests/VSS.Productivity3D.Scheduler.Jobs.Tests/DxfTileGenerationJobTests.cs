using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.Pegasus.Client;
using VSS.Pegasus.Client.Models;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Push.Clients;
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
    public void CanSetupJob()
    {
      var job = CreateJobWithMocks();
      job.Setup(null);
    }

    [TestMethod]
    public void CanTearDownJob()
    {
      var job = CreateJobWithMocks();
      job.TearDown(null);
    }

    [TestMethod]
    public async Task CanRunJobSuccess()
    {
      var request = new DxfTileGenerationRequest
      {
        CustomerUid = Guid.NewGuid(), ProjectUid = Guid.NewGuid(), ImportedFileUid = Guid.NewGuid(),
        DataOceanRootFolder = "some folder", DxfFileName = "a dxf file", DcFileName = "a dc file", DxfUnitsType = DxfUnitsType.Meters
      };
      var obj = JObject.Parse(JsonConvert.SerializeObject(request));
      var mockPegasus = new Mock<IPegasusClient>();
      mockPegasus.Setup(p => p.GenerateDxfTiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DxfUnitsType>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(new TileMetadata());
      var mockNotification = new Mock<INotificationHubClient>();
      mockNotification.Setup(n => n.Notify(It.IsAny<ProjectFileDxfTilesGeneratedNotification>()))
        .Returns(Task.FromResult(default(object)));
      var mockTPaaSAuth = new Mock<ITPaaSApplicationAuthentication>();
      mockTPaaSAuth.Setup(t => t.GetApplicationBearerToken()).Returns("this is a dummy bearer token");
      var job = new DxfTileGenerationJob(mockPegasus.Object, mockTPaaSAuth.Object, mockNotification.Object, loggerFactory);
      await job.Run(obj);
    }

    [TestMethod]
    [ExpectedException(typeof(ServiceException))]//Assert.ThrowsException doesn't work so use this instead

    public async Task CanRunJobFailureMissingRequest()
    {
      var job = CreateJobWithMocks();
      await job.Run(null);
    }

    [TestMethod]
    [ExpectedException(typeof(ServiceException))]//Assert.ThrowsException doesn't work so use this instead
    public async Task CanRunJobFailureWrongRequest()
    {
      var badRequest = new JobRequest();//any model which is not DxfTileGenerationRequest
      var obj = JObject.Parse(JsonConvert.SerializeObject(badRequest));
      var job = CreateJobWithMocks();
      await job.Run(obj);
    }

    private DxfTileGenerationJob CreateJobWithMocks()
    {
      var mockPegasus = new Mock<IPegasusClient>();
      var mockTPaaSAuth = new Mock<ITPaaSApplicationAuthentication>();
      var mockProvider = new Mock<IServiceProvider>();
      var mockConfig = new Mock<IConfigurationStore>();
      var mockPushProxy = new Mock<IServiceResolution>();
      var mockNotification = new Mock<NotificationHubClient>(mockProvider.Object, mockConfig.Object, mockPushProxy.Object, loggerFactory);
      var job = new DxfTileGenerationJob(mockPegasus.Object, mockTPaaSAuth.Object, mockNotification.Object, loggerFactory);
      return job;
    }
  }
}
