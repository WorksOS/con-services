using System.Web.Http.ModelBinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using VSS.Nighthawk.ReferenceIdentifierService.Controllers;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using System.Web.Http.Hosting;
using Moq;
using System.Web.Http;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Constants;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;
using System.Net;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests.ControllerTests
{
  [TestClass]
  public class CredentialControllerTest
  {
    [TestMethod]
    public void CredentialControllerTests_Retrieve_Success()
    {
      Mock<ICredentialManager> _mockCredentialManager = new Mock<ICredentialManager>();
      CredentialController _controller = new CredentialController(_mockCredentialManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      _mockCredentialManager.Setup(o => o.RetrieveByUrl(It.IsAny<string>())).Returns(new Credentials { UserName = "User", EncryptedPassword = "Passwordd"});
      HttpResponseMessage response = _controller.Retrieve(_httpRequestMessage, "http://Test");
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      var lookupResponse = (LookupResponse<Credentials>)((ObjectContent)response.Content).Value;
      Assert.AreEqual("User", lookupResponse.Data.UserName);
      Assert.AreEqual("Passwordd", lookupResponse.Data.EncryptedPassword);
      Assert.IsNull(lookupResponse.Exception);
    }

    [TestMethod]
    public void CredentialControllerTests_Retrieve_ManagerThrowsException()
    {
      Mock<ICredentialManager> _mockCredentialManager = new Mock<ICredentialManager>();
      CredentialController _controller = new CredentialController(_mockCredentialManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      _mockCredentialManager.Setup(o => o.RetrieveByUrl(It.IsAny<string>()))
        .Throws(new Exception("an exception"));
      HttpResponseMessage response = _controller.Retrieve(_httpRequestMessage, "http://Test");
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
      var lookupResponse = (LookupResponse<Credentials>)((ObjectContent)response.Content).Value;
      Assert.IsNull(lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("an exception"));
    }

    [TestMethod]
    public void CredentialControllerTests_Retrieve_ModelBinderError()
    {
      Mock<ICredentialManager> _mockCredentialManager = new Mock<ICredentialManager>();
      CredentialController _controller = new CredentialController(_mockCredentialManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      var mockCredentialManager = new Mock<ICredentialManager>();
      var credentialController = new CredentialController(mockCredentialManager.Object);
      var httpRequestMessage = new HttpRequestMessage();
      credentialController.Request = httpRequestMessage;
      credentialController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      // set up binder error condition here
      credentialController
        .ModelState
        .Add(
          new KeyValuePair<string, ModelState>(
            ModelBinderConstants.IdentifierDefinitionModelBinderError, 
            new ModelState
            {
              Errors = { new ModelError(new Exception("Some model exception."), "Model exception occurred.") }
            }));

      var response = credentialController.Retrieve(httpRequestMessage, "http://Test");
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
      var lookupResponse = (LookupResponse<Credentials>)((ObjectContent)response.Content).Value;
      Assert.IsNull(lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("Model exception"));
    }
  }
}
