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
     // todo  Assert.IsNotNull(result.ProjectBoundaries, "executor returned incorrect ProjectBoundaries");
    }

    [TestMethod]
    public void CanCallGetPProjectBoundariesAtDateExecutorWithLegacyAssetId()
    {
      long legacyProjectID = 46534636436;      
      var eventkeyDate = DateTime.UtcNow;
      GetProjectBoundariesAtDateRequest ProjectBoundariesAtDateRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyProjectID, eventkeyDate);

      GetProjectBoundariesAtDateResult ProjectBoundariesAtDateResult = new GetProjectBoundariesAtDateResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory).Process(ProjectBoundariesAtDateRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor returned nothing");
      // todo Assert.IsNotNull(result.ProjectBoundaries, "executor returned incorrect ProjectBoundaries");
    }

    
  }
}
