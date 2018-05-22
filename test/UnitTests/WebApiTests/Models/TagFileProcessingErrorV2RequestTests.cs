using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.Notifications.Enums;

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
      long? legacyAssetId = null;
      int? legacyProjectId = null;
      int error = 0;
      string tagFileName = "";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
        (tccOrgId, legacyAssetId, legacyProjectId,
         error, tagFileName,
         deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(1);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidAssetId()
    {
      string tccOrgId = null;
      long? legacyAssetId = -2;
      int? legacyProjectId = null;
      int error = 0;
      string tagFileName = "";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(2);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidProjectId()
    {
      string tccOrgId = null;
      long? legacyAssetId = null;
      int? legacyProjectId = -4;
      int error = 0;
      string tagFileName = "";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(3);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidErrorNumber1()
    {
      string tccOrgId = null;
      long? legacyAssetId = null;
      int? legacyProjectId = null;
      int error = 0;
      string tagFileName = "";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(4);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_MapTagFileError_minus2()
    {
      TagFileErrorsEnum raptorErrorNumber = (TagFileErrorsEnum) (-2);
      
      var tagFileErrorMappings = new TagFileErrorMappings();
      int notificationErrorNumber =  tagFileErrorMappings.tagFileErrorTypes.Find(st => string.Equals(st.name, raptorErrorNumber.ToString(), StringComparison.OrdinalIgnoreCase)).NotificationEnum;
      TagFileError notificationError = (TagFileError)tagFileErrorMappings.tagFileErrorTypes.Find(st => string.Equals(st.name, raptorErrorNumber.ToString(), StringComparison.OrdinalIgnoreCase)).NotificationEnum;

      Assert.AreEqual(8, notificationErrorNumber, "Invalid Raptor-Notification error code mapping");
      Assert.AreEqual(TagFileError.UnknownProject, notificationError, "Invalid Raptor-Notification error string mapping");
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_MapTagFileError2()
    {
      TagFileErrorsEnum raptorErrorNumber = (TagFileErrorsEnum)(2);

      var tagFileErrorMappings = new TagFileErrorMappings();
      int notificationErrorNumber = tagFileErrorMappings.tagFileErrorTypes.Find(st => string.Equals(st.name, raptorErrorNumber.ToString(), StringComparison.OrdinalIgnoreCase)).NotificationEnum;
      TagFileError notificationError = (TagFileError)tagFileErrorMappings.tagFileErrorTypes.Find(st => string.Equals(st.name, raptorErrorNumber.ToString(), StringComparison.OrdinalIgnoreCase)).NotificationEnum;

      Assert.AreEqual(2, notificationErrorNumber, "Invalid Raptor-Notification error code mapping");
      Assert.AreEqual(TagFileError.NoMatchingProjectArea, notificationError, "Invalid Raptor-Notification error string mapping");
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidErrorNumber2()
    {
      string tccOrgId = null;
      long? legacyAssetId = null;
      int? legacyProjectId = null;
      int error = -3;
      string tagFileName = "";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(4);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidErrorNumber3()
    {
      string tccOrgId = null;
      long? legacyAssetId = null;
      int? legacyProjectId = null;
      int error = 10;
      string tagFileName = "";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(4);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidTagFileName1()
    {
      string tccOrgId = null;
      long? legacyAssetId = null;
      int? legacyProjectId = null;
      int error = 1;
      string tagFileName = null;
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(5);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidTagFileName2()
    {
      string tccOrgId = null;
      long? legacyAssetId = null;
      int? legacyProjectId = null;
      int error = 1;
      string tagFileName = "";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(5);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_NotEnoughForCustomerIdentification()
    {
      // Even if no customerUid can be determined e.g. from tccOrgId/AssetUid/ProjectUid
      //      still write a notification.
      string tccOrgId = null;
      long? legacyAssetId = null;
      int? legacyProjectId = null;
      int error = 1;
      string tagFileName = "theDevice Serial455-- theMachine 444Name -- 160819203837.tag";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber);
      request.Validate();
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_ValidDeviceSerialNumber()
    {
      string tccOrgId = null;
      long? legacyAssetId = null;
      int? legacyProjectId = 56;
      int error = 1;
      string tagFileName = "theDevice Serial455-- theMachine 444Name -- 160819203837.tag";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber);
      request.Validate();
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidDisplaySerialNumber()
    {
      string tccOrgId = null;
      long? legacyAssetId = null;
      int? legacyProjectId = null;
      int error = 1;
      string tagFileName = "  -- theMachine 444Name -- 160819203837.tag";
      string deviceSerialNumber = "b;aj";

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber, 6);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(6);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidDeviceType()
    {
      string tccOrgId = null;
      long? legacyAssetId = null;
      int? legacyProjectId = null;
      int error = 1;
      string tagFileName = "  -- theMachine 444Name -- 160819203837.tag";
      string deviceSerialNumber = "b;aj";

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber, 500);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(30);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidMachineName()
    {
      string tccOrgId = null;
      long? legacyAssetId = null;
      int? legacyProjectId = 45;
      int error = 1;
      string tagFileName = "theDevice Serial455--  -- 160819203837.tag";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber, 0);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(7);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void TagFileProcessingErrorV2Request_Validation_InvalidTagFileCreateUtc()
    {
      string tccOrgId = null;
      long? legacyAssetId = null;
      int? legacyProjectId = 34;
      int error = 1;
      string tagFileName = "theDevice Serial455-- theMachine 444Name -- 169919203837.tag";
      string deviceSerialNumber = null;

      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber, 6);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(8);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(tagFilePrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

  }
}
