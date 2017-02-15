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
  public class TagFileProcessingErrorExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public void CanCallTagFileProcessingErrorExecutorNoValidInput()
    {
      TagFileProcessingErrorRequest TagFileProcessingErrorRequest = new TagFileProcessingErrorRequest();
      TagFileProcessingErrorResult TagFileProcessingErrorResult = new TagFileProcessingErrorResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory).Process(TagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.result, "executor returned incorrect legacy TagFileProcessingError");
    }

    [TestMethod]
    public void CanCallGetPTagFileProcessingErrorExecutorWithLegacyAssetId()
    {
      long legacyAssetID = 46534636436;
      string tagFileName = "Whatever";
      TagFileErrorsEnum error = null; 
      var eventkeyDate = DateTime.UtcNow;
      TagFileProcessingErrorRequest TagFileProcessingErrorRequest = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(legacyAssetID, tagFileName, error);

      TagFileProcessingErrorResult TagFileProcessingErrorResult = new TagFileProcessingErrorResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory).Process(TagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.result, "executor returned incorrect legacy TagFileProcessingError");
    }

    
  }
}
