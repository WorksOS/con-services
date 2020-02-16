using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Nighthawk.ReferenceIdentifierService.Controllers;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests.ControllerTests
{
  [TestClass]
  public class StoreLookupControllerTests
  {
    [TestMethod]
    public void StoreLookupControllerTest_FindStoreByCustomerId_Success()
    {
      Mock<IStoreLookupManager> _mockStoreLookupManager = new Mock<IStoreLookupManager>();
      StoreLookupController _controller = new StoreLookupController(_mockStoreLookupManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      _mockStoreLookupManager.Setup(o => o.FindStoreByCustomerId(It.IsAny<long>())).Returns(123L);
      HttpResponseMessage response = _controller.FindStoreByCustomerId(_httpRequestMessage, 321L);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      var lookupResponse = (LookupResponse<long>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(123L, lookupResponse.Data);
      Assert.IsNull(lookupResponse.Exception);
    }

    [TestMethod]
    public void StoreLookupControllerTest_FindStoreByCustomerId_Exception()
    {
      Mock<IStoreLookupManager> _mockStoreLookupManager = new Mock<IStoreLookupManager>();
      StoreLookupController _controller = new StoreLookupController(_mockStoreLookupManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      _mockStoreLookupManager.Setup(o => o.FindStoreByCustomerId(It.IsAny<long>())).Throws(new Exception("Ex"));

      HttpResponseMessage response = _controller.FindStoreByCustomerId(_httpRequestMessage, 321L);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
      var lookupResponse = (LookupResponse<long>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(0L, lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("Ex"));
    }
  }
}
