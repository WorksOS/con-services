using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.Models;

namespace WebApiTests.Models
{
  [TestClass]
  public class GetProjectUidsEarthWorksRequestTests : ModelBaseTests
  {
    [TestMethod]
    [DataRow( "ec520Serial", 91, 179, 3021)] // invalid lat
    [DataRow( "cb460Serial", 89, 181, 3022)] // invalid long
    [DataRow("", 89, 179, 3051)] // must have ec520 serial
    public void ValidateGetProjectUidsBaseRequest_ValidationErrors(
     string platformSerial, double latitude, double longitude, int errorCode)
    {
      var projectAndAssetUidsEarthWorksRequest =
        new GetProjectUidsEarthWorksRequest(platformSerial, latitude, longitude);

      var ex = Assert.ThrowsException<ServiceException>(() => projectAndAssetUidsEarthWorksRequest.Validate());

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(errorCode, ex.GetResult.Code);     
    }

    [TestMethod]
    [DataRow( "ec520Serial", 89, 179, 0)]
    public void ValidateProjectUidsBaseRequest_ValidationHappyPath
    (string platformSerial, double latitude, double longitude, int errorCode)
    {
      var projectAndAssetUidsEarthWorksRequest =
        new GetProjectUidsEarthWorksRequest(platformSerial, latitude, longitude);
      projectAndAssetUidsEarthWorksRequest.Validate();
    }
  }
}
