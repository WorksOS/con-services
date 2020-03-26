using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;

namespace WebApiTests.Models
{
  [TestClass]
  public class ProjectAndAssetUidsRequestTests : ModelBaseTests
  {
    [TestMethod]
    [DataRow("", 999, "snm940Serial", "", 89, 179, 3030)] // invalid deviceType
    public void ValidateGetProjectAndAssetUidsRequest_InvalidDeviceType
      (string projectUid, int deviceType, string radioSerial, string ec520Serial, 
        double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      var projectAndAssetUidsRequest =
        new GetProjectAndAssetUidsRequest
        (projectUid, deviceType, radioSerial, ec520Serial, 
          latitude, longitude, timeOfPosition);
      var ex = Assert.ThrowsException<ServiceException>(() => projectAndAssetUidsRequest.Validate());

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(errorCode, ex.GetResult.Code);
    }

    [TestMethod]
    [DataRow("", TagFileDeviceTypeEnum.SNM940, "snm940Serial", "", 91, 179, 3021)] // invalid lat
    [DataRow("", TagFileDeviceTypeEnum.SNM940, "snm940Serial", "", 89, 181, 3022)] // invalid long
    [DataRow("scooby", TagFileDeviceTypeEnum.SNM940, "snm940Serial","", 89, 179, 3036)] // invalid projectUid
    [DataRow("", TagFileDeviceTypeEnum.ManualImport, "", "", 89, 179, 3037)] // missing serialNumber, ec520 and tccOrgId
    public void ValidateGetProjectAndAssetUidsRequest_ValidationErrors
    (string projectUid, TagFileDeviceTypeEnum deviceType, string radioSerial, string ec520Serial,

      double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      var projectAndAssetUidsRequest =
        new GetProjectAndAssetUidsRequest
        (projectUid, (int)deviceType, radioSerial, ec520Serial, 
          latitude, longitude, timeOfPosition);

      var ex = Assert.ThrowsException<ServiceException>(() => projectAndAssetUidsRequest.Validate());

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(errorCode, ex.GetResult.Code);
     
      //var errorCodeResult = projectAndAssetUidsRequest.Validate();
      //Assert.AreEqual(errorCode, errorCodeResult);
    }

    [TestMethod]
    [DataRow("", TagFileDeviceTypeEnum.SNM940, "snm940Serial", "", 89, 179, 0)]
   // [DataRow("87e6bd66-54d8-4651-8907-88b15d81b2d7", TagFileDeviceTypeEnum.SNM940, "snm940Serial", "", 89, 179, 0)]
   // [DataRow("87e6bd66-54d8-4651-8907-88b15d81b2d7", TagFileDeviceTypeEnum.ManualImport, "", "ec520Serial", 89, 179, 0)]
    public void ValidateGetProjectAndAssetUidsRequest_ValidationHappyPath
    (string projectUid, TagFileDeviceTypeEnum deviceType, string radioSerial, string ec520Serial, 
      double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      var projectAndAssetUidsRequest =
        new GetProjectAndAssetUidsRequest
        (projectUid, (int)deviceType, radioSerial, ec520Serial,
          latitude, longitude, timeOfPosition);
      projectAndAssetUidsRequest.Validate();
    }
  }
}
