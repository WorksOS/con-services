using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.TagFileAuth.Service.WebApi.Interfaces;
using VSS.TagFileAuth.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using VSS.TagFileAuth.Service.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.ResultHandling;
using VSS.TagFileAuth.Service.Executors;
using System;
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
    public void CanCallGetPProjectBoundaryAtDateExecutorWithLegacyAssetId()
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
