using ExecutorTests.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests
{
  [TestClass]
  public class GetBoundariesExecutorTests : BoundaryRepositoryBase
  {
    private static string boundaryPolygon;
    private static string goldenDimensionsPolygon = "POLYGON((-115.025723657623 36.2101347890754,-115.026281557098 36.2056332151707,-115.018041811005 36.205460072542,-115.017698488251 36.2102040420362,-115.025723657623 36.2101347890754))";

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      boundaryPolygon = GenerateWKTPolygon();
    }

    [TestInitialize]
    public void ClassInit()
    {
      Setup();
    }

    [TestMethod]
    public async Task Should_return_no_boundaries_when_none_exist()
    {
      var custUid = Guid.NewGuid();
      var userId = Guid.NewGuid();
      var projectUid = Guid.NewGuid();

      var request = CreateAndValidateRequest(custUid, projectUid, userId, goldenDimensionsPolygon);

      var executor = RequestExecutorContainer.Build<GetBoundariesExecutor>(
        ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo, geofenceProxy: GeofenceProxy, unifiedProductivityProxy: UnifiedProductivityProxy);
      var result = await executor.ProcessAsync(request) as GeofenceDataListResult;
      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      Assert.AreEqual(2, result.GeofenceData.Count, "Should be two overlapping favorite or associated geofences returned");
      var boundaries = result.GeofenceData.Where(b => b.GeofenceType == GeofenceType.Filter.ToString()).ToList();
      Assert.AreEqual(0, boundaries.Count, "Shouldn't be any boundaries returned");
    }

    [TestMethod]
    public async Task Should_return_all_boundaries_for_a_given_Project_when_Project_exists()
    {
      var custUid = Guid.NewGuid();
      var userId = Guid.NewGuid();
      var projectUid = Guid.NewGuid();
      var boundaryUid = Guid.NewGuid();
      var name = "name";

      WriteEventToDb(new CreateGeofenceEvent
      {
        GeofenceUID = boundaryUid,
        GeofenceType = GeofenceType.Filter.ToString(),
        CustomerUID = custUid,
        UserUID = userId,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow,
        GeometryWKT = boundaryPolygon,
        GeofenceName = name,
        Description = null,
      });
      WriteEventToDb(new AssociateProjectGeofence
      {
        ProjectUID = projectUid,
        GeofenceUID = boundaryUid,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = CreateAndValidateRequest(custUid, projectUid, userId, goldenDimensionsPolygon);

      var executor =
        RequestExecutorContainer.Build<GetBoundariesExecutor>(
          ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo, geofenceProxy: GeofenceProxy, unifiedProductivityProxy: UnifiedProductivityProxy);
      var result = await executor.ProcessAsync(request) as GeofenceDataListResult;

      var boundaryToTest = new GeofenceDataSingleResult(
        new GeofenceData
        {
          GeofenceUID = boundaryUid,
          GeofenceName = name,
          UserUID = userId,
          GeometryWKT = boundaryPolygon,
          GeofenceType = GeofenceType.Filter.ToString(),
          CustomerUID = custUid,
          FillColor = 0,
          IsTransparent = false,
          Description = null
        });

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      var boundaries = result.GeofenceData.Where(b => b.GeofenceType == GeofenceType.Filter.ToString()).ToList();
      Assert.AreEqual(1, boundaries.Count);
      Assert.AreEqual(boundaryToTest.GeofenceData.GeofenceUID, boundaries[0].GeofenceUID);
      Assert.AreEqual(boundaryToTest.GeofenceData.GeofenceName, boundaries[0].GeofenceName);
      Assert.AreEqual(boundaryToTest.GeofenceData.UserUID, boundaries[0].UserUID);
      Assert.AreEqual(boundaryToTest.GeofenceData.GeometryWKT, boundaries[0].GeometryWKT);
      Assert.AreEqual(boundaryToTest.GeofenceData.GeofenceType, boundaries[0].GeofenceType);
      Assert.AreEqual(boundaryToTest.GeofenceData.CustomerUID, boundaries[0].CustomerUID);
      Assert.AreEqual(boundaryToTest.GeofenceData.FillColor, boundaries[0].FillColor);
      Assert.AreEqual(boundaryToTest.GeofenceData.IsTransparent, boundaries[0].IsTransparent);
      Assert.AreEqual(boundaryToTest.GeofenceData.Description, boundaries[0].Description);
    }

    [TestMethod]
    public async Task Should_return_expected_Geofence_When_using_case_insensitive_keys()
    {
      var custUid = Guid.NewGuid();
      var userId = Guid.NewGuid();
      var projectUid = Guid.NewGuid();
      var boundaryUid = Guid.NewGuid();
      var name = "name";

      WriteEventToDb(new CreateGeofenceEvent
      {
        GeofenceUID = boundaryUid,
        GeofenceType = GeofenceType.Filter.ToString(),
        CustomerUID = custUid,
        UserUID = userId,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow,
        GeometryWKT = boundaryPolygon,
        GeofenceName = name,
        Description = null
      });
      WriteEventToDb(new AssociateProjectGeofence
      {
        ProjectUID = projectUid,
        GeofenceUID = boundaryUid,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = CreateAndValidateRequest(custUid, projectUid, userId, goldenDimensionsPolygon);

      var executor =
        RequestExecutorContainer.Build<GetBoundariesExecutor>(
          ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo, geofenceProxy: GeofenceProxy, unifiedProductivityProxy: UnifiedProductivityProxy);
      var result = await executor.ProcessAsync(request) as GeofenceDataListResult;

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      var boundaries = result.GeofenceData.Where(b => b.GeofenceType == GeofenceType.Filter.ToString()).ToList();
      Assert.AreEqual(1, boundaries.Count);
    }

    private BaseRequestFull CreateAndValidateRequest(Guid custUid, Guid projectUid, Guid userId, string projectGeometryWKT)
    {
      var request = BaseRequestFull.Create(
        custUid.ToString(),
        false,
        new ProjectData() { ProjectUid = projectUid.ToString(), ProjectGeofenceWKT = projectGeometryWKT},
        userId.ToString(), null);
      request.Validate(ServiceExceptionHandler);
      return request;
    }
  }
}
