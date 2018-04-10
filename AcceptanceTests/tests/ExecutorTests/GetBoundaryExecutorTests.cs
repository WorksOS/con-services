using ExecutorTests.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests
{
  [TestClass]
  public class GetBoundaryExecutorTests : BoundaryRepositoryBase
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
    public async Task Should_throw_When_object_doesnt_exist_in_database()
    {
      var custUid = Guid.NewGuid();
      var userId = Guid.NewGuid();
      var projectUid = Guid.NewGuid();
      var request = CreateAndValidateRequest(custUid, projectUid, userId, Guid.NewGuid());

      var executor = RequestExecutorContainer.Build<GetBoundaryExecutor>(ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo);
      var serviceException = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));

      Assert.IsTrue(serviceException.GetContent.Contains("2049"));
      Assert.IsTrue(serviceException.GetContent.Contains("GetBoundary By BoundaryUid. The requested Boundary does not exist, or does not belong to the requesting project or filter."));
    }

    [TestMethod]
    public async Task Should_return_expected_boundary_When_object_exists_in_database()
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

      var request = CreateAndValidateRequest(custUid, projectUid, userId, boundaryUid);

      var executor =
        RequestExecutorContainer.Build<GetBoundaryExecutor>(ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo);
      var result = await executor.ProcessAsync(request) as GeofenceDataSingleResult;

      var filterToTest = new GeofenceDataSingleResult(
        new GeofenceData
        {
          GeofenceUID = boundaryUid,
          Description = null,
          GeofenceName = name,
          UserUID = userId,
          GeometryWKT = boundaryPolygon,
          GeofenceType = GeofenceType.Filter.ToString(),
          CustomerUID = custUid,
          FillColor = 0,
          IsTransparent = false
        });

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      Assert.AreEqual(filterToTest.GeofenceData.GeofenceUID, result.GeofenceData.GeofenceUID);
      Assert.AreEqual(filterToTest.GeofenceData.Description, result.GeofenceData.Description);
      Assert.AreEqual(filterToTest.GeofenceData.GeofenceName, result.GeofenceData.GeofenceName);
      Assert.AreEqual(filterToTest.GeofenceData.UserUID, result.GeofenceData.UserUID);
      Assert.AreEqual(filterToTest.GeofenceData.GeometryWKT, result.GeofenceData.GeometryWKT);
      Assert.AreEqual(filterToTest.GeofenceData.GeofenceType, result.GeofenceData.GeofenceType);
      Assert.AreEqual(filterToTest.GeofenceData.CustomerUID, result.GeofenceData.CustomerUID);
      Assert.AreEqual(filterToTest.GeofenceData.FillColor, result.GeofenceData.FillColor);
      Assert.AreEqual(filterToTest.GeofenceData.IsTransparent, result.GeofenceData.IsTransparent);
    }

    [TestMethod]
    public async Task Should_return_expected_boundary_When_using_case_insensitive_keys()
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

      var request = CreateAndValidateRequest(custUid, projectUid, userId, boundaryUid);

      var executor =
        RequestExecutorContainer.Build<GetBoundaryExecutor>(ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo);
      var result = await executor.ProcessAsync(request) as GeofenceDataSingleResult;

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      Assert.AreEqual(boundaryUid, result.GeofenceData.GeofenceUID, Responses.IncorrectFilterDescriptorFilterUid);
      Assert.AreEqual(name, result.GeofenceData.GeofenceName, Responses.IncorrectFilterDescriptorName);
      Assert.AreEqual(boundaryPolygon, result.GeofenceData.GeometryWKT, Responses.IncorrectFilterDescriptorFilterJson);
    }

    [TestMethod]
    public async Task GetBoundaryExecutor_BelongsToAnotherProject_NotAllowed()
    {
      var custUid = Guid.NewGuid();
      var userId = Guid.NewGuid();
      var projectUid1 = Guid.NewGuid();
      var projectUid2 = Guid.NewGuid();
      var boundaryUid = Guid.NewGuid();

      WriteEventToDb(new CreateGeofenceEvent
      {
        GeofenceUID = boundaryUid,
        GeofenceType = GeofenceType.Filter.ToString(),
        CustomerUID = custUid,
        UserUID = userId,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow,
        GeometryWKT = GenerateWKTPolygon(),
        GeofenceName = "name",
        Description = null
      });
      WriteEventToDb(new AssociateProjectGeofence
      {
        ProjectUID = projectUid1,
        GeofenceUID = boundaryUid,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = CreateAndValidateRequest(custUid, projectUid2, userId, boundaryUid);

      var executor =
        RequestExecutorContainer.Build<GetBoundaryExecutor>(ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo);

      var serviceException = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));
      Assert.IsTrue(serviceException.GetContent.Contains("2049"));
      Assert.IsTrue(serviceException.GetContent.Contains("GetBoundary By BoundaryUid. The requested Boundary does not exist, or does not belong to the requesting project or filter."));
    }

    private BoundaryUidRequestFull CreateAndValidateRequest(Guid custUid, Guid projectUid, Guid userId, Guid boundaryUid)
    {
      var request = BoundaryUidRequestFull.Create(
        custUid.ToString(),
        false,
        new ProjectData() { ProjectUid = projectUid.ToString() },
        userId.ToString(),
        boundaryUid.ToString());
      request.Validate(ServiceExceptionHandler);
      return request;
    }
  }
}