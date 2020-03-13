using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.TagFileAuth.Models;

namespace WebApiTests.Models
{
  [TestClass]
  public class ProjectAndAssetUidsRequestTests : ModelBaseTests
  {
    [TestMethod]
    [DataRow("", 999, "snm940Serial", "", "", 89, 179, 3030)] // invalid deviceType
    public void ValidateGetProjectAndAssetUidsRequest_InvalidDeviceType
      (string projectUid, int deviceType, string radioSerial, string ec520Serial, 
        string tccOrgUid,
        double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      var projectAndAssetUidsRequest =
        new GetProjectAndAssetUidsRequest
        (projectUid, deviceType, radioSerial, ec520Serial, tccOrgUid, 
          latitude, longitude, timeOfPosition);
      var ex = Assert.ThrowsException<ServiceException>(() => projectAndAssetUidsRequest.Validate());

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(errorCode, ex.GetResult.Code);
    }

    [TestMethod]
    [DataRow("", DeviceTypeEnum.SNM940, "snm940Serial", "", "", 91, 179, 3021)] // invalid lat
    [DataRow("", DeviceTypeEnum.SNM940, "snm940Serial", "", "", 89, 181, 3022)] // invalid long
    [DataRow("scooby", DeviceTypeEnum.SNM940, "snm940Serial", "", "", 89, 179, 3036)] // invalid projectUid
    [DataRow("", DeviceTypeEnum.MANUALDEVICE, "", "", "", 89, 179, 3037)] // missing serialNumber, ec520 and tccOrgId
    public void ValidateGetProjectAndAssetUidsRequest_ValidationErrors
    (string projectUid, DeviceTypeEnum deviceType, string radioSerial, string ec520Serial,
      string tccOrgUid,
      double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      var projectAndAssetUidsRequest =
        new GetProjectAndAssetUidsRequest
        (projectUid, (int)deviceType, radioSerial, ec520Serial, tccOrgUid,
          latitude, longitude, timeOfPosition);

      var ex = Assert.ThrowsException<ServiceException>(() => projectAndAssetUidsRequest.Validate());

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(errorCode, ex.GetResult.Code);
     
      //var errorCodeResult = projectAndAssetUidsRequest.Validate();
      //Assert.AreEqual(errorCode, errorCodeResult);
    }

    [TestMethod]
    [DataRow("", DeviceTypeEnum.SNM940, "snm940Serial", "", "", 89, 179, 0)]
    [DataRow("87e6bd66-54d8-4651-8907-88b15d81b2d7", DeviceTypeEnum.SNM940, "snm940Serial", "", "", 89, 179, 0)]
    [DataRow("87e6bd66-54d8-4651-8907-88b15d81b2d7", DeviceTypeEnum.MANUALDEVICE, "", "", "87e6bd65-54d8-4651-8907-88b15d81b2d7", 89, 179, 0)]
    [DataRow("87e6bd66-54d8-4651-8907-88b15d81b2d7", DeviceTypeEnum.MANUALDEVICE, "", "ec520Serial", "", 89, 179, 0)]
    public void ValidateGetProjectAndAssetUidsRequest_ValidationHappyPath
    (string projectUid, DeviceTypeEnum deviceType, string radioSerial, string ec520Serial, 
      string tccOrgUid,
      double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      var projectAndAssetUidsRequest =
        new GetProjectAndAssetUidsRequest
        (projectUid, (int)deviceType, radioSerial, ec520Serial,
          tccOrgUid, latitude, longitude, timeOfPosition);
      projectAndAssetUidsRequest.Validate();
    }
  }
}
