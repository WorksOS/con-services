using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace ExecutorTests
{
  [TestClass]
  public class ProjectBoundaryAtDateExecutorTests : ExecutorTestData
  {

    [TestMethod]
    public async Task ProjectBoundaryAtDateExecutor_NonExistingProject()
    {
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);

      GetProjectBoundaryAtDateRequest projectBoundaryAtDateExecutorRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(legacyProjectId, timeOfPositionUtc);
      projectBoundaryAtDateExecutorRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectBoundaryAtDateExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo, 
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectBoundaryAtDateExecutorRequest) as GetProjectBoundaryAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundary, "executor returned incorrect projectBoundary result");
      Assert.IsNull(result.projectBoundary.FencePoints, "executor returned incorrect points result");
    }

    [TestMethod]
    public async Task ProjectBoundaryAtDateExecutor_ExistingProject()
    {
      Guid projectUid = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid customerUid = Guid.NewGuid();
      CreateCustomer(customerUid, "");
      CreateProject(projectUid, legacyProjectId, customerUid);

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);

      GetProjectBoundaryAtDateRequest projectBoundaryAtDateExecutorRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(legacyProjectId, timeOfPositionUtc);
      projectBoundaryAtDateExecutorRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectBoundaryAtDateExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo, 
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectBoundaryAtDateExecutorRequest) as GetProjectBoundaryAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundary, "executor returned incorrect projectBoundary result");
      Assert.IsNotNull(result.projectBoundary.FencePoints, "executor returned incorrect points result");
      Assert.AreEqual(5, result.projectBoundary.FencePoints.Length, "executor returned incorrect point count");
    }

    /// <summary>
    ///  Can't do this test automatically as can't write the project using the repo
    ///    tested this manually by removing point inside ParseBoundaryData
    /// </summary>
    [TestMethod]
    [Ignore]
    public async Task ProjectBoundaryAtDateExecutor_ExistingProjectInvalidBoundary()
    {
      Guid projectUid = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid customerUid = Guid.NewGuid();
      string invalidBoundary = "POLYGON((170 10, 190 10, 170 10))"; // missing matching endpoint
      var isCreatedOk = CreateProject(projectUid, legacyProjectId, customerUid, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.Standard, invalidBoundary);
      Assert.IsTrue(isCreatedOk, "created project");

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);

      GetProjectBoundaryAtDateRequest projectBoundaryAtDateExecutorRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(legacyProjectId, timeOfPositionUtc);
      projectBoundaryAtDateExecutorRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectBoundaryAtDateExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo, 
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectBoundaryAtDateExecutorRequest) as GetProjectBoundaryAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundary, "executor returned incorrect projectBoundary result");
      Assert.IsNotNull(result.projectBoundary.FencePoints, "executor returned incorrect points result");
      Assert.AreEqual(0, result.projectBoundary.FencePoints.Length, "executor returned incorrect point count");
    }
  }
}

