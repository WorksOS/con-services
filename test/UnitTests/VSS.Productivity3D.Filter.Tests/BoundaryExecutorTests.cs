using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class BoundaryExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public async Task GetBoundaryExecutor()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string boundaryUid = Guid.NewGuid().ToString();
      string name = "blah";
      string geometryWKT = "whatever";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
      var geofence = new Geofence
      {
        CustomerUID = custUid,
        UserUID = userUid,
        GeofenceUID = boundaryUid,
        Name = name,
        GeometryWKT = geometryWKT,
        GeofenceType = GeofenceType.Filter,
        LastActionedUTC = DateTime.UtcNow
      };
      geofenceRepo.As<IGeofenceRepository>().Setup(g => g.GetGeofence(It.IsAny<string>())).ReturnsAsync(geofence);

      var projectRepo = new Mock<ProjectRepository>(configStore, logger);
      var projectGeofence = new ProjectGeofence {GeofenceUID = boundaryUid, ProjectUID = projectUid};
      projectRepo.As<IProjectRepository>().Setup(p => p.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(new List<ProjectGeofence> {projectGeofence});

      var geofenceToTest = new GeofenceDataSingleResult(AutoMapperUtility.Automapper.Map<GeofenceData>(geofence));

      var request = BoundaryUidRequestFull.Create
      (
        custUid,
        false,
        new ProjectData() { ProjectUid = projectUid },
        userUid,
        boundaryUid
      );

      var executor =
        RequestExecutorContainer.Build<GetBoundaryExecutor>(configStore, logger, serviceExceptionHandler,
          geofenceRepo.Object, projectRepo.Object);
      var result = await executor.ProcessAsync(request) as GeofenceDataSingleResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(geofenceToTest.GeofenceData.GeofenceUID, result.GeofenceData.GeofenceUID,
        "executor returned incorrect GeofenceUID");
      Assert.AreEqual(geofenceToTest.GeofenceData.GeofenceName, result.GeofenceData.GeofenceName,
        "executor returned incorrect GeofenceName");
      Assert.AreEqual(geofenceToTest.GeofenceData.GeometryWKT, result.GeofenceData.GeometryWKT,
        "executor returned incorrect GeometryWKT");
    }

    [TestMethod]
    public async Task GetBoundariesExecutor()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string boundaryUid = Guid.NewGuid().ToString();
      string name = "blah";
      string geometryWKT = "whatever";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
      var geofence = new Geofence
      {
        CustomerUID = custUid,
        UserUID = userUid,
        GeofenceUID = boundaryUid,
        Name = name,
        GeometryWKT = geometryWKT,
        GeofenceType = GeofenceType.Filter,
        LastActionedUTC = DateTime.UtcNow
      };
      geofenceRepo.As<IGeofenceRepository>().Setup(g => g.GetGeofences(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<Geofence>{geofence});

      var projectRepo = new Mock<ProjectRepository>(configStore, logger);
      var projectGeofence = new ProjectGeofence { GeofenceUID = boundaryUid, ProjectUID = projectUid };
      projectRepo.As<IProjectRepository>().Setup(p => p.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(new List<ProjectGeofence> { projectGeofence });

      var geofenceToTest = AutoMapperUtility.Automapper.Map<GeofenceData>(geofence);

      var request = BaseRequestFull.Create
      (
        custUid,
        false,
        new ProjectData() { ProjectUid = projectUid },
        userUid
      );

      var executor =
        RequestExecutorContainer.Build<GetBoundariesExecutor>(configStore, logger, serviceExceptionHandler,
          geofenceRepo.Object, projectRepo.Object);
      var result = await executor.ProcessAsync(request) as GeofenceDataListResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(1, result.GeofenceData.Count, "executor returned wrong boundary count");
      Assert.AreEqual(geofenceToTest.GeofenceUID, result.GeofenceData[0].GeofenceUID,
        "executor returned incorrect GeofenceUID");
      Assert.AreEqual(geofenceToTest.GeofenceName, result.GeofenceData[0].GeofenceName,
        "executor returned incorrect GeofenceName");
      Assert.AreEqual(geofenceToTest.GeometryWKT, result.GeofenceData[0].GeometryWKT,
        "executor returned incorrect GeometryWKT");
    }

    [TestMethod]
    public async Task UpsertBoundaryExecutor()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string name = "not entry";
      string geometryWKT = "whatever";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
      var geofence = new Geofence
      {
        CustomerUID = custUid,
        UserUID = userUid,
        Name = name,
        GeometryWKT = geometryWKT,
        GeofenceType = GeofenceType.Filter,
        LastActionedUTC = DateTime.UtcNow
      };
      geofenceRepo.As<IGeofenceRepository>().Setup(g => g.GetGeofence(It.IsAny<string>())).ReturnsAsync(geofence);
      geofenceRepo.As<IGeofenceRepository>().Setup(g => g.StoreEvent(It.IsAny<CreateGeofenceEvent>())).ReturnsAsync(1);

      var projectRepo = new Mock<ProjectRepository>(configStore, logger);
      projectRepo.As<IProjectRepository>().Setup(p => p.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(new List<ProjectGeofence>());
      projectRepo.As<IProjectRepository>().Setup(p => p.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);

      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseDataResult());

      var producer = new Mock<IKafka>();
      string kafkaTopicName = "whatever";

      var request = BoundaryRequestFull.Create
      (
        custUid,
        false,
        new ProjectData() { ProjectUid = projectUid },
        userUid,
        new BoundaryRequest {BoundaryUid = null, Name = name, BoundaryPolygonWKT = geometryWKT}
      );

      var executor =
        RequestExecutorContainer.Build<UpsertBoundaryExecutor>(configStore, logger, serviceExceptionHandler,
          geofenceRepo.Object, projectRepo.Object, null, raptorProxy.Object, producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as GeofenceDataSingleResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(name, result.GeofenceData.GeofenceName,
        "executor returned incorrect GeofenceName");
      Assert.AreEqual(geometryWKT, result.GeofenceData.GeometryWKT,
        "executor returned incorrect GeometryWKT");
    }

    [TestMethod]
    public async Task DeleteBoundaryExecutor()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string boundaryUid = Guid.NewGuid().ToString();
      string name = "not entry";
      string geometryWKT = "whatever";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
      var geofence = new Geofence
      {
        CustomerUID = custUid,
        UserUID = userUid,
        GeofenceUID = boundaryUid,
        Name = name,
        GeometryWKT = geometryWKT,
        GeofenceType = GeofenceType.Filter,
        LastActionedUTC = DateTime.UtcNow
      };
      geofenceRepo.As<IGeofenceRepository>().Setup(g => g.GetGeofence(It.IsAny<string>())).ReturnsAsync(geofence);
      geofenceRepo.As<IGeofenceRepository>().Setup(g => g.StoreEvent(It.IsAny<DeleteGeofenceEvent>())).ReturnsAsync(1);

      var projectRepo = new Mock<ProjectRepository>(configStore, logger);
      var projectGeofence = new ProjectGeofence { GeofenceUID = boundaryUid, ProjectUID = projectUid };
      projectRepo.As<IProjectRepository>().Setup(p => p.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(new List<ProjectGeofence> { projectGeofence });

      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseDataResult());

      var producer = new Mock<IKafka>();
      string kafkaTopicName = "whatever";

      var request = BoundaryUidRequestFull.Create
      (
        custUid,
        false,
        new ProjectData() { ProjectUid = projectUid },
        userUid,
        boundaryUid
      );

      var executor =
        RequestExecutorContainer.Build<DeleteBoundaryExecutor>(configStore, logger, serviceExceptionHandler,
          geofenceRepo.Object, projectRepo.Object, null, raptorProxy.Object, producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(request);

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(0, result.Code, "executor returned incorrect result code");
      Assert.AreEqual("success", result.Message, "executor returned incorrect result message");
    }
  }
}
