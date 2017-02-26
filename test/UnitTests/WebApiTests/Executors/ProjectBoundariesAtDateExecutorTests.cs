using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System;
using VSS.Masterdata;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using VSS.TagFileAuth.Service.WebApiModels.Executors;

namespace VSS.TagFileAuth.Service.WebApiTests.Executors
{
  [TestClass]
  public class ProjectBoundariesAtDateExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public void CanCallProjectBoundariesAtDateExecutorNoValidInput()
    {
      GetProjectBoundariesAtDateRequest ProjectBoundariesAtDateRequest = new GetProjectBoundariesAtDateRequest();
      GetProjectBoundariesAtDateResult ProjectBoundariesAtDateResult = new GetProjectBoundariesAtDateResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory).Process(ProjectBoundariesAtDateRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect ProjectBoundaries");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect ProjectBoundaries");
    }

    [TestMethod]
    public void CanCallGetProjectBoundariesAtDateExecutorWithLegacyAssetId()
    {
      long legacyProjectID = 46534636436;
      var eventkeyDate = DateTime.UtcNow;
      GetProjectBoundariesAtDateRequest ProjectBoundariesAtDateRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyProjectID, eventkeyDate);

      GetProjectBoundariesAtDateResult ProjectBoundariesAtDateResult = new GetProjectBoundariesAtDateResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory).Process(ProjectBoundariesAtDateRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect ProjectBoundaries");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect ProjectBoundaries");
    }
  }
}
