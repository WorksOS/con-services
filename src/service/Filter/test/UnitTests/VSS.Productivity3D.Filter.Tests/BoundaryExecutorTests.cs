using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.Productivity3D.Filter.Repository;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Project.Repository;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
using Xunit;

namespace VSS.Productivity3D.Filter.Tests
{
  public class BoundaryExecutorTests : ExecutorBaseTests
  {
    [Fact]
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
      var projectGeofence = new ProjectGeofence { GeofenceUID = boundaryUid, ProjectUID = projectUid };
      projectRepo.As<IProjectRepository>().Setup(p => p.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(new List<ProjectGeofence> { projectGeofence });

      var geofenceToTest = new GeofenceDataSingleResult(AutoMapperUtility.Automapper.Map<GeofenceData>(geofence));

      var request = BoundaryUidRequestFull.Create
      (
        custUid,
        false,
        new ProjectData { ProjectUID = projectUid },
        userUid,
        boundaryUid
      );

      var executor =
        RequestExecutorContainer.Build<GetBoundaryExecutor>(configStore, logger, serviceExceptionHandler,
          geofenceRepo.Object, projectRepo.Object);
      var result = await executor.ProcessAsync(request) as GeofenceDataSingleResult;

      Assert.NotNull(result);
      Assert.Equal(geofenceToTest.GeofenceData.GeofenceUID, result.GeofenceData.GeofenceUID);
      Assert.Equal(geofenceToTest.GeofenceData.GeofenceName, result.GeofenceData.GeofenceName);
      Assert.Equal(geofenceToTest.GeofenceData.GeometryWKT, result.GeofenceData.GeometryWKT);
    }

    // ccss don't have a geofence service yet
    //[Fact]
    //public async Task GetBoundariesExecutor_WithFavorites()
    //{
    //  string custUid = Guid.NewGuid().ToString();
    //  string userUid = Guid.NewGuid().ToString();
    //  string projectUid = Guid.NewGuid().ToString();
    //  string boundaryUid = Guid.NewGuid().ToString();
    //  string boundaryName = "blah";
    //  string boundaryGeometryWKT = "whatever";
    //  Guid favoriteUidInside = Guid.NewGuid();
    //  string favoriteNameInside = "favorite blah";
    //  string favoriteGeometryWKTInside = "who cares";
    //  Guid favoriteUidOutside = Guid.NewGuid();
    //  string favoriteNameOutside = "more favorite blah";
    //  string favoriteGeometryWKTOutside = "still don't care";

    //  var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
    //  var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

    //  var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
    //  var geofenceBoundary = new Geofence
    //  {
    //    CustomerUID = custUid,
    //    UserUID = userUid,
    //    GeofenceUID = boundaryUid,
    //    Name = boundaryName,
    //    GeometryWKT = boundaryGeometryWKT,
    //    GeofenceType = GeofenceType.Filter,
    //    LastActionedUTC = DateTime.UtcNow
    //  };
    //  geofenceRepo.As<IGeofenceRepository>().Setup(g => g.GetGeofences(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<Geofence> { geofenceBoundary });

    //  var favoriteGeofenceInside = new GeofenceData
    //  {
    //    GeofenceUID = favoriteUidInside,
    //    GeofenceName = favoriteNameInside,
    //    GeometryWKT = favoriteGeometryWKTInside,
    //    GeofenceType = GeofenceType.Generic.ToString(),
    //    CustomerUID = Guid.Parse(custUid)
    //  };
    //  var favoriteGeofenceOutside = new GeofenceData
    //  {
    //    GeofenceUID = favoriteUidOutside,
    //    GeofenceName = favoriteNameOutside,
    //    GeometryWKT = favoriteGeometryWKTOutside,
    //    GeofenceType = GeofenceType.Generic.ToString(),
    //    CustomerUID = Guid.Parse(custUid)
    //  };
    //  var favorites = new List<GeofenceData> { favoriteGeofenceInside, favoriteGeofenceOutside };
    //  //var geofenceProxy = new Mock<IGeofenceProxy>();
    //  //geofenceProxy.Setup(g => g.GetFavoriteGeofences(custUid, userUid, null)).ReturnsAsync(favorites);

    //  //var upProxy = new Mock<IUnifiedProductivityProxy>();
    //  //upProxy.Setup(u => u.GetAssociatedGeofences(projectUid, null)).ReturnsAsync((List<GeofenceData>)null);

    //  var projectRepo = new Mock<ProjectRepository>(configStore, logger);
    //  var projectGeofence = new ProjectGeofence { GeofenceUID = boundaryUid, ProjectUID = projectUid };
    //  projectRepo.As<IProjectRepository>().Setup(p => p.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(new List<ProjectGeofence> { projectGeofence });
    //  projectRepo.As<IProjectRepository>().Setup(p => p.DoPolygonsOverlap(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<bool> { true, false });

    //  var geofenceToTest = AutoMapperUtility.Automapper.Map<GeofenceData>(geofenceBoundary);

    //  var request = BaseRequestFull.Create
    //  (
    //    custUid,
    //    false,
    //    new ProjectData { ProjectUID = projectUid },
    //    userUid,
    //    null
    //  );

    //  var executor =
    //    RequestExecutorContainer.Build<GetBoundariesExecutor>(configStore, logger, serviceExceptionHandler,
    //      geofenceRepo.Object, projectRepo.Object
    //      /*, geofenceProxy: geofenceProxy.Object, unifiedProductivityProxy: upProxy.Object */);
    //  var result = await executor.ProcessAsync(request) as GeofenceDataListResult;

    //  Assert.NotNull(result);
    //  Assert.Equal(2, result.GeofenceData.Count);
    //  var actualBoundary = result.GeofenceData.SingleOrDefault(g => g.GeofenceUID == geofenceToTest.GeofenceUID);
    //  Assert.Equal(geofenceToTest.GeofenceUID, actualBoundary.GeofenceUID);
    //  Assert.Equal(geofenceToTest.GeofenceName, actualBoundary.GeofenceName);
    //  Assert.Equal(geofenceToTest.GeometryWKT, actualBoundary.GeometryWKT);
    //  Assert.Equal(geofenceToTest.GeofenceType, actualBoundary.GeofenceType);
    //  var actualFavorite = result.GeofenceData.SingleOrDefault(g => g.GeofenceUID == favoriteGeofenceInside.GeofenceUID);
    //  Assert.NotNull(actualFavorite);
    //  Assert.Equal(favoriteGeofenceInside.GeofenceUID, actualFavorite.GeofenceUID);
    //  Assert.Equal(favoriteGeofenceInside.GeofenceName, actualFavorite.GeofenceName);
    //  Assert.Equal(favoriteGeofenceInside.GeometryWKT, actualFavorite.GeometryWKT);
    //  Assert.Equal(favoriteGeofenceInside.GeofenceType, actualFavorite.GeofenceType);
    //}

    // ccss don't have a geofence service yet
    //[Fact]
    //public async Task GetBoundariesExecutor_WithAssociatedGeofences()
    //{
    //  string custUid = Guid.NewGuid().ToString();
    //  string userUid = Guid.NewGuid().ToString();
    //  string projectUid = Guid.NewGuid().ToString();
    //  string boundaryUid = Guid.NewGuid().ToString();
    //  string boundaryName = "blah";
    //  string boundaryGeometryWKT = "whatever";
    //  Guid associatedUid = Guid.NewGuid();
    //  string associatedName = "favorite blah";
    //  string associatedGeometryWKT = "who cares";

    //  var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
    //  var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

    //  var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
    //  var geofenceBoundary = new Geofence
    //  {
    //    CustomerUID = custUid,
    //    UserUID = userUid,
    //    GeofenceUID = boundaryUid,
    //    Name = boundaryName,
    //    GeometryWKT = boundaryGeometryWKT,
    //    GeofenceType = GeofenceType.Filter,
    //    LastActionedUTC = DateTime.UtcNow
    //  };
    //  geofenceRepo.As<IGeofenceRepository>().Setup(g => g.GetGeofences(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<Geofence> { geofenceBoundary });

    //  //var geofenceProxy = new Mock<IGeofenceProxy>();
    //  //geofenceProxy.Setup(g => g.GetFavoriteGeofences(custUid, userUid, null)).ReturnsAsync(new List<GeofenceData>());

    //  var associatedGeofence = new GeofenceData
    //  {
    //    GeofenceUID = associatedUid,
    //    GeofenceName = associatedName,
    //    GeometryWKT = associatedGeometryWKT,
    //    GeofenceType = GeofenceType.Generic.ToString(),
    //    CustomerUID = Guid.Parse(custUid)
    //  };
    //  //var upProxy = new Mock<IUnifiedProductivityProxy>();
    //  //upProxy.Setup(u => u.GetAssociatedGeofences(projectUid, null)).ReturnsAsync(new List<GeofenceData> { associatedGeofence });

    //  var projectRepo = new Mock<ProjectRepository>(configStore, logger);
    //  var projectGeofence = new ProjectGeofence { GeofenceUID = boundaryUid, ProjectUID = projectUid };
    //  projectRepo.As<IProjectRepository>().Setup(p => p.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(new List<ProjectGeofence> { projectGeofence });
    //  projectRepo.As<IProjectRepository>().Setup(p => p.DoPolygonsOverlap(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<bool> { true, false });

    //  var geofenceToTest = AutoMapperUtility.Automapper.Map<GeofenceData>(geofenceBoundary);

    //  var request = BaseRequestFull.Create
    //  (
    //    custUid,
    //    false,
    //    new ProjectData { ProjectUID = projectUid },
    //    userUid,
    //    null
    //  );

    //  var executor =
    //    RequestExecutorContainer.Build<GetBoundariesExecutor>(configStore, logger, serviceExceptionHandler,
    //      geofenceRepo.Object, projectRepo.Object
    //      /* , geofenceProxy: geofenceProxy.Object, unifiedProductivityProxy: upProxy.Object */ );
    //  var result = await executor.ProcessAsync(request) as GeofenceDataListResult;

    //  Assert.NotNull(result);
    //  Assert.Equal(2, result.GeofenceData.Count);
    //  var actualBoundary = result.GeofenceData.SingleOrDefault(g => g.GeofenceUID == geofenceToTest.GeofenceUID);
    //  Assert.Equal(geofenceToTest.GeofenceUID, actualBoundary.GeofenceUID);
    //  Assert.Equal(geofenceToTest.GeofenceName, actualBoundary.GeofenceName);
    //  Assert.Equal(geofenceToTest.GeometryWKT, actualBoundary.GeometryWKT);
    //  Assert.Equal(geofenceToTest.GeofenceType, actualBoundary.GeofenceType);
    //  var actualAssociated = result.GeofenceData.SingleOrDefault(g => g.GeofenceUID == associatedGeofence.GeofenceUID);
    //  Assert.NotNull(actualAssociated);
    //  Assert.Equal(associatedGeofence.GeofenceUID, actualAssociated.GeofenceUID);
    //  Assert.Equal(associatedGeofence.GeofenceName, actualAssociated.GeofenceName);
    //  Assert.Equal(associatedGeofence.GeometryWKT, actualAssociated.GeometryWKT);
    //  Assert.Equal(associatedGeofence.GeofenceType, actualAssociated.GeofenceType);
    //}

    [Fact]
    public async Task GetBoundariesExecutor_WithException()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string boundaryUid = Guid.NewGuid().ToString();
      string boundaryName = "blah";
      string boundaryGeometryWKT = "whatever";

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

      //var geofenceProxy = new Mock<IGeofenceProxy>();
      //geofenceProxy.Setup(g => g.GetFavoriteGeofences(custUid, userUid, null)).ReturnsAsync(new List<GeofenceData>());

      //var upProxy = new Mock<IUnifiedProductivityProxy>();
      //upProxy.Setup(u => u.GetAssociatedGeofences(projectUid, null)).Throws(new Exception("No UserCustomerAssociation exists."));

      var projectRepo = new Mock<ProjectRepository>(configStore, logger);
      var projectGeofence = new ProjectGeofence { GeofenceUID = boundaryUid, ProjectUID = projectUid };
      projectRepo.As<IProjectRepository>().Setup(p => p.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(new List<ProjectGeofence> { projectGeofence });
      projectRepo.As<IProjectRepository>().Setup(p => p.DoPolygonsOverlap(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<bool> { true, false });

      var geofenceToTest = AutoMapperUtility.Automapper.Map<GeofenceData>(geofenceBoundary);

      var request = BaseRequestFull.Create
      (
        custUid,
        false,
        new ProjectData { ProjectUID = projectUid },
        userUid,
        null
      );

      var executor =
        RequestExecutorContainer.Build<GetBoundariesExecutor>(configStore, logger, serviceExceptionHandler,
          geofenceRepo.Object, projectRepo.Object
          /* , geofenceProxy: geofenceProxy.Object, unifiedProductivityProxy: upProxy.Object */ );
      var result = await executor.ProcessAsync(request) as GeofenceDataListResult;

      Assert.NotNull(result);
      Assert.Single(result.GeofenceData);
      var actualBoundary = result.GeofenceData.SingleOrDefault(g => g.GeofenceUID == geofenceToTest.GeofenceUID);
      Assert.Equal(geofenceToTest.GeofenceUID, actualBoundary.GeofenceUID);
      Assert.Equal(geofenceToTest.GeofenceName, actualBoundary.GeofenceName);
      Assert.Equal(geofenceToTest.GeometryWKT, actualBoundary.GeometryWKT);
      Assert.Equal(geofenceToTest.GeofenceType, actualBoundary.GeofenceType);
    }

    // ccss don't have a geofence service yet
    //[Fact]
    //public async Task GetBoundariesExecutor_WithDuplicates()
    //{
    //  string custUid = Guid.NewGuid().ToString();
    //  string userUid = Guid.NewGuid().ToString();
    //  string projectUid = Guid.NewGuid().ToString();
    //  string boundaryUid = Guid.NewGuid().ToString();
    //  string boundaryName = "blah";
    //  string boundaryGeometryWKT = "whatever";
    //  Guid geofenceUid = Guid.NewGuid();
    //  string geofenceName = "favorite blah";
    //  string geofenceWKT = "who cares";

    //  var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
    //  var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

    //  var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
    //  var geofenceBoundary = new Geofence
    //  {
    //    CustomerUID = custUid,
    //    UserUID = userUid,
    //    GeofenceUID = boundaryUid,
    //    Name = boundaryName,
    //    GeometryWKT = boundaryGeometryWKT,
    //    GeofenceType = GeofenceType.Filter,
    //    LastActionedUTC = DateTime.UtcNow
    //  };
    //  geofenceRepo.As<IGeofenceRepository>().Setup(g => g.GetGeofences(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<Geofence> { geofenceBoundary });

    //  var geofence = new GeofenceData
    //  {
    //    GeofenceUID = geofenceUid,
    //    GeofenceName = geofenceName,
    //    GeometryWKT = geofenceWKT,
    //    GeofenceType = GeofenceType.Generic.ToString(),
    //    CustomerUID = Guid.Parse(custUid)
    //  };
    //  var geofences = new List<GeofenceData> { geofence };

    //  //var upProxy = new Mock<IUnifiedProductivityProxy>();
    //  //upProxy.Setup(u => u.GetAssociatedGeofences(projectUid, null)).ReturnsAsync(geofences);

    //  //var geofenceProxy = new Mock<IGeofenceProxy>();
    //  //geofenceProxy.Setup(g => g.GetFavoriteGeofences(custUid, userUid, null)).ReturnsAsync(geofences);

    //  var projectRepo = new Mock<ProjectRepository>(configStore, logger);
    //  var projectGeofence = new ProjectGeofence { GeofenceUID = boundaryUid, ProjectUID = projectUid };
    //  projectRepo.As<IProjectRepository>().Setup(p => p.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(new List<ProjectGeofence> { projectGeofence });
    //  projectRepo.As<IProjectRepository>().Setup(p => p.DoPolygonsOverlap(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<bool> { true, false });

    //  var geofenceToTest = AutoMapperUtility.Automapper.Map<GeofenceData>(geofenceBoundary);

    //  var request = BaseRequestFull.Create
    //  (
    //    custUid,
    //    false,
    //    new ProjectData { ProjectUID = projectUid },
    //    userUid,
    //    null
    //  );

    //  var executor =
    //    RequestExecutorContainer.Build<GetBoundariesExecutor>(configStore, logger, serviceExceptionHandler,
    //      geofenceRepo.Object, projectRepo.Object
    //      /* , geofenceProxy: geofenceProxy.Object, unifiedProductivityProxy: upProxy.Object */ );
    //  var result = await executor.ProcessAsync(request) as GeofenceDataListResult;

    //  Assert.NotNull(result);
    //  Assert.Equal(2, result.GeofenceData.Count);
    //  var actualBoundary = result.GeofenceData.SingleOrDefault(g => g.GeofenceUID == geofenceToTest.GeofenceUID);
    //  Assert.Equal(geofenceToTest.GeofenceUID, actualBoundary.GeofenceUID);
    //  Assert.Equal(geofenceToTest.GeofenceName, actualBoundary.GeofenceName);
    //  Assert.Equal(geofenceToTest.GeometryWKT, actualBoundary.GeometryWKT);
    //  Assert.Equal(geofenceToTest.GeofenceType, actualBoundary.GeofenceType);
    //  var actualGeofence = result.GeofenceData.SingleOrDefault(g => g.GeofenceUID == geofence.GeofenceUID);
    //  Assert.NotNull(actualGeofence);
    //  Assert.Equal(geofence.GeofenceUID, actualGeofence.GeofenceUID);
    //  Assert.Equal(geofence.GeofenceName, actualGeofence.GeofenceName);
    //  Assert.Equal(geofence.GeometryWKT, actualGeofence.GeometryWKT);
    //  Assert.Equal(geofence.GeofenceType, actualGeofence.GeofenceType);
    //}

    [Fact]
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

      var productivity3dV2ProxyNotification = new Mock<IProductivity3dV2ProxyNotification>();
      productivity3dV2ProxyNotification.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseMasterDataResult());

      var request = BoundaryRequestFull.Create
      (
        custUid,
        false,
        new ProjectData { ProjectUID = projectUid },
        userUid,
        new BoundaryRequest { BoundaryUid = null, Name = name, BoundaryPolygonWKT = geometryWKT }
      );

      var executor =
        RequestExecutorContainer.Build<UpsertBoundaryExecutor>(configStore, logger, serviceExceptionHandler,
          geofenceRepo.Object, projectRepo.Object);
      var result = await executor.ProcessAsync(request) as GeofenceDataSingleResult;

      Assert.NotNull(result);
      Assert.Equal(name, result.GeofenceData.GeofenceName);
      Assert.Equal(geometryWKT, result.GeofenceData.GeometryWKT);
    }

    [Fact]
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

      var productivity3dV2ProxyNotification = new Mock<IProductivity3dV2ProxyNotification>();
      productivity3dV2ProxyNotification.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseMasterDataResult());

      var request = BoundaryUidRequestFull.Create
      (
        custUid,
        false,
        new ProjectData { ProjectUID = projectUid },
        userUid,
        boundaryUid
      );

      var executor =
        RequestExecutorContainer.Build<DeleteBoundaryExecutor>(configStore, logger, serviceExceptionHandler,
          geofenceRepo.Object, projectRepo.Object,
          productivity3dV2ProxyNotification: productivity3dV2ProxyNotification.Object);
      var result = await executor.ProcessAsync(request);

      Assert.NotNull(result);
      Assert.Equal(0, result.Code);
      Assert.Equal("success", result.Message);
    }
  }
}
