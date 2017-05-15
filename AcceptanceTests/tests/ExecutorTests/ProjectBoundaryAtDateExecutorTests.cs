using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiModels.Models;
using WebApiModels.Executors;
using WebApiModels.ResultHandling;

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
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundary, "executor returned incorrect projectBoundary result");
      Assert.IsNull(result.projectBoundary.FencePoints, "executor returned incorrect points result");
    }

    [TestMethod]
    public void ProjectBoundaryAtDateExecutor_ExistingProject()
    {
      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid customerUID = Guid.NewGuid();
      CreateCustomer(customerUID, "");
      CreateProject(projectUID, legacyProjectId, customerUID);

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);

      GetProjectBoundaryAtDateRequest projectBoundaryAtDateExecutorRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(legacyProjectId, timeOfPositionUtc);
      projectBoundaryAtDateExecutorRequest.Validate();

      var result = RequestExecutorContainer.Build<ProjectBoundaryAtDateExecutor>(factory, logger).Process(projectBoundaryAtDateExecutorRequest) as GetProjectBoundaryAtDateResult;
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
    public void ProjectBoundaryAtDateExecutor_ExistingProjectInvalidBoundary()
    {
      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid customerUID = Guid.NewGuid();
      string invalidBoundary = "POLYGON((170 10, 190 10, 170 10))"; // missing matching endpoint
      var isCreatedOk = CreateProject(projectUID, legacyProjectId, customerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.Standard, invalidBoundary);
      Assert.IsTrue(isCreatedOk, "created project");

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);

      GetProjectBoundaryAtDateRequest projectBoundaryAtDateExecutorRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(legacyProjectId, timeOfPositionUtc);
      projectBoundaryAtDateExecutorRequest.Validate();

      var result = RequestExecutorContainer.Build<ProjectBoundaryAtDateExecutor>(factory, logger).Process(projectBoundaryAtDateExecutorRequest) as GetProjectBoundaryAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundary, "executor returned incorrect projectBoundary result");
      Assert.IsNotNull(result.projectBoundary.FencePoints, "executor returned incorrect points result");
      Assert.AreEqual(0, result.projectBoundary.FencePoints.Length, "executor returned incorrect point count");
    }
  }
}

