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
  public class ProjectIdExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public void CanCallProjectIDExecutorNoValidInput()
    {
      GetProjectIdRequest ProjectIdRequest = new GetProjectIdRequest();
      GetProjectIdResult ProjectIdResult = new GetProjectIdResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<ProjectIdExecutor>(factory).Process(ProjectIdRequest) as GetProjectIdResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.projectId, "executor returned incorrect legacy ProjectId");
    }

    [TestMethod]
    public void CanCallGetProjectIdExecutorWithLegacyAssetId()
    {
      long legacyAssetID = 46534636436;
      double latitude = 80;
      double longitude = 170;
      double height = 0;
      var eventkeyDate = DateTime.UtcNow;
      GetProjectIdRequest ProjectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(legacyAssetID, latitude, longitude, height, eventkeyDate, "");

      GetProjectIdResult ProjectIdResult = new GetProjectIdResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<ProjectIdExecutor>(factory).Process(ProjectIdRequest) as GetProjectIdResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.projectId, "executor returned incorrect legacy ProjectId");
    }
  }
}
