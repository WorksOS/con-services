using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.TagFileAuth.Service.WebApi.Interfaces;
using VSS.TagFileAuth.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using VSS.TagFileAuth.Service.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.ResultHandling;
using VSS.TagFileAuth.Service.Executors;
using System;

namespace VSS.TagFileAuth.Service.WebApiTests.Executors
{
  [TestClass]
  public class ProjectBoundaryAtDateExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public void CanCallProjectBoundaryAtDateExecutorNoValidInput()
    {
      GetProjectBoundaryAtDateRequest ProjectBoundaryAtDateRequest = new GetProjectBoundaryAtDateRequest();
      GetProjectBoundaryAtDateResult ProjectBoundaryAtDateResult = new GetProjectBoundaryAtDateResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<ProjectBoundaryAtDateExecutor>(factory).Process(ProjectBoundaryAtDateRequest) as GetProjectBoundaryAtDateResult;
      Assert.IsNotNull(result, "executor returned nothing");
     // todo  Assert.IsNotNull(result.projectBoundary, "executor returned incorrect projectBoundary");
    }

    [TestMethod]
    public void CanCallGetPProjectBoundaryAtDateExecutorWithLegacyAssetId()
    {
      long legacyProjectID = 46534636436;      
      var eventkeyDate = DateTime.UtcNow;
      GetProjectBoundaryAtDateRequest ProjectBoundaryAtDateRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(legacyProjectID, eventkeyDate);

      GetProjectBoundaryAtDateResult ProjectBoundaryAtDateResult = new GetProjectBoundaryAtDateResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<ProjectBoundaryAtDateExecutor>(factory).Process(ProjectBoundaryAtDateRequest) as GetProjectBoundaryAtDateResult;
      Assert.IsNotNull(result, "executor returned nothing");
      // todo Assert.IsNotNull(result.projectBoundary, "executor returned incorrect projectBoundary");
    }

    
  }
}
