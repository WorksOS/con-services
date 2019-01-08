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
    public void ValidateGetProjectAndAssetUidsRequest_InvalidDeviceType
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
    [DataRow("", DeviceTypeEnum.SNM940, "rs45Serial", "", "", 91, 179, 21)] // invalid lat
    [DataRow("", DeviceTypeEnum.SNM940, "rs45Serial", "", "", 89, 181, 22)] // invalid long
    [DataRow("scooby", DeviceTypeEnum.SNM940, "rs45Serial", "", "", 89, 179, 36)] // invalid projectUid
    [DataRow("", DeviceTypeEnum.MANUALDEVICE, "", "", "", 89, 179, 37)] // missing radioSerial, ec520 and tccOrgId
    public void ValidateGetProjectAndAssetUidsRequest_ValidationErrors
    (string projectUid, DeviceTypeEnum deviceType, string radioSerial, string ec520Serial,
      string tccOrgUid,
      double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      GetProjectAndAssetUidsRequest projectAndAssetUidsRequest =
        GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest
        (projectUid, (int)deviceType, radioSerial, ec520Serial, tccOrgUid,
          latitude, longitude, timeOfPosition);
      var errorCodeResult = projectAndAssetUidsRequest.Validate();
      Assert.AreEqual(errorCode, errorCodeResult);
    }

    [TestMethod]
    [DataRow("", DeviceTypeEnum.SNM940, "rs45", "", "", 89, 179, 0)]
    [DataRow("87e6bd66-54d8-4651-8907-88b15d81b2d7", DeviceTypeEnum.SNM940, "rs45Serial", "", "", 89, 179, 0)]
    [DataRow("87e6bd66-54d8-4651-8907-88b15d81b2d7", DeviceTypeEnum.MANUALDEVICE, "", "", "87e6bd65-54d8-4651-8907-88b15d81b2d7", 89, 179, 0)]
    [DataRow("87e6bd66-54d8-4651-8907-88b15d81b2d7", DeviceTypeEnum.MANUALDEVICE, "", "ec520Serial", "", 89, 179, 0)]
    public void ValidateGetProjectAndAssetUidsRequest_ValidationHappyPath
    (string projectUid, DeviceTypeEnum deviceType, string radioSerial, string ec520Serial, 
      string tccOrgUid,
      double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      GetProjectAndAssetUidsRequest projectAndAssetUidsRequest =
        GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest
          (projectUid, (int)deviceType, radioSerial, ec520Serial,
          tccOrgUid, latitude, longitude, timeOfPosition);
      var errorCodeResult = projectAndAssetUidsRequest.Validate();
      Assert.AreEqual(errorCode, errorCodeResult);
    }
  }
}
