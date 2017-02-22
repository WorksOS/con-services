using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.TagFileAuth.Service.WebApi.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using VSS.TagFileAuth.Service.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.ResultHandling;
using VSS.TagFileAuth.Service.Executors;
using System;
using VSS.Masterdata;

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
    public void CanCallGetPProjectIdExecutorWithLegacyAssetId()
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
