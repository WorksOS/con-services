using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.AssetMgmt3D.Abstractions;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Project.Repository;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class BoundaryExecutorTests : ExecutorBaseTests
  {
    private IAssetResolverProxy _assetResolverProxy;

    [TestInitialize]
    public void TestInit()
    {
      var mockedAssetResolverProxySetup = new Mock<IAssetResolverProxy>();
      mockedAssetResolverProxySetup.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(new List<KeyValuePair<Guid, long>>(0));
      mockedAssetResolverProxySetup.Setup(x => x.GetMatchingAssets(It.IsAny<List<long>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(new List<KeyValuePair<Guid, long>>(0));

      _assetResolverProxy = mockedAssetResolverProxySetup.Object;
    }

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
    public async Task GetBoundariesExecutor_WithFavorites()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string boundaryUid = Guid.NewGuid().ToString();
      string boundaryName = "blah";
      string boundaryGeometryWKT = "whatever";
      Guid favoriteUidInside = Guid.NewGuid();
      string favoriteNameInside = "favorite blah";
      string favoriteGeometryWKTInside = "who cares";
      Guid favoriteUidOutside = Guid.NewGuid();
      string favoriteNameOutside = "more favorite blah";
      string favoriteGeometryWKTOutside = "still don't care";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
      var geofenceBoundary = new Geofence
      {
        CustomerUID = custUid,
        UserUID = userUid,
        GeofenceUID = boundaryUid,
        Name = boundaryName,
        GeometryWKT = boundaryGeometryWKT,
        GeofenceType = GeofenceType.Filter,
        LastActionedUTC = DateTime.UtcNow
      };
      geofenceRepo.As<IGeofenceRepository>().Setup(g => g.GetGeofences(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<Geofence>{geofenceBoundary});

      var favoriteGeofenceInside = new GeofenceData
      {
        GeofenceUID = favoriteUidInside,
        GeofenceName = favoriteNameInside,
        GeometryWKT = favoriteGeometryWKTInside,
        GeofenceType = GeofenceType.Generic.ToString(),
        CustomerUID = Guid.Parse(custUid)
      };
      var favoriteGeofenceOutside = new GeofenceData
      {
        GeofenceUID = favoriteUidOutside,
        GeofenceName = favoriteNameOutside,
        GeometryWKT = favoriteGeometryWKTOutside,
        GeofenceType = GeofenceType.Generic.ToString(),
        CustomerUID = Guid.Parse(custUid)
      };
      var favorites = new List<GeofenceData>{favoriteGeofenceInside, favoriteGeofenceOutside};
      var geofenceProxy = new Mock<IGeofenceProxy>();
      geofenceProxy.Setup(g => g.GetFavoriteGeofences(custUid, userUid, null)).ReturnsAsync(favorites);

      var upProxy = new Mock<IUnifiedProductivityProxy>();
      upProxy.Setup(u => u.GetAssociatedGeofences(projectUid, null)).ReturnsAsync((List<GeofenceData>)null);

      var projectRepo = new Mock<ProjectRepository>(configStore, logger);
      var projectGeofence = new ProjectGeofence { GeofenceUID = boundaryUid, ProjectUID = projectUid };
      projectRepo.As<IProjectRepository>().Setup(p => p.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(new List<ProjectGeofence> { projectGeofence });
      projectRepo.As<IProjectRepository>().Setup(p => p.DoPolygonsOverlap(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<bool> { true, false });

      var geofenceToTest = AutoMapperUtility.Automapper.Map<GeofenceData>(geofenceBoundary);

      var request = BaseRequestFull.Create
      (
        custUid,
        false,
        new ProjectData() { ProjectUid = projectUid },
        userUid,
        null
      );

      var executor =
        RequestExecutorContainer.Build<GetBoundariesExecutor>(configStore, logger, serviceExceptionHandler,
          geofenceRepo.Object, projectRepo.Object, geofenceProxy: geofenceProxy.Object, 
          unifiedProductivityProxy: upProxy.Object);
      var result = await executor.ProcessAsync(request) as GeofenceDataListResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(2, result.GeofenceData.Count, "executor returned wrong boundary count");
      var actualBoundary = result.GeofenceData.SingleOrDefault(g => g.GeofenceUID == geofenceToTest.GeofenceUID);
      Assert.AreEqual(geofenceToTest.GeofenceUID, actualBoundary.GeofenceUID,
        "executor returned incorrect boundary GeofenceUID");
      Assert.AreEqual(geofenceToTest.GeofenceName, actualBoundary.GeofenceName,
        "executor returned incorrect boundary GeofenceName");
      Assert.AreEqual(geofenceToTest.GeometryWKT, actualBoundary.GeometryWKT,
        "executor returned incorrect boundary GeometryWKT");
      Assert.AreEqual(geofenceToTest.GeofenceType, actualBoundary.GeofenceType,
        "executor returned incorrect boundary GeofenceType");
      var actualFavorite = result.GeofenceData.SingleOrDefault(g => g.GeofenceUID == favoriteGeofenceInside.GeofenceUID);
      Assert.IsNotNull(actualFavorite, "missing favorite geofence");
      Assert.AreEqual(favoriteGeofenceInside.GeofenceUID, actualFavorite.GeofenceUID,
        "executor returned incorrect favorite GeofenceUID");
      Assert.AreEqual(favoriteGeofenceInside.GeofenceName, actualFavorite.GeofenceName,
        "executor returned incorrect favorite GeofenceName");
      Assert.AreEqual(favoriteGeofenceInside.GeometryWKT, actualFavorite.GeometryWKT,
        "executor returned incorrect favorite GeometryWKT");
      Assert.AreEqual(favoriteGeofenceInside.GeofenceType, actualFavorite.GeofenceType,
        "executor returned incorrect favorite GeofenceType");
    }

    [TestMethod]
    public async Task GetBoundariesExecutor_WithAssociatedGeofences()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string boundaryUid = Guid.NewGuid().ToString();
      string boundaryName = "blah";
      string boundaryGeometryWKT = "whatever";
      Guid associatedUid = Guid.NewGuid();
      string associatedName = "favorite blah";
      string associatedGeometryWKT = "who cares";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
      var geofenceBoundary = new Geofence
      {
        CustomerUID = custUid,
        UserUID = userUid,
        GeofenceUID = boundaryUid,
        Name = boundaryName,
        GeometryWKT = boundaryGeometryWKT,
        GeofenceType = GeofenceType.Filter,
        LastActionedUTC = DateTime.UtcNow
      };
      geofenceRepo.As<IGeofenceRepository>().Setup(g => g.GetGeofences(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<Geofence> { geofenceBoundary });
   
      var geofenceProxy = new Mock<IGeofenceProxy>();
      geofenceProxy.Setup(g => g.GetFavoriteGeofences(custUid, userUid, null)).ReturnsAsync(new List<GeofenceData>());

      var associatedGeofence = new GeofenceData
      {
        GeofenceUID = associatedUid,
        GeofenceName = associatedName,
        GeometryWKT = associatedGeometryWKT,
        GeofenceType = GeofenceType.Generic.ToString(),
        CustomerUID = Guid.Parse(custUid)
      };
      var upProxy = new Mock<IUnifiedProductivityProxy>();
      upProxy.Setup(u => u.GetAssociatedGeofences(projectUid, null)).ReturnsAsync(new List<GeofenceData>{associatedGeofence});

      var projectRepo = new Mock<ProjectRepository>(configStore, logger);
      var projectGeofence = new ProjectGeofence { GeofenceUID = boundaryUid, ProjectUID = projectUid };
      projectRepo.As<IProjectRepository>().Setup(p => p.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(new List<ProjectGeofence> { projectGeofence });
      projectRepo.As<IProjectRepository>().Setup(p => p.DoPolygonsOverlap(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<bool> { true, false });

      var geofenceToTest = AutoMapperUtility.Automapper.Map<GeofenceData>(geofenceBoundary);

      var request = BaseRequestFull.Create
      (
        custUid,
        false,
        new ProjectData() { ProjectUid = projectUid },
        userUid,
        null
      );

      var executor =
        RequestExecutorContainer.Build<GetBoundariesExecutor>(configStore, logger, serviceExceptionHandler,
          geofenceRepo.Object, projectRepo.Object, geofenceProxy: geofenceProxy.Object,
          unifiedProductivityProxy: upProxy.Object);
      var result = await executor.ProcessAsync(request) as GeofenceDataListResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(2, result.GeofenceData.Count, "executor returned wrong boundary count");
      var actualBoundary = result.GeofenceData.SingleOrDefault(g => g.GeofenceUID == geofenceToTest.GeofenceUID);
      Assert.AreEqual(geofenceToTest.GeofenceUID, actualBoundary.GeofenceUID,
        "executor returned incorrect boundary GeofenceUID");
      Assert.AreEqual(geofenceToTest.GeofenceName, actualBoundary.GeofenceName,
        "executor returned incorrect boundary GeofenceName");
      Assert.AreEqual(geofenceToTest.GeometryWKT, actualBoundary.GeometryWKT,
        "executor returned incorrect boundary GeometryWKT");
      Assert.AreEqual(geofenceToTest.GeofenceType, actualBoundary.GeofenceType,
        "executor returned incorrect boundary GeofenceType");
      var actualAssociated = result.GeofenceData.SingleOrDefault(g => g.GeofenceUID == associatedGeofence.GeofenceUID);
      Assert.IsNotNull(actualAssociated, "missing associated geofence");
      Assert.AreEqual(associatedGeofence.GeofenceUID, actualAssociated.GeofenceUID,
        "executor returned incorrect associated GeofenceUID");
      Assert.AreEqual(associatedGeofence.GeofenceName, actualAssociated.GeofenceName,
        "executor returned incorrect associated GeofenceName");
      Assert.AreEqual(associatedGeofence.GeometryWKT, actualAssociated.GeometryWKT,
        "executor returned incorrect associated GeometryWKT");
      Assert.AreEqual(associatedGeofence.GeofenceType, actualAssociated.GeofenceType,
        "executor returned incorrect associated GeofenceType");
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
          geofenceRepo.Object, projectRepo.Object, null, raptorProxy.Object, _assetResolverProxy, producer.Object, kafkaTopicName);
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
          geofenceRepo.Object, projectRepo.Object, null, raptorProxy.Object, _assetResolverProxy, producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(request);

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(0, result.Code, "executor returned incorrect result code");
      Assert.AreEqual("success", result.Message, "executor returned incorrect result message");
    }
  }
}
