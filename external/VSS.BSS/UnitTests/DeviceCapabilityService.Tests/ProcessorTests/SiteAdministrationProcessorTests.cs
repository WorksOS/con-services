using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
//using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.Nighthawk.DeviceCapabilityService.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.Processors;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.ProcessorTests
{
  [TestClass]
  public class SiteAdministrationProcessorTests
  {
    [TestMethod]
    public void Test_ForA5N2_Success()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();
      Mock<IDeviceHandlerParameters> _mockDeviceHandlerParameters = new Mock<IDeviceHandlerParameters>();
      Mock<IDeviceQueryHelper> _mockDeviceQueryHelper = new Mock<IDeviceQueryHelper>();

      IEnumerable<EndpointDescriptor> expectedEndpointDescriptors = GetTestEndpointDescriptors();
      _mockDeviceQueryHelper.Setup(o => o.GetDeviceType(It.IsAny<IDeviceQuery>(), _mockStorage.Object)).Returns(DeviceTypeEnum.PL631);
      _mockStorage.Setup(o => o.GetEndpointDescriptorsForNames(It.IsAny<IEnumerable<string>>()))
        .Returns(expectedEndpointDescriptors);
      _mockDeviceHandlerParameters.SetupGet(o => o.DeviceHandlers).Returns(GetA5N2TestDeviceHandlers());
      _mockDeviceHandlerParameters.SetupGet(o => o.UnknownDeviceHandler).Returns(new UnknownDeviceHandler());
      SiteAdministrationProcessor processor = new SiteAdministrationProcessor(_mockStorage.Object, _mockDeviceHandlerParameters.Object, _mockDeviceQueryHelper.Object);

      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);

      IFactoryOutboundEventTypeDescriptor factoryTypeDescriptor = processor.GetSiteDispatchedEvent(device.Object);
      Assert.IsTrue(factoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.ISiteDispatchedEvent"));
      Assert.AreEqual(expectedEndpointDescriptors.First().ContentType, factoryTypeDescriptor.Destinations.First().ContentType);
      Assert.AreEqual(expectedEndpointDescriptors.First().Id, factoryTypeDescriptor.Destinations.First().Id);
      Assert.AreEqual(expectedEndpointDescriptors.First().Name, factoryTypeDescriptor.Destinations.First().Name);
      Assert.AreEqual(expectedEndpointDescriptors.First().EncryptedPwd, factoryTypeDescriptor.Destinations.First().EncryptedPwd);
      Assert.AreEqual(expectedEndpointDescriptors.First().Url, factoryTypeDescriptor.Destinations.First().Url);
      Assert.AreEqual(expectedEndpointDescriptors.First().Username, factoryTypeDescriptor.Destinations.First().Username);

      factoryTypeDescriptor = processor.GetSiteRemovedEvent(device.Object);
      Assert.IsTrue(factoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.ISiteRemovedEvent"));
      Assert.AreEqual(expectedEndpointDescriptors.First().ContentType, factoryTypeDescriptor.Destinations.First().ContentType);
      Assert.AreEqual(expectedEndpointDescriptors.First().Id, factoryTypeDescriptor.Destinations.First().Id);
      Assert.AreEqual(expectedEndpointDescriptors.First().Name, factoryTypeDescriptor.Destinations.First().Name);
      Assert.AreEqual(expectedEndpointDescriptors.First().EncryptedPwd, factoryTypeDescriptor.Destinations.First().EncryptedPwd);
      Assert.AreEqual(expectedEndpointDescriptors.First().Url, factoryTypeDescriptor.Destinations.First().Url);
      Assert.AreEqual(expectedEndpointDescriptors.First().Username, factoryTypeDescriptor.Destinations.First().Username);
    }

    [TestMethod]
    public void Test_ForUnsupportedDevice_ThrowsException()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();
      Mock<IDeviceHandlerParameters> _mockDeviceHandlerParameters = new Mock<IDeviceHandlerParameters>();
      Mock<IDeviceQueryHelper> _mockDeviceQueryHelper = new Mock<IDeviceQueryHelper>();

      IEnumerable<EndpointDescriptor> expectedEndpointDescriptors = GetTestEndpointDescriptors();
      _mockDeviceQueryHelper.Setup(o => o.GetDeviceType(It.IsAny<IDeviceQuery>(), _mockStorage.Object)).Returns(DeviceTypeEnum.TrimTrac);
      _mockStorage.Setup(o => o.GetEndpointDescriptorsForNames(It.IsAny<IEnumerable<string>>()))
        .Returns(expectedEndpointDescriptors);
      _mockDeviceHandlerParameters.SetupGet(o => o.DeviceHandlers).Returns(GetUnsupportedTestDeviceHandlers());
      _mockDeviceHandlerParameters.SetupGet(o => o.UnknownDeviceHandler).Returns(new UnknownDeviceHandler());
      SiteAdministrationProcessor processor = new SiteAdministrationProcessor(_mockStorage.Object, _mockDeviceHandlerParameters.Object, _mockDeviceQueryHelper.Object);

      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);
      try
      {
        processor.GetSiteDispatchedEvent(device.Object);
        Assert.Fail("Expected NotImplementedException");
      }
      catch (NotImplementedException)
      {
      }
      try
      {
        processor.GetSiteRemovedEvent(device.Object);
        Assert.Fail("Expected NotImplementedException");
      }
      catch (NotImplementedException)
      {
      }
    }


    [TestMethod]
    public void Test_ForNullDevice()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();
      Mock<IDeviceHandlerParameters> _mockDeviceHandlerParameters = new Mock<IDeviceHandlerParameters>();
      Mock<IDeviceQueryHelper> _mockDeviceQueryHelper = new Mock<IDeviceQueryHelper>();

      IEnumerable<EndpointDescriptor> expectedEndpointDescriptors = GetTestEndpointDescriptors();
      _mockDeviceQueryHelper.Setup(o => o.GetDeviceType(It.IsAny<IDeviceQuery>(), _mockStorage.Object)).Returns((DeviceTypeEnum?)null);
      _mockStorage.Setup(o => o.GetEndpointDescriptorsForNames(It.IsAny<IEnumerable<string>>()))
        .Returns(expectedEndpointDescriptors);
      _mockDeviceHandlerParameters.SetupGet(o => o.DeviceHandlers).Returns(GetUnsupportedTestDeviceHandlers());
      _mockDeviceHandlerParameters.SetupGet(o => o.UnknownDeviceHandler).Returns(new UnknownDeviceHandler());
      SiteAdministrationProcessor processor = new SiteAdministrationProcessor(_mockStorage.Object, _mockDeviceHandlerParameters.Object, _mockDeviceQueryHelper.Object);

      var device = new Mock<IDeviceQuery>();
      try
      {
        processor.GetSiteDispatchedEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetSiteRemovedEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
    }

    [TestMethod]
    public void Test_ForUnknownDevice_ThrowsException()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();
      Mock<IDeviceHandlerParameters> _mockDeviceHandlerParameters = new Mock<IDeviceHandlerParameters>();
      Mock<IDeviceQueryHelper> _mockDeviceQueryHelper = new Mock<IDeviceQueryHelper>();

      _mockDeviceQueryHelper.Setup(o => o.GetDeviceType(It.IsAny<IDeviceQuery>(), _mockStorage.Object)).Returns(DeviceTypeEnum.TAP66);
      _mockStorage.Setup(o => o.GetEndpointDescriptorsForNames(It.IsAny<IEnumerable<string>>()))
        .Returns(new List<EndpointDescriptor>());
      _mockDeviceHandlerParameters.SetupGet(o => o.DeviceHandlers)
        .Returns(new Dictionary<DeviceTypeEnum, IDeviceHandlerStrategy>());
      _mockDeviceHandlerParameters.SetupGet(o => o.UnknownDeviceHandler).Returns(new UnknownDeviceHandler());
      SiteAdministrationProcessor processor = new SiteAdministrationProcessor(_mockStorage.Object, _mockDeviceHandlerParameters.Object, _mockDeviceQueryHelper.Object);
      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);
      try
      {
        processor.GetSiteDispatchedEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetSiteRemovedEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
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

    private IDictionary<DeviceTypeEnum, IDeviceHandlerStrategy> GetA5N2TestDeviceHandlers()
    {
      IDictionary<DeviceTypeEnum, IDeviceHandlerStrategy> handlers = new Dictionary<DeviceTypeEnum, IDeviceHandlerStrategy>();
      handlers.Add(DeviceTypeEnum.PL631, new PL631DeviceHandler(Helpers.GetTestEndpointNames().Where(o => o.Contains("CAT"))));
      return handlers;
    }

    private IDictionary<DeviceTypeEnum, IDeviceHandlerStrategy> GetUnsupportedTestDeviceHandlers()
    {
      IDictionary<DeviceTypeEnum, IDeviceHandlerStrategy> handlers = new Dictionary<DeviceTypeEnum, IDeviceHandlerStrategy>();
      handlers.Add(DeviceTypeEnum.TrimTrac, new TrimTracDeviceHandler(null));
      return handlers;
    }
  }
}
