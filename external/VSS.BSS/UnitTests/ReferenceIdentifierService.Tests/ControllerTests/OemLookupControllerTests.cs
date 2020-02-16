using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Nighthawk.ReferenceIdentifierService.Controllers;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;
using VSS.Nighthawk.ReferenceIdentifierService.Workers;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests.ControllerTests
{
  [TestClass]
  public class OemLookupControllerTests
  {
    [TestMethod]
    public void OemLookupControllerTest_FindOemIdentifierByCustomerId_Success()
    {
      Mock<IOemLookupManager> _mockOemLookupManager = new Mock<IOemLookupManager>();
      OemLookupController _controller = new OemLookupController(_mockOemLookupManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      _mockOemLookupManager.Setup(o => o.FindOemIdentifierByCustomerId(It.IsAny<long>())).Returns(123);
      HttpResponseMessage response = _controller.FindOemIdentifierByCustomerId(_httpRequestMessage, 321);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      var lookupResponse = (LookupResponse<int>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(123, lookupResponse.Data);
      Assert.IsNull(lookupResponse.Exception);
    }

    [TestMethod]
    public void OemLookupControllerTest_FindOemIdentifierByCustomerId_Exception()
    {
      Mock<IOemLookupManager> _mockOemLookupManager = new Mock<IOemLookupManager>();
      OemLookupController _controller = new OemLookupController(_mockOemLookupManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      _mockOemLookupManager.Setup(o => o.FindOemIdentifierByCustomerId(It.IsAny<long>())).Throws(new Exception("Ex"));

      HttpResponseMessage response = _controller.FindOemIdentifierByCustomerId(_httpRequestMessage, 321L);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
      var lookupResponse = (LookupResponse<int>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(0L, lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("Ex"));
    }
  }
}
