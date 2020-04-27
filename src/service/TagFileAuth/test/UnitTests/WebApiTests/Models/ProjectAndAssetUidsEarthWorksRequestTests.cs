using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.Models;

namespace WebApiTests.Models
{
  [TestClass]
  public class ProjectAndAssetUidsEarthWorksRequestTests : ModelBaseTests
  {
    [TestMethod]
    [DataRow("snm940Serial", "ec520Serial", 91, 179, 3021)] // invalid lat
    [DataRow("", "ec520Serial", 89, 181, 3022)] // invalid long
    [DataRow("snm940Serial", "", 89, 179, 3051)] // must have ec520 serial
    public void ValidateGetProjectAndAssetUidsEarthWorksRequest_ValidationErrors(
      string radioSerial, string ec520Serial, double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      var projectAndAssetUidsEarthWorksRequest =
        new GetProjectAndAssetUidsEarthWorksRequest (ec520Serial, radioSerial, latitude, longitude, timeOfPosition);

      var ex = Assert.ThrowsException<ServiceException>(() => projectAndAssetUidsEarthWorksRequest.Validate());

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(errorCode, ex.GetResult.Code);     
    }

    [TestMethod]
    [DataRow("snm940Serial", "ec520Serial", 89, 179, 0)]
    [DataRow("", "ec520Serial", 89, 179, 0)]
    public void ValidateGetProjectAndAssetUidsEarthWorksRequest_ValidationHappyPath
    (string radioSerial, string ec520Serial, double latitude, double longitude, int errorCode)
    {
      var timeOfPosition = DateTime.UtcNow;
      var projectAndAssetUidsEarthWorksRequest =
        new GetProjectAndAssetUidsEarthWorksRequest(ec520Serial, radioSerial, latitude, longitude, timeOfPosition);
      projectAndAssetUidsEarthWorksRequest.Validate();
    }
  }
}
