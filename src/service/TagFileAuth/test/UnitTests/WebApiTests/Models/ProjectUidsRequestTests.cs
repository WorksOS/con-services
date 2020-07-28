using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.Models;

namespace WebApiTests.Models
{
  [TestClass]
  public class ProjectUidsRequestTests : ModelBaseTests
  {
    [TestMethod]
    [DataRow("", "ec520Serial",91, 179, null, null, 3021)] // invalid lat
    [DataRow("", "cb450Serial", 89, 181, null, null, 3022)] // invalid long
    [DataRow("scooby", "CB460Serial", 89, 179, null, null, 3036)] // invalid projectUid
    [DataRow("", "cb450Serial", 0, 0, null, null, 3054)] // missing LL and NE

    public void ValidateGeProjectUidsRequest_ValidationErrors
    (string projectUid, string platformSerial,
      double latitude, double longitude, double? northing, double? easting, int errorCode)
    {
      var projectAndAssetUidsRequest =
        new GetProjectUidsRequest
        (projectUid, platformSerial, 
          latitude, longitude, northing, easting);

      var ex = Assert.ThrowsException<ServiceException>(() => projectAndAssetUidsRequest.Validate());

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(errorCode, ex.GetResult.Code);
    }

    [TestMethod]
    [DataRow("", "ec520Serial",89, 179, null, null, 0)]
    [DataRow("", "cb450serial", 0, 0, 560.1, 1000.56, 0)]
    [DataRow("", "ec520-wSerial", 0, 0, 0.0, 0.0, 0)]
    public void ValidateGetProjectUidsRequest_ValidationHappyPath
    (string projectUid, string platformSerial, 
      double latitude, double longitude, double? northing, double? easting, int errorCode)
    {
      var projectAndAssetUidsRequest =
        new GetProjectUidsRequest
        (projectUid, platformSerial,
          latitude, longitude, northing, easting);
      projectAndAssetUidsRequest.Validate();
    }
  }
}
