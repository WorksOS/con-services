using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace WebApiTests.Models
{
  [TestClass]
  public class TagFileProcessingErrorV2RequestTests : ModelBaseTests
  {
    private string tagFilePrefix = "{{";

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidTccOrgId()
    {
      string tccOrgId = "blah";
      long? assetId = null;
      long? projectId = null;
      int error = 0;
      string tagFileName = "";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
        (tccOrgId, assetId, projectId,
         error, tagFileName,
         deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(1);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + exceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidAssetId()
    {
      string tccOrgId = null;
      long? assetId = -2;
      long? projectId = null;
      int error = 0;
      string tagFileName = "";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, assetId, projectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(2);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + exceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidProjectId()
    {
      string tccOrgId = null;
      long? assetId = null;
      long? projectId = -4;
      int error = 0;
      string tagFileName = "";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, assetId, projectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(3);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + exceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidErrorNumber1()
    {
      string tccOrgId = null;
      long? assetId = null;
      long? projectId = null;
      int error = 0;
      string tagFileName = "";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, assetId, projectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(4);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + exceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidErrorNumber2()
    {
      string tccOrgId = null;
      long? assetId = null;
      long? projectId = null;
      int error = -3;
      string tagFileName = "";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, assetId, projectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(4);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + exceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidErrorNumber3()
    {
      string tccOrgId = null;
      long? assetId = null;
      long? projectId = null;
      int error = 10;
      string tagFileName = "";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, assetId, projectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(4);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + exceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidTagFileName1()
    {
      string tccOrgId = null;
      long? assetId = null;
      long? projectId = null;
      int error = 1;
      string tagFileName = null;
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, assetId, projectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(5);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + exceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidTagFileName2()
    {
      string tccOrgId = null;
      long? assetId = null;
      long? projectId = null;
      int error = 1;
      string tagFileName = "";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, assetId, projectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(5);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + exceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_NotEnoughForCustomerIdentification()
    {
      string tccOrgId = null;
      long? assetId = null;
      long? projectId = null;
      int error = 1;
      string tagFileName = "theDevice Serial455-- theMachine 444Name -- 160819203837.tag";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, assetId, projectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(10);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + exceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_ValidDeviceSerialNumber()
    {
      string tccOrgId = null;
      long? assetId = null;
      long? projectId = 56;
      int error = 1;
      string tagFileName = "theDevice Serial455-- theMachine 444Name -- 160819203837.tag";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, assetId, projectId,
        error, tagFileName,
        deviceSerialNumber);
      request.Validate();
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidDisplaySerialNumber()
    {
      string tccOrgId = null;
      long? assetId = null;
      long? projectId = null;
      int error = 1;
      string tagFileName = "  -- theMachine 444Name -- 160819203837.tag";
      string deviceSerialNumber = "b;aj";

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, assetId, projectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(6);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + exceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidMachineName()
    {
      string tccOrgId = null;
      long? assetId = null;
      long? projectId = 45;
      int error = 1;
      string tagFileName = "theDevice Serial455--  -- 160819203837.tag";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, assetId, projectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(7);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + exceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidTagFileCreateUtc()
    {
      string tccOrgId = null;
      long? assetId = null;
      long? projectId = 34;
      int error = 1;
      string tagFileName = "theDevice Serial455-- theMachine 444Name -- 169919203837.tag";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, assetId, projectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(8);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + exceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

  }
}
