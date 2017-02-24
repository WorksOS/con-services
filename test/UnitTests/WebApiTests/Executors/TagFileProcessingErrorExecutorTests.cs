using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System;
using VSS.Masterdata;
using VSS.TagFileAuth.Service.WebApiModels.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using VSS.TagFileAuth.Service.WebApiModels.Interfaces;
using VSS.TagFileAuth.Service.WebApiModels.Executors;
using VSS.TagFileAuth.Service.WebApiModels.Enums;

namespace VSS.TagFileAuth.Service.WebApiTests.Executors
{
  [TestClass]
  public class TagFileProcessingErrorExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public void CanCallTagFileProcessingErrorExecutorNoValidInput()
    {
      TagFileProcessingErrorRequest tagFileProcessingErrorRequest = new TagFileProcessingErrorRequest();
      TagFileProcessingErrorResult tagFileProcessingErrorResult = new TagFileProcessingErrorResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory).Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.result, "executor didn't process TagFileProcessingError");
    }

    [TestMethod]
    public void CanCallGetPTagFileProcessingErrorExecutorWithLegacyAssetId()
    {
      long legacyAssetID = 46534636436;
      string tagFileName = "Whatever";
      TagFileErrorsEnum error = TagFileErrorsEnum.None;
      var eventkeyDate = DateTime.UtcNow;
      TagFileProcessingErrorRequest TagFileProcessingErrorRequest = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(legacyAssetID, tagFileName, error);

      TagFileProcessingErrorResult TagFileProcessingErrorResult = new TagFileProcessingErrorResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory).Process(TagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.result, "executor didn't process TagFileProcessingError");
    }
  }
}
