using ExecutorTests.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

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
      var custUid = Guid.NewGuid();
      var userId = Guid.NewGuid();
      var projectUid = Guid.NewGuid();
      var request = CreateAndValidateRequest(custUid, projectUid, userId, Guid.NewGuid());

      var executor =
        RequestExecutorContainer.Build<DeleteBoundaryExecutor>(ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);

      var serviceException = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));
      Assert.IsTrue(serviceException.GetContent.Contains("2049"));
      Assert.IsTrue(serviceException.GetContent.Contains("GetBoundary By BoundaryUid. The requested Boundary does not exist, or does not belong to the requesting project or filter."));
    }

    [TestMethod]
    public async Task DeleteBoundaryExecutor_ExistingBoundary()
    {
      var custUid = Guid.NewGuid();
      var userId = Guid.NewGuid();
      var projectUid = Guid.NewGuid();
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
        ProjectUID = projectUid,
        GeofenceUID = boundaryUid,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = CreateAndValidateRequest(custUid, projectUid, userId, boundaryUid);

      var executor =
        RequestExecutorContainer.Build<DeleteBoundaryExecutor>(ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var result = await executor.ProcessAsync(request);

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.AreEqual(0, result.Code, "executor returned incorrect code");
      Assert.AreEqual("success", result.Message, "executor returned incorrect message");
    }

    [TestMethod]
    public async Task DeleteBoundaryExecutor_BelongsToAnotherProject_NotAllowed()
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
        RequestExecutorContainer.Build<DeleteBoundaryExecutor>(ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);

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
