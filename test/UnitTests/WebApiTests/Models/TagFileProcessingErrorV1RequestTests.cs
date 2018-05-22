using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace WebApiTests.Models
{
  [TestClass]
  public class TagFileProcessingErrorV1RequestTests :ModelBaseTests
  {
    private string tagFilePrefix = "{{";

    [TestMethod]
    public void TagFileProcessingErrorV1Request_InvalidAssetId()
    {
      var request = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(-1, "", 0);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(9);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV1Request_InvalidTagFileName1()
    {
      var request = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(34345, "", 0);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(5);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV1Request_InvalidErrorNumber1()
    {
      var request = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(34345, "aFilename", 0);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(4);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV1Request_InvalidErrorNumber2()
    {
      var request = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(34345, "aFilename", -3);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(4);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV1Request_InvalidErrorNumber3()
    {
      var request = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(34345, "aFilename", 10);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(4);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV1Request_InvalidTagFileName2()
    {
      var request = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(34345, null, 1);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(5);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }
  }
}
