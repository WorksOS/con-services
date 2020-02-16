using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
//using VSS.Hosted.VLCommon;
using  VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.Nighthawk.DeviceCapabilityService.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.Processors;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.ProcessorTests
{
  [TestClass]
  public class LocationUpdateRequestedProcessorTests
  {
    [TestMethod]
    public void GetLocationUpdateRequestedEvent_IDeviceQueryWithDeviceType_Success()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();
      Mock<IDeviceQueryHelper> _mockDeviceQueryHelper = new Mock<IDeviceQueryHelper>();
      Mock<IDeviceHandlerParameters> _mockParams = new Mock<IDeviceHandlerParameters>();
      _mockParams.Setup(o => o.UnknownDeviceHandler).Returns(new UnknownDeviceHandler());
      Mock<IDeviceHandlerStrategy> _strategy = new Mock<IDeviceHandlerStrategy>();
      _strategy.Setup(e => e.LocationUpdateRequestedEventType).Returns(typeof(TestClass));

      Mock<IDeviceQuery> _device = new Mock<IDeviceQuery>();

      IEnumerable<EndpointDescriptor> expectedEndpointDescriptors = GetTestEndpointDescriptors();
      _mockStorage.Setup(o => o.GetEndpointDescriptorsForNames(It.IsAny<IEnumerable<string>>())).Returns(expectedEndpointDescriptors);
      _mockParams.Setup(o => o.DeviceHandlers.ContainsKey(It.IsAny<DeviceTypeEnum>())).Returns(true);
      _mockParams.Setup(o => o.DeviceHandlers[It.IsAny<DeviceTypeEnum>()]).Returns(_strategy.Object);
      LocationUpdateRequestedProcessor loc = new LocationUpdateRequestedProcessor(_mockStorage.Object, _mockParams.Object, _mockDeviceQueryHelper.Object);

      _mockDeviceQueryHelper.Setup(o => o.GetDeviceType(It.IsAny<IDeviceQuery>(), _mockStorage.Object)).Returns(DeviceTypeEnum.PLE641);
      var factory = loc.GetLocationUpdateRequestedEvent(_device.Object);
      _mockDeviceQueryHelper.Verify(e => e.GetDeviceType(It.IsAny<IDeviceQuery>(), It.IsAny<IStorage>()), Times.Once());
      Assert.AreEqual(typeof(TestClass).AssemblyQualifiedName, factory.AssemblyQualifiedName);
      Assert.AreEqual(expectedEndpointDescriptors.First().ContentType, factory.Destinations.First().ContentType);
      Assert.AreEqual(expectedEndpointDescriptors.First().Id, factory.Destinations.First().Id);
      Assert.AreEqual(expectedEndpointDescriptors.First().Name, factory.Destinations.First().Name);
      Assert.AreEqual(expectedEndpointDescriptors.First().EncryptedPwd, factory.Destinations.First().EncryptedPwd);
      Assert.AreEqual(expectedEndpointDescriptors.First().Url, factory.Destinations.First().Url);
      Assert.AreEqual(expectedEndpointDescriptors.First().Username, factory.Destinations.First().Username);
    }

    [TestMethod]
    public void GetLocationUpdateRequestedEvent_IDeviceQueryWithOutDeviceType_Success()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();
      Mock<IDeviceQueryHelper> _mockDeviceQueryHelper = new Mock<IDeviceQueryHelper>();
      Mock<IDeviceHandlerParameters> _mockParams = new Mock<IDeviceHandlerParameters>();
      _mockParams.Setup(o => o.UnknownDeviceHandler).Returns(new UnknownDeviceHandler());
      Mock<IDeviceHandlerStrategy> _strategy = new Mock<IDeviceHandlerStrategy>();
      _strategy.Setup(e => e.LocationUpdateRequestedEventType).Returns(typeof(TestClass));

      Mock<IDeviceQuery> _device = new Mock<IDeviceQuery>();

      _mockDeviceQueryHelper.Setup(o => o.GetDeviceType(It.IsAny<IDeviceQuery>(), _mockStorage.Object)).Returns(DeviceTypeEnum.PLE641);
      _mockParams.Setup(o => o.DeviceHandlers.ContainsKey(It.IsAny<DeviceTypeEnum>())).Returns(true);
      _mockParams.Setup(o => o.DeviceHandlers[It.IsAny<DeviceTypeEnum>()]).Returns(_strategy.Object);
      LocationUpdateRequestedProcessor loc = new LocationUpdateRequestedProcessor(_mockStorage.Object, _mockParams.Object, _mockDeviceQueryHelper.Object);

      _device.Setup(e => e.ID).Returns(55);
      var factory = loc.GetLocationUpdateRequestedEvent(_device.Object);
      _mockDeviceQueryHelper.Verify(e => e.GetDeviceType(It.IsAny<IDeviceQuery>(), It.IsAny<IStorage>()), Times.Once());
      Assert.AreEqual(typeof(TestClass).AssemblyQualifiedName, factory.AssemblyQualifiedName);
    }

    [TestMethod]
    public void GetLocationUpdateRequestedEvent_UnknownDeviceHandler_Returned()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();
      Mock<IDeviceQueryHelper> _mockDeviceQueryHelper = new Mock<IDeviceQueryHelper>();
      Mock<IDeviceHandlerParameters> _mockParams = new Mock<IDeviceHandlerParameters>();
      _mockParams.Setup(o => o.UnknownDeviceHandler).Returns(new UnknownDeviceHandler());
      Mock<IDeviceHandlerStrategy> _strategy = new Mock<IDeviceHandlerStrategy>();
      _strategy.Setup(e => e.LocationUpdateRequestedEventType).Returns(typeof(TestClass));

      Mock<IDeviceQuery> _device = new Mock<IDeviceQuery>();

      _mockDeviceQueryHelper.Setup(o => o.GetDeviceType(It.IsAny<IDeviceQuery>(), _mockStorage.Object)).Returns(DeviceTypeEnum.Series522);
      _mockParams.Setup(o => o.DeviceHandlers.ContainsKey(It.IsAny<DeviceTypeEnum>())).Returns(false);
      _mockParams.Setup(o => o.DeviceHandlers[It.IsAny<DeviceTypeEnum>()]).Returns((IDeviceHandlerStrategy)null);
      LocationUpdateRequestedProcessor loc = new LocationUpdateRequestedProcessor(_mockStorage.Object, _mockParams.Object, _mockDeviceQueryHelper.Object);
      _device.Setup(e => e.ID).Returns(55);
      try
      {
        var factory = loc.GetLocationUpdateRequestedEvent(_device.Object);
        Assert.Fail("Should have thrown UnknownDeviceException");
      }
      catch (Exception ex)
      {
        Assert.AreEqual(typeof(UnknownDeviceException), ex.GetType());
        _mockDeviceQueryHelper.Verify(e => e.GetDeviceType(It.IsAny<IDeviceQuery>(), It.IsAny<IStorage>()), Times.Once());
      }
    }

    [TestMethod]
    public void GetLocationUpdateRequestedEvent_NullDeviceTypeUnknownDeviceHandler_Returned()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();
      Mock<IDeviceQueryHelper> _mockDeviceQueryHelper = new Mock<IDeviceQueryHelper>();
      Mock<IDeviceHandlerParameters> _mockParams = new Mock<IDeviceHandlerParameters>();
      _mockParams.Setup(o => o.UnknownDeviceHandler).Returns(new UnknownDeviceHandler());
      Mock<IDeviceHandlerStrategy> _strategy = new Mock<IDeviceHandlerStrategy>();
      _strategy.Setup(e => e.LocationUpdateRequestedEventType).Returns(typeof(TestClass));

      Mock<IDeviceQuery> _device = new Mock<IDeviceQuery>();

      _mockDeviceQueryHelper.Setup(o => o.GetDeviceType(It.IsAny<IDeviceQuery>(), _mockStorage.Object)).Returns((DeviceTypeEnum?)null);
      _mockParams.Setup(o => o.DeviceHandlers.ContainsKey(It.IsAny<DeviceTypeEnum>())).Returns(false);
      _mockParams.Setup(o => o.DeviceHandlers[It.IsAny<DeviceTypeEnum>()]).Returns((IDeviceHandlerStrategy)null);
      LocationUpdateRequestedProcessor loc = new LocationUpdateRequestedProcessor(_mockStorage.Object, _mockParams.Object, _mockDeviceQueryHelper.Object);
      _device.Setup(e => e.ID).Returns(55);
      try
      {
        var factory = loc.GetLocationUpdateRequestedEvent(_device.Object);
        Assert.Fail("Should have thrown UnknownDeviceException");
      }
      catch (Exception ex)
      {
        Assert.AreEqual(typeof(UnknownDeviceException), ex.GetType());
        _mockDeviceQueryHelper.Verify(e => e.GetDeviceType(It.IsAny<IDeviceQuery>(), It.IsAny<IStorage>()), Times.Once());
      }
    }

    public class TestClass
    {

    }

    private IEnumerable<EndpointDescriptor> GetTestEndpointDescriptors()
    {
      yield return
        new EndpointDescriptor
        {
          ContentType = "application/xml",
          Id = 1,
          Name = "endpoint",
          EncryptedPwd = "pwd",
          Url = "http://www.url.com",
          Username = "username"
        };
    }
  }
}
