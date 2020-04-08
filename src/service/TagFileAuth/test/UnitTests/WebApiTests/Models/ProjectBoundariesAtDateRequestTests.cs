﻿using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;

namespace WebApiTests.Models
{
  [TestClass]
  public class ProjectBoundariesAtDateRequestTests :ModelBaseTests
  {
    private string projectBoundariesPrefix = @"{{""projectBoundaries"":[],";

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase1()
    {
      var projectBoundariesAtDateRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(-1, DateTime.UtcNow);
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundariesAtDateRequest.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(9);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(projectBoundariesPrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase2()
    {
      var projectBoundariesAtDateRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(-1, DateTime.UtcNow.AddYears(-1));
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundariesAtDateRequest.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(9);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(projectBoundariesPrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase3()
    {
      var now = DateTime.UtcNow.AddYears(-50).AddMonths(-1);
      var projectBoundariesAtDateRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(1233, now);
      var ex = Assert.ThrowsException<ServiceException>(() => projectBoundariesAtDateRequest.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(23);
      var internalCode = (int)ContractExecutionStatesEnum.ValidationError;
      var exceptionMessage = string.Format(projectBoundariesPrefix + RaptorExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    public void ValidateGetAssetIdRequest_ValidatorCase4()
    {
      var projectBoundariesAtDateRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(23434, DateTime.UtcNow.AddYears(-1));
      projectBoundariesAtDateRequest.Validate();
    }

  }
}
