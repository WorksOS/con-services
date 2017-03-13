using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
using VSS.TagFileAuth.Service.WebApiModels.Executors;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using TestUtility.Model.Enums;

namespace RepositoryTests
{
  [TestClass]
  public class ProjectBoundaryAtDateExecutorTests : ExecutorTestData
  {

    [TestMethod]
    public void ProjectBoundaryAtDateExecutor_NonExistingProject()
    {
      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid customerUID = Guid.NewGuid();

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);

      GetProjectBoundaryAtDateRequest projectBoundaryAtDateExecutorRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(legacyProjectId, timeOfPositionUtc);
      projectBoundaryAtDateExecutorRequest.Validate();

      var result = RequestExecutorContainer.Build<ProjectBoundaryAtDateExecutor>(factory, logger).Process(projectBoundaryAtDateExecutorRequest) as GetProjectBoundaryAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundary, "executor returned incorrect projectBoundary result");
      Assert.IsNull(result.projectBoundary.FencePoints, "executor returned incorrect points result");
    }

    [TestMethod]
    public void ProjectBoundaryAtDateExecutor_ExistingProject()
    {
      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid customerUID = Guid.NewGuid();
      var isCreatedOk = CreateProject(projectUID, legacyProjectId, customerUID);
      Assert.IsTrue(isCreatedOk, "created project");

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);

      GetProjectBoundaryAtDateRequest projectBoundaryAtDateExecutorRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(legacyProjectId, timeOfPositionUtc);
      projectBoundaryAtDateExecutorRequest.Validate();

      var result = RequestExecutorContainer.Build<ProjectBoundaryAtDateExecutor>(factory, logger).Process(projectBoundaryAtDateExecutorRequest) as GetProjectBoundaryAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundary, "executor returned incorrect projectBoundary result");
      Assert.IsNotNull(result.projectBoundary.FencePoints, "executor returned incorrect points result");
      Assert.AreEqual(5, result.projectBoundary.FencePoints.Length, "executor returned incorrect point count");
    }
  }
}

