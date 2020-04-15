using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;

namespace WebApiTests.Models
{
  [TestClass]
  public class ProjectIdRequestTests : ModelBaseTests
  {
    private string projectIdPrefix = @"{{""projectId"":-1,";

    [TestMethod]
    public void ValidateGetProjectIdRequest_ValidatorCase1()
    {
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(-1, 91, 181, DateTime.MinValue);
      var ex = Assert.ThrowsException<ServiceException>(() => projectIdRequest.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(20);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(projectIdPrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void ValidateGetProjectIdRequest_ValidatorCase2()
    {
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(-1, 89, 179, DateTime.MinValue);
      var ex = Assert.ThrowsException<ServiceException>(() => projectIdRequest.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(20);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(projectIdPrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void ValidateGetProjectIdRequest_ValidatorCase3()
    {
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(345345, -91, 179, DateTime.MinValue);
      var ex = Assert.ThrowsException<ServiceException>(() => projectIdRequest.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(21);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(projectIdPrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void ValidateGetProjectIdRequest_ValidatorCase4()
    {
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(345345, -89, 179, DateTime.UtcNow.AddYears(-50).AddMonths(-1));
      var ex = Assert.ThrowsException<ServiceException>(() => projectIdRequest.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(23);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(projectIdPrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }
    
    [TestMethod]
    public void ValidateProjectIdRequest_ValidatorCase5()
    {
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(345345, -89, 179, DateTime.UtcNow.AddMonths(-1));
      projectIdRequest.Validate();
    }
  }
}
