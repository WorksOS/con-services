using ExecutorTests.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;

namespace ExecutorTests
{
  [TestClass]
  public class UpsertBoundaryExecutorTests : BoundaryRepositoryBase
  {
    private static string geometryWKT;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      geometryWKT = GenerateWKTPolygon();
    }

    [TestInitialize]
    public void ClassInit()
    {
      Setup();
    }

    [TestMethod]
    public async Task UpsertBoundaryExecutor_NoExistingBoundary()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string name = "name";

      var request = BoundaryRequestFull.Create(custUid, false, new ProjectData() { ProjectUid = projectUid }, userUid, new BoundaryRequest { Name = name, BoundaryPolygonWKT = geometryWKT });

      var executor = RequestExecutorContainer.Build<UpsertBoundaryExecutor>(ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var result = await executor.ProcessAsync(request) as GeofenceDataSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsNotNull(result.GeofenceData, "executor returned null geofence");
      Assert.AreEqual(name, result.GeofenceData.GeofenceName, "executor returned incorrect GeofenceName");
      Assert.AreEqual(geometryWKT, result.GeofenceData.GeometryWKT, "executor returned incorrect geometryWKT");
    }

    [TestMethod]
    public async Task UpsertBoundaryExecutor_BoundaryUidNotSupported()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string name = "name";

      var request = BoundaryRequestFull.Create(custUid, false, new ProjectData() { ProjectUid = projectUid }, userUid, new BoundaryRequest { BoundaryUid = Guid.NewGuid().ToString(), Name = name, BoundaryPolygonWKT = geometryWKT });

      var executor = RequestExecutorContainer.Build<UpsertBoundaryExecutor>(ConfigStore, Logger, ServiceExceptionHandler, GeofenceRepo, ProjectRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var serviceException = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));

      Assert.IsTrue(serviceException.GetContent.Contains("2061"));
      Assert.IsTrue(serviceException.GetContent.Contains("UpsertBoundary. Update not supported"));
    }
  }
}