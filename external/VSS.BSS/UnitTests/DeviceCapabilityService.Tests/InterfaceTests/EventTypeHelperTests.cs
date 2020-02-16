using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Net;
//using VSS.Hosted.VLCommon;
//using VSS.Hosted.VLCommon.Events;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.BaseEvents;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Query;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.InterfaceTests
{
  public interface IEventTypeHelperTestObject : IEndpointDestinedEvent { }
  public class EventTypeHelperTestObject : IEventTypeHelperTestObject { public EndpointDefinition[] Destinations { get; set; } }

  /// <summary>
  /// Summary description for LocationStatusUpdateRequestedEventFactoryTests
  /// </summary>
  [TestClass]
  public class EventTypeHelperTests
  {
    [TestMethod]
    public void GetQueryString_DeviceTypeHasValueSuccess()
    {
      IDeviceQuery testDeviceQuery = new DeviceQuery() { AssetID = null, DeviceType = DeviceTypeEnum.TAP66, GPSDeviceID = "12345", ID = null };
      string expectedUrl = string.Format("gpsdeviceid={0}&devicetype={1}", testDeviceQuery.GPSDeviceID, testDeviceQuery.DeviceType);
      string resultUrl = EventTypeHelper.GetQueryString(testDeviceQuery);
      Assert.IsTrue(expectedUrl.Equals(resultUrl));
    }

    [TestMethod]
    public void GetQueryString_IdHasValueSuccess()
    {
      IDeviceQuery testDeviceQuery = new DeviceQuery() { AssetID = null, DeviceType = null, GPSDeviceID = null, ID = 12345 };
      string expectedUrl = string.Format("id={0}", testDeviceQuery.ID);
      string resultUrl = EventTypeHelper.GetQueryString(testDeviceQuery);
      Assert.IsTrue(expectedUrl.Equals(resultUrl));
    }

    [TestMethod]
    public void GetQueryString_AssetIdHasValueSuccess()
    {
      IDeviceQuery testDeviceQuery = new DeviceQuery() { AssetID = 12345, DeviceType = null, GPSDeviceID = null, ID = null };
      string expectedUrl = string.Format("assetid={0}", testDeviceQuery.AssetID);
      string resultUrl = EventTypeHelper.GetQueryString(testDeviceQuery);
      Assert.IsTrue(expectedUrl.Equals(resultUrl));
    }

    [TestMethod]
    public void QueryServiceForTypeAndBuildInstance_UriFormatSuccess()
    {
      string resultUrl = string.Empty;
      const string testServerAction = "testServerAction";
      const string testDeviceCapabilitySvcUri = "testDeviceCapabilitySvcUri";
      IDeviceQuery testDeviceQuery = new DeviceQuery() { DeviceType = null, ID = 12345};
      string expectedUrl = String.Format("{0}/{1}?{2}", testDeviceCapabilitySvcUri, testServerAction, EventTypeHelper.GetQueryString(testDeviceQuery));

      var mockHttpClientWrapper = new Mock<IHttpClientWrapper>();
      var mockHttpResponseWrapper = new Mock<IHttpResponseWrapper>();
      string testInterfaceAssemblyQualifiedName = typeof(IEventTypeHelperTestObject).AssemblyQualifiedName;
      FactoryOutboundEventTypeDescriptor factoryTypeDescriptor = new FactoryOutboundEventTypeDescriptor 
      { 
        AssemblyQualifiedName = testInterfaceAssemblyQualifiedName,
        Destinations = new EndpointDescriptor[0]
      };

      mockHttpResponseWrapper.SetupGet(o => o.StatusCode).Returns(HttpStatusCode.OK);
      mockHttpResponseWrapper.Setup(o => o.StaticBody<FactoryOutboundEventTypeDescriptor>(It.IsAny<string>())).Returns(factoryTypeDescriptor);
      mockHttpClientWrapper.Setup(o => o.Get(It.IsAny<string>(), It.IsAny<object>())).Callback<string, object>((x, y) => { resultUrl = x; }).Returns(mockHttpResponseWrapper.Object);

      IEventTypeHelper eventTypeHelper = new EventTypeHelper(mockHttpClientWrapper.Object);
      eventTypeHelper.QueryServiceForTypeAndBuildInstance<IEventTypeHelperTestObject>(testServerAction, testDeviceCapabilitySvcUri, testDeviceQuery);

      Assert.IsTrue(expectedUrl.Equals(resultUrl));
    }

    [TestMethod]
    public void QueryServiceForTypeAndBuildInstance_TypeSuccess()
    {
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockHttpClientWrapper = new Mock<IHttpClientWrapper>();
      var mockHttpResponseWrapper = new Mock<IHttpResponseWrapper>();
      string testInterfaceAssemblyQualifiedName = typeof (IEventTypeHelperTestObject).AssemblyQualifiedName;
      EndpointDescriptor[] endpointDescriptors = 
      { 
        new EndpointDescriptor()
        {
          ContentType = "ContentType",
          EncryptedPwd = "EncryptedPwd",
          Id = 42,
          Name = "Name",
          Url = "Url",
          Username = "Username"
        }
      };
      FactoryOutboundEventTypeDescriptor factoryTypeDescriptor = new FactoryOutboundEventTypeDescriptor 
      { 
        AssemblyQualifiedName = testInterfaceAssemblyQualifiedName,
        Destinations = endpointDescriptors
      };

      mockHttpResponseWrapper.SetupGet(o => o.StatusCode).Returns(HttpStatusCode.OK);
      mockHttpResponseWrapper.Setup(o => o.StaticBody<FactoryOutboundEventTypeDescriptor>(It.IsAny<string>())).Returns(factoryTypeDescriptor);
      mockHttpClientWrapper.Setup(o => o.Get(It.IsAny<string>(), It.IsAny<object>())).Returns(mockHttpResponseWrapper.Object);

      IEventTypeHelper eventTypeHelper = new EventTypeHelper(mockHttpClientWrapper.Object);
      var eventTypeHelperTestObject = eventTypeHelper.QueryServiceForTypeAndBuildInstance<IEventTypeHelperTestObject>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object);

      Assert.IsTrue(eventTypeHelperTestObject.GetType().GetInterfaces().Any(i => i.AssemblyQualifiedName == testInterfaceAssemblyQualifiedName));
      Assert.AreEqual(endpointDescriptors.First().ContentType, eventTypeHelperTestObject.Destinations.First().ContentType);
      Assert.AreEqual(endpointDescriptors.First().EncryptedPwd, Convert.ToBase64String(eventTypeHelperTestObject.Destinations.First().EncryptedPwd));
      Assert.AreEqual(endpointDescriptors.First().Id, eventTypeHelperTestObject.Destinations.First().EndpointDefinitionId);
      Assert.AreEqual(endpointDescriptors.First().Name, eventTypeHelperTestObject.Destinations.First().Name);
      Assert.AreEqual(endpointDescriptors.First().Url, eventTypeHelperTestObject.Destinations.First().Url);
      Assert.AreEqual(endpointDescriptors.First().Username, eventTypeHelperTestObject.Destinations.First().UserName);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception), "QueryServiceForTypeAndBuildInstance_ServiceCallError")]
    public void QueryServiceForTypeAndBuildInstance_ServiceCallError()
    {
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockHttpClientWrapper = new Mock<IHttpClientWrapper>();
      var mockHttpResponseWrapper = new Mock<IHttpResponseWrapper>();

      mockHttpResponseWrapper.SetupGet(o => o.StatusCode).Returns(HttpStatusCode.InternalServerError);
      mockHttpResponseWrapper.SetupGet(o => o.RawText).Returns("QueryServiceForTypeAndBuildInstance_ServiceCallError");
      mockHttpClientWrapper.Setup(o => o.Get(It.IsAny<string>(), It.IsAny<object>())).Returns(mockHttpResponseWrapper.Object);

      IEventTypeHelper eventTypeHelper = new EventTypeHelper(mockHttpClientWrapper.Object);
      eventTypeHelper.QueryServiceForTypeAndBuildInstance<IEventTypeHelperTestObject>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object);
    }
  }
}
