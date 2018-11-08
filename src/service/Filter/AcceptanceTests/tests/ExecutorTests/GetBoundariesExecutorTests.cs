using ExecutorTests.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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

      var request = CreateAndValidateRequest(custUid, projectUid, userId);

      var executor = RequestExecutorContainer.Build<GetBoundariesExecutor>(ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo);
      var result = await executor.ProcessAsync(request) as GeofenceDataListResult;
      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      Assert.AreEqual(0, result.GeofenceData.Count, "Shouldn't be any boundaries returned");
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

      var request = CreateAndValidateRequest(custUid, projectUid, userId);

      var executor =
        RequestExecutorContainer.Build<GetBoundariesExecutor>(ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo);
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
      Assert.AreEqual(1, result.GeofenceData.Count);
      Assert.AreEqual(boundaryToTest.GeofenceData.GeofenceUID, result.GeofenceData[0].GeofenceUID);
      Assert.AreEqual(boundaryToTest.GeofenceData.GeofenceName, result.GeofenceData[0].GeofenceName);
      Assert.AreEqual(boundaryToTest.GeofenceData.UserUID, result.GeofenceData[0].UserUID);
      Assert.AreEqual(boundaryToTest.GeofenceData.GeometryWKT, result.GeofenceData[0].GeometryWKT);
      Assert.AreEqual(boundaryToTest.GeofenceData.GeofenceType, result.GeofenceData[0].GeofenceType);
      Assert.AreEqual(boundaryToTest.GeofenceData.CustomerUID, result.GeofenceData[0].CustomerUID);
      Assert.AreEqual(boundaryToTest.GeofenceData.FillColor, result.GeofenceData[0].FillColor);
      Assert.AreEqual(boundaryToTest.GeofenceData.IsTransparent, result.GeofenceData[0].IsTransparent);
      Assert.AreEqual(boundaryToTest.GeofenceData.Description, result.GeofenceData[0].Description);
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

      var request = CreateAndValidateRequest(custUid, projectUid, userId);

      var executor =
        RequestExecutorContainer.Build<GetBoundariesExecutor>(ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo);
      var result = await executor.ProcessAsync(request) as GeofenceDataListResult;

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      Assert.AreEqual(1, result.GeofenceData.Count);
    }

    private BaseRequestFull CreateAndValidateRequest(Guid custUid, Guid projectUid, Guid userId)
    {
      var request = BaseRequestFull.Create(
        custUid.ToString(),
        false,
        new ProjectData() { ProjectUid = projectUid.ToString() },
        userId.ToString());
      request.Validate(ServiceExceptionHandler);
      return request;
    }
  }
}