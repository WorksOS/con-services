using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.MasterData.Models.Models;

namespace WebApiTests.Models
{
  [TestClass]
  public class ProjectAndAssetUidsRequestTests : ModelBaseTests
  {
    [TestMethod]
    [DataRow("", 999, "rs45Serial", "", "", 89, 179, 30)] // invalid deviceType
    [DataRow("", 6, "rs45Serial", "", "", 91, 179, 21)] // invalid lat
    [DataRow("", 6, "rs45Serial", "", "", 89, 181, 22)] // invalid long
    [DataRow("scooby", 6, "rs45Serial", "", "", 89, 179, 36)] // invalid projectUid
    [DataRow("", 0, "", "", "", 89, 179, 37)] // missing radioSerial, ec520 and tccOrgId
    public void ValidateGetProjectAndAssetUidsRequest_ValidationErrors
      (string projectUid, int deviceType, string radioSerial, string ec520Serial, 
        string tccOrgUid,
        double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      GetProjectAndAssetUidsRequest projectAndAssetUidsRequest =
        GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest
        (projectUid, deviceType, radioSerial, ec520Serial, tccOrgUid, 
          latitude, longitude, timeOfPosition);
      var errorCodeResult = projectAndAssetUidsRequest.Validate();
      Assert.AreEqual(errorCode, errorCodeResult);
    }

    [TestMethod]
    [DataRow("", 6, "rs45", "", "", 89, 179, 0)]
    [DataRow("87e6bd66-54d8-4651-8907-88b15d81b2d7", 6, "rs45Serial", "", "", 89, 179, 0)]
    [DataRow("87e6bd66-54d8-4651-8907-88b15d81b2d7", 0, "", "", "87e6bd65-54d8-4651-8907-88b15d81b2d7", 89, 179, 0)]
    [DataRow("87e6bd66-54d8-4651-8907-88b15d81b2d7", 0, "", "ec520Serial", "", 89, 179, 0)]
    public void ValidateGetProjectAndAssetUidsRequest_ValidationHappyPath
    (string projectUid, int deviceType, string radioSerial, string ec520Serial, 
      string tccOrgUid,
      double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      GetProjectAndAssetUidsRequest projectAndAssetUidsRequest =
        GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest
          (projectUid, deviceType, radioSerial, ec520Serial,
          tccOrgUid, latitude, longitude, timeOfPosition);
      var errorCodeResult = projectAndAssetUidsRequest.Validate();
      Assert.AreEqual(errorCode, errorCodeResult);
    }
  }
}
