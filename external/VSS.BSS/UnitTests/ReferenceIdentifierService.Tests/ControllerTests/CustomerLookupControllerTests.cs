using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.Bss;
using VSS.Nighthawk.ReferenceIdentifierService.Controllers;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests.ControllerTests
{
  [TestClass]
  public class CustomerLookupControllerTests
  {
    [TestMethod]
    public void CustomerLookupControllerTest_FindDealers_Success()
    {
      Mock<ICustomerLookupManager> _mockCustomerLookupManager = new Mock<ICustomerLookupManager>();
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      CustomerLookupController _controller = new CustomerLookupController(_mockCustomerLookupManager.Object);
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      _mockCustomerLookupManager.Setup(o =>o
        .FindDealers(It.IsAny<List<IdentifierDefinition>>(), It.IsAny<long>()))
        .Returns(new List<IdentifierDefinition> { new IdentifierDefinition { Alias = "keyDealer1", Value = "valueDealer1" }});

      HttpResponseMessage response = _controller.FindDealers(_httpRequestMessage, It.IsAny<long>(), "keyOrg1,valueOrg1;keyOrg2,valueOrg2");
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      var lookupResponse = (LookupResponse<IList<IdentifierDefinition>>)((ObjectContent)response.Content).Value;
      Assert.AreEqual("keyDealer1", lookupResponse.Data[0].Alias);
      Assert.AreEqual("valueDealer1", lookupResponse.Data[0].Value);
      Assert.IsNull(lookupResponse.Exception);
    }

    [TestMethod]
    public void CustomerLookupControllerTest_FindDealers_NoContent()
    {
      Mock<ICustomerLookupManager> _mockCustomerLookupManager = new Mock<ICustomerLookupManager>();
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      CustomerLookupController _controller = new CustomerLookupController(_mockCustomerLookupManager.Object);
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      _mockCustomerLookupManager.Setup(o => o.FindDealers(It.IsAny<List<IdentifierDefinition>>(), It.IsAny<long>())).Throws(new Exception("Ex"));

      HttpResponseMessage response = _controller.FindDealers(_httpRequestMessage, It.IsAny<long>(), "");
      Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
      var lookupResponse = (LookupResponse<IList<IdentifierDefinition>>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(null, lookupResponse.Data);
    }

    [TestMethod]
    public void CustomerLookupControllerTest_FindDealers_Exception()
    {
      Mock<ICustomerLookupManager> _mockCustomerLookupManager = new Mock<ICustomerLookupManager>();
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      CustomerLookupController _controller = new CustomerLookupController(_mockCustomerLookupManager.Object);
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      _mockCustomerLookupManager.Setup(o => o.FindDealers(It.IsAny<List<IdentifierDefinition>>(), It.IsAny<long>())).Throws(new Exception("Ex"));

      HttpResponseMessage response = _controller.FindDealers(_httpRequestMessage, It.IsAny<long>(), "keyOrg1,valueOrg1;keyOrg2,valueOrg2");
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
      var lookupResponse = (LookupResponse<IList<IdentifierDefinition>>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(null, lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("Ex"));
    }

    [TestMethod]
    public void CustomerLookupControllerTest_FindCustomerGuidByCustomerId_Success()
    {
      Mock<ICustomerLookupManager> _mockCustomerLookupManager = new Mock<ICustomerLookupManager>();
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      CustomerLookupController _controller = new CustomerLookupController(_mockCustomerLookupManager.Object);
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      var customerGuid = Guid.NewGuid();
      _mockCustomerLookupManager.Setup(o => o.FindCustomerGuidByCustomerId(It.IsAny<long>()))
        .Returns(customerGuid);

      HttpResponseMessage response = _controller.FindCustomerGuidByCustomerId(_httpRequestMessage, It.IsAny<long>());
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      var lookupResponse = (LookupResponse<Guid?>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(customerGuid, lookupResponse.Data);
      Assert.IsNull(lookupResponse.Exception);
    }

    [TestMethod]
    public void CustomerLookupControllerTest_FindCustomerGuidByCustomerId_Exception()
    {
      Mock<ICustomerLookupManager> _mockCustomerLookupManager = new Mock<ICustomerLookupManager>();
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      CustomerLookupController _controller = new CustomerLookupController(_mockCustomerLookupManager.Object);
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      _mockCustomerLookupManager.Setup(o => o.FindCustomerGuidByCustomerId(It.IsAny<long>())).Throws(new Exception("Ex"));

      HttpResponseMessage response = _controller.FindCustomerGuidByCustomerId(_httpRequestMessage, It.IsAny<long>());
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
      var lookupResponse = (LookupResponse<Guid?>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(null, lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("Ex"));
    }

    [TestMethod]
    public void CustomerLookupControllerTest_FindAllCustomersForService_Success()
    {
      Mock<ICustomerLookupManager> _mockCustomerLookupManager = new Mock<ICustomerLookupManager>();
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      CustomerLookupController _controller = new CustomerLookupController(_mockCustomerLookupManager.Object);
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      var customerOneGuid = Guid.NewGuid();
      var customerTwoGuid = Guid.NewGuid();
      _mockCustomerLookupManager.Setup(o => o.FindAllCustomersForService(It.IsAny<Guid>()))
        .Returns(new List<Guid?> {customerOneGuid, customerTwoGuid});

      HttpResponseMessage response = _controller.FindAllCustomersForService(_httpRequestMessage, It.IsAny<Guid>());
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      var lookupResponse = (LookupResponse<List<Guid?>>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(customerOneGuid, lookupResponse.Data[0]);
      Assert.AreEqual(customerTwoGuid, lookupResponse.Data[1]);
      Assert.IsNull(lookupResponse.Exception);
    }

    [TestMethod]
    public void CustomerLookupControllerTest_FindAllCustomersForService_Exception()
    {
      Mock<ICustomerLookupManager> _mockCustomerLookupManager = new Mock<ICustomerLookupManager>();
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      CustomerLookupController _controller = new CustomerLookupController(_mockCustomerLookupManager.Object);
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      _mockCustomerLookupManager.Setup(o => o.FindAllCustomersForService(It.IsAny<Guid>())).Throws(new Exception("Ex"));

      HttpResponseMessage response = _controller.FindAllCustomersForService(_httpRequestMessage, It.IsAny<Guid>());
      var lookupResponse = (LookupResponse<List<Guid?>>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(null, lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("Ex"));
    }

    [TestMethod]
    public void CustomerLookupControllerTest_FindCustomerParent_Success()
    {
      Mock<ICustomerLookupManager> mockCustomerLookupManager = new Mock<ICustomerLookupManager>();
      HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
      CustomerLookupController controller = new CustomerLookupController(mockCustomerLookupManager.Object)
      {
        Request = httpRequestMessage
      };
      controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      var childUid = Guid.NewGuid();
      var parentUid = Guid.NewGuid();
      var parentCustomerTypeString = CustomerTypeEnum.Dealer.ToString();
      mockCustomerLookupManager.Setup(o => o.FindCustomerParent(childUid, CustomerTypeEnum.Dealer)).Returns(parentUid);

      HttpResponseMessage response = controller.FindCustomerParent(httpRequestMessage, childUid, parentCustomerTypeString);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

      var lookupResponse = (LookupResponse<Guid?>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(parentUid, lookupResponse.Data);
      Assert.IsNull(lookupResponse.Exception);
    }

    [TestMethod]
    public void CustomerLookupControllerTest_FindAccountsForDealer_Success()
    {
      Mock<ICustomerLookupManager> mockCustomerLookupManager = new Mock<ICustomerLookupManager>();
      HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
      CustomerLookupController controller = new CustomerLookupController(mockCustomerLookupManager.Object)
      {
        Request = httpRequestMessage
      };
      controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      var dealerUid = Guid.NewGuid();
      var accountInfo = new AccountInfo { CustomerUid = Guid.NewGuid(), DealerAccountCode = "1234" };
      mockCustomerLookupManager.Setup(o => o.FindAccountsForDealer(dealerUid)).Returns(new List<AccountInfo>{accountInfo});

      HttpResponseMessage response = controller.FindAccountsForDealer(httpRequestMessage, dealerUid);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

      var lookupResponse = (LookupResponse<IList<AccountInfo>>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(accountInfo.CustomerUid, lookupResponse.Data.FirstOrDefault().CustomerUid);
      Assert.AreEqual(accountInfo.DealerAccountCode, lookupResponse.Data.FirstOrDefault().DealerAccountCode);
      Assert.IsNull(lookupResponse.Exception);
    }

    [TestMethod]
    public void CustomerLookupControllerTest_FindCustomerParent_LookupManagerException()
    {
      Mock<ICustomerLookupManager> mockCustomerLookupManager = new Mock<ICustomerLookupManager>();
      HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
      CustomerLookupController controller = new CustomerLookupController(mockCustomerLookupManager.Object)
      {
        Request = httpRequestMessage
      };
      controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      mockCustomerLookupManager.Setup(o => o.FindCustomerParent(It.IsAny<Guid>(), It.IsAny<CustomerTypeEnum>())).Throws(new Exception("Ex"));

      HttpResponseMessage response = controller.FindCustomerParent(httpRequestMessage, Guid.NewGuid(), CustomerTypeEnum.Dealer.ToString());
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      var lookupResponse = (LookupResponse<Guid?>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(null, lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("Ex"));
    }

    [TestMethod]
    public void CustomerLookupControllerTest_FindAccountsForDealer_LookupManagerException()
    {
      Mock<ICustomerLookupManager> mockCustomerLookupManager = new Mock<ICustomerLookupManager>();
      HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
      CustomerLookupController controller = new CustomerLookupController(mockCustomerLookupManager.Object)
      {
        Request = httpRequestMessage
      };
      controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      mockCustomerLookupManager.Setup(o => o.FindAccountsForDealer(It.IsAny<Guid>())).Throws(new Exception("Ex"));

      HttpResponseMessage response = controller.FindAccountsForDealer(httpRequestMessage, Guid.NewGuid());
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      var lookupResponse = (LookupResponse<IList<AccountInfo>>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(null, lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("Ex"));
    }

    [TestMethod]
    public void CustomerLookupControllerTest_FindCustomerParent_EnumParseException()
    {
      Mock<ICustomerLookupManager> mockCustomerLookupManager = new Mock<ICustomerLookupManager>();
      HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
      CustomerLookupController controller = new CustomerLookupController(mockCustomerLookupManager.Object)
      {
        Request = httpRequestMessage
      };
      controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      var parentCustomerTypeString = "This is not a valid customer type";
      HttpResponseMessage response = controller.FindCustomerParent(httpRequestMessage, Guid.NewGuid(), parentCustomerTypeString);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      var lookupResponse = (LookupResponse<Guid?>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(null, lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception is ArgumentException);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains(parentCustomerTypeString));
    }

    [TestMethod]
    public void CustomerLookupControllerTest_FindDealers_FirstOrgIdentifiersIsNullOrWhiteSpace_Exception()
    {
      Mock<ICustomerLookupManager> _mockCustomerLookupManager = new Mock<ICustomerLookupManager>();
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      CustomerLookupController _controller = new CustomerLookupController(_mockCustomerLookupManager.Object);
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      var mockCustomerLookupManager = new Mock<ICustomerLookupManager>();
      var httpRequestMessage = new HttpRequestMessage();
      var controller = new CustomerLookupController(mockCustomerLookupManager.Object);
      controller.Request = httpRequestMessage;
      controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
      mockCustomerLookupManager.Setup(o => o.FindDealers(It.IsAny<List<IdentifierDefinition>>(), It.IsAny<long>())).Throws(new Exception("Ex"));

      // ";" will cause the first org identifier to be empty
      HttpResponseMessage response = controller.FindDealers(httpRequestMessage, It.IsAny<long>(), ";");
      Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
      var lookupResponse = (LookupResponse<IList<IdentifierDefinition>>)((ObjectContent)response.Content).Value;
      Assert.IsNull(lookupResponse.Data);
      Assert.IsNull(lookupResponse.Exception);
    }
  }
}
