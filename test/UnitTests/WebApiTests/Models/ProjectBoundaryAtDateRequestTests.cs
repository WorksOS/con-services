using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace WebApiTests.Models
{
  [TestClass]
  public class ProjectBoundaryAtDateRequestTests :ModelBaseTests
  {
    private string projectBoundaryPrefix = @"{{""projectBoundary"":{{""FencePoints"":null}},";

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase1()
    {
      GetProjectBoundaryAtDateRequest projectBoundaryAtDateRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(-1, DateTime.UtcNow);
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundaryAtDateRequest.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(18);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(projectBoundaryPrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase2()
    {
      GetProjectBoundaryAtDateRequest projectBoundaryAtDateRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(-1, DateTime.UtcNow.AddYears(-1));
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundaryAtDateRequest.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(18);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(projectBoundaryPrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase3()
    {
      DateTime now = DateTime.UtcNow.AddYears(-50).AddMonths(-1);
      GetProjectBoundaryAtDateRequest projectBoundaryAtDateRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(1233, now);
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundaryAtDateRequest.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(17);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(projectBoundaryPrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase4()
    {
      GetProjectBoundaryAtDateRequest projectBoundaryAtDateRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(23434, DateTime.UtcNow.AddYears(-1));
      projectBoundaryAtDateRequest.Validate();
    }

  }
}
