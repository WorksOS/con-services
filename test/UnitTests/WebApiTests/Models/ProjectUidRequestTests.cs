using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;

namespace WebApiTests.Models
{
  [TestClass]
  public class ProjectUidRequestTests : ModelBaseTests
  {
    private string projectUidPrefix = @"{{""ProjectUid"":"""",";

    [TestMethod]
    [DataRow(1, "rs45", 89, 179, 30)]
    [DataRow(6, "", 89, 179, 32)]
    [DataRow(6, "rs45", 91, 179, 21)]
    [DataRow(6, "rs45", 89, 181, 22)]
    public void ValidateGetProjectUidRequest_ValidationErrors(int deviceType, string radioSerial,
    double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      GetProjectUidRequest projectUidRequest = GetProjectUidRequest.CreateGetProjectUidRequest(deviceType, radioSerial, latitude, longitude, timeOfPosition);
      var ex = Assert.ThrowsException<ServiceException>(() => projectUidRequest.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(errorCode);
      var internalCode = contractExecutionStatesEnum.GetErrorNumberwithOffset(errorCode);
      var exceptionMessage = string.Format(projectUidPrefix + TrexExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

   [TestMethod]
   [DataRow(6, "rs45", 89, 179, 0)]
    public void ValidateGetProjectUidRequest_ValidationHappPath(int deviceType, string radioSerial,
      double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      GetProjectUidRequest projectUidRequest = GetProjectUidRequest.CreateGetProjectUidRequest(deviceType, radioSerial, latitude, longitude, timeOfPosition);
      projectUidRequest.Validate();
    }
  }
}
