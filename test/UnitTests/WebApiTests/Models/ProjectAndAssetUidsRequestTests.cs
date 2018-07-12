using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;

namespace WebApiTests.Models
{
  [TestClass]
  public class ProjectAndAssetUidsRequestTests : ModelBaseTests
  {
    private string projectUidPrefix = @"{{""ProjectUid"":"""",""AssetUid"":"""",";

    [TestMethod]
    [DataRow("", 999, "rs45", "", 89, 179, 30)] // invalid deviceType
    [DataRow("", 6, "rs45", "", 91, 179, 21)] // invalid lat
    [DataRow("", 6, "rs45", "", 89, 181, 22)] // invalid long
    [DataRow("scooby", 6, "rs45", "", 89, 179, 36)] // invalid projectUid
    [DataRow("", 1, "", "", 89, 179, 37)] // missing radioSerial
    public void ValidateGetProjectAndAssetUidsRequest_ValidationErrors
      (string projectUid, int deviceType, string radioSerial, string tccOrgUid,
        double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      GetProjectAndAssetUidsRequest projectAndAssetUidsRequest =
        GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest
        (projectUid, deviceType, radioSerial, tccOrgUid, 
          latitude, longitude, timeOfPosition);
      var ex = Assert.ThrowsException<ServiceException>(() => projectAndAssetUidsRequest.Validate());
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);

      var errorMessage = contractExecutionStatesEnum.FirstNameWithOffset(errorCode);
      var internalCode = contractExecutionStatesEnum.GetErrorNumberwithOffset(errorCode);
      var exceptionMessage = string.Format(projectUidPrefix + TrexExceptionTemplate, internalCode, errorMessage);
      Assert.AreEqual(exceptionMessage, ex.GetContent);
    }

    [TestMethod]
    [DataRow("", 6, "rs45", "", 89, 179, 0)]
    [DataRow("87e6bd66-54d8-4651-8907-88b15d81b2d7", 1, "rs45", "", 89, 179, 0)]
    [DataRow("87e6bd66-54d8-4651-8907-88b15d81b2d7", 1, "", "87e6bd65-54d8-4651-8907-88b15d81b2d7", 89, 179, 0)]
    public void ValidateGetProjectAndAssetUidsRequest_ValidationHappyPath
    (string projectUid, int deviceType, string radioSerial, string tccOrgUid,
      double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      GetProjectAndAssetUidsRequest projectAndAssetUidsRequest =
        GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, deviceType, radioSerial,
          tccOrgUid, latitude, longitude, timeOfPosition);
      projectAndAssetUidsRequest.Validate();
    }
  }
}
