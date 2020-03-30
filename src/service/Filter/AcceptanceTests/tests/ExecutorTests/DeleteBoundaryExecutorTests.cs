using ExecutorTests.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace ExecutorTests
{
  [TestClass]
  public class DeleteBoundaryExecutorTests : BoundaryRepositoryBase
  {
    [TestInitialize]
    public void ClassInit()
    {
      Setup();
    }

    [TestMethod]
    public async Task DeleteBoundaryExecutor_NoExistingBoundary()
    {
      var custUid = Guid.NewGuid().ToString();
      var userId = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var request = CreateAndValidateRequest(custUid, projectUid, userId, Guid.NewGuid().ToString());

      var executor =
        RequestExecutorContainer.Build<DeleteBoundaryExecutor>(ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo, ProjectProxy);

      var serviceException = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));
      Assert.IsTrue(serviceException.GetContent.Contains("2049"));
      Assert.IsTrue(serviceException.GetContent.Contains("GetBoundary By BoundaryUid. The requested Boundary does not exist, or does not belong to the requesting project or filter."));
    }

    [TestMethod]
    public async Task DeleteBoundaryExecutor_ExistingBoundary()
    {
      var custUid = Guid.NewGuid().ToString();
      var userId = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var boundaryUid = Guid.NewGuid().ToString();

      WriteEventToDb(new CreateGeofenceEvent
      {
        GeofenceUID = boundaryUid.ToString(),
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
        ProjectUID = projectUid,
        GeofenceUID = boundaryUid.ToString(),
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = CreateAndValidateRequest(custUid, projectUid, userId, boundaryUid);

      var executor =
        RequestExecutorContainer.Build<DeleteBoundaryExecutor>(ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo, ProjectProxy);
      var result = await executor.ProcessAsync(request);

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.AreEqual(0, result.Code, "executor returned incorrect code");
      Assert.AreEqual("success", result.Message, "executor returned incorrect message");
    }

    [TestMethod]
    public async Task DeleteBoundaryExecutor_BelongsToAnotherProject_NotAllowed()
    {
      var custUid = Guid.NewGuid().ToString();
      var userId = Guid.NewGuid().ToString();
      var projectUid1 = Guid.NewGuid().ToString();
      var projectUid2 = Guid.NewGuid().ToString();
      var boundaryUid = Guid.NewGuid().ToString();

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
        GeofenceUID = boundaryUid.ToString(),
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = CreateAndValidateRequest(custUid, projectUid2, userId, boundaryUid);

      var executor =
        RequestExecutorContainer.Build<DeleteBoundaryExecutor>(ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo, ProjectProxy);

      var serviceException = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));
      Assert.IsTrue(serviceException.GetContent.Contains("2049"));
      Assert.IsTrue(serviceException.GetContent.Contains("GetBoundary By BoundaryUid. The requested Boundary does not exist, or does not belong to the requesting project or filter."));
    }

    private BoundaryUidRequestFull CreateAndValidateRequest(string custUid, string projectUid, string userId, string boundaryUid)
    {
      var request = BoundaryUidRequestFull.Create(
        custUid.ToString(),
        false,
        new ProjectData() { ProjectUID = projectUid },
        userId.ToString(),
        boundaryUid);
      request.Validate(ServiceExceptionHandler);
      return request;
    }

  }
}
