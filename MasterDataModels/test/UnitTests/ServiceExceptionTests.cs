﻿using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;

namespace VSS.MasterData.Models.Tests
{
  [TestClass]
  public class ServiceExceptionTests
  {
    [TestMethod]
    public void CanCreateServiceException()
    {
      var exception = new ServiceException(HttpStatusCode.Accepted, new ContractExecutionResult());
      Assert.IsNotNull(exception);
      Assert.AreEqual(HttpStatusCode.Accepted,exception.Code);
    }

    [TestMethod]
    public void CanOverrideStatusCode()
    {
      var exception = new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError));
      exception.OverrideBadRequest(HttpStatusCode.NoContent);
      Assert.AreEqual(HttpStatusCode.NoContent, exception.Code);
    }

    [TestMethod]
    public void CanNotOverrideStatusCode()
    {
      var exception = new ServiceException(HttpStatusCode.Ambiguous,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError));
      exception.OverrideBadRequest(HttpStatusCode.NoContent);
      Assert.AreNotEqual(HttpStatusCode.NoContent, exception.Code);
    }

  }
}