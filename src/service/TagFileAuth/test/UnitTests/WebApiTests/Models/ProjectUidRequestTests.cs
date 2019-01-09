using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.MasterData.Models.Models;

namespace WebApiTests.Models
{
  [TestClass]
  public class ProjectUidRequestTests : ModelBaseTests
  {
    [TestMethod]
    [DataRow(1, "rs45", 89, 179, 30)]
    [DataRow(6, "", 89, 179, 10)]
    [DataRow(6, "rs45", 91, 179, 21)]
    [DataRow(6, "rs45", 89, 181, 22)]
    public void ValidateGetProjectUidRequest_ValidationErrors(int deviceType, string radioSerial,
    double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      GetProjectUidRequest projectUidRequest = GetProjectUidRequest.CreateGetProjectUidRequest(deviceType, radioSerial, latitude, longitude, timeOfPosition);
      var errorCodeResult = projectUidRequest.Validate();
      Assert.AreEqual(errorCode, errorCodeResult);
    }

   [TestMethod]
   [DataRow(6, "rs45", 89, 179, 0)]
    public void ValidateGetProjectUidRequest_ValidationHappyPath(int deviceType, string radioSerial,
      double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      GetProjectUidRequest projectUidRequest = GetProjectUidRequest.CreateGetProjectUidRequest(deviceType, radioSerial, latitude, longitude, timeOfPosition);
      var errorCodeResult = projectUidRequest.Validate();
      Assert.AreEqual(errorCode, errorCodeResult);
    }
  }
}
