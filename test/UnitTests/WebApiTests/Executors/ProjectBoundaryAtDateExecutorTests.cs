using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System;
using VSS.Masterdata;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.Executors;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.WebApiTests.Executors
{
  [TestClass]
  public class ProjectBoundaryAtDateExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public void CanCallProjectBoundaryAtDateExecutorNoValidInput()
    {
      GetProjectBoundaryAtDateRequest ProjectBoundaryAtDateRequest = new GetProjectBoundaryAtDateRequest();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<ProjectBoundaryAtDateExecutor>(factory).Process(ProjectBoundaryAtDateRequest) as GetProjectBoundaryAtDateResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code, "executor "); // todo does executed successfully mean it executed but may return nothing?
      // todo then should Boundary be null or empty list?
      // todo these returned classes include encode/decode. What is cam# equivalant?
      Assert.IsNotNull(result.projectBoundary, "executor returned incorrect projectBoundary");
      Assert.IsNull(result.projectBoundary.FencePoints, "executor returned incorrect projectBoundary count");
    }

    [TestMethod]
    public void CanCallGetProjectBoundaryAtDateExecutorWithLegacyAssetId()
    {
      long legacyProjectID = 46534636436;
      var eventkeyDate = DateTime.UtcNow;
      GetProjectBoundaryAtDateRequest ProjectBoundaryAtDateRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(legacyProjectID, eventkeyDate);           
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<ProjectBoundaryAtDateExecutor>(factory).Process(ProjectBoundaryAtDateRequest) as GetProjectBoundaryAtDateResult;
      Assert.IsNotNull(result.projectBoundary, "executor returned incorrect projectBoundary");
      Assert.IsNull(result.projectBoundary.FencePoints, "executor returned incorrect projectBoundary count");
    }
  }
}
