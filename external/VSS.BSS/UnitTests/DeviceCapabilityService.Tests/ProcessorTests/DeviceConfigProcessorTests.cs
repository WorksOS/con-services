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
  public class DeviceConfigProcessorTests
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
      DeviceConfigProcessor processor = new DeviceConfigProcessor(_mockStorage.Object, _mockDeviceHandlerParameters.Object, _mockDeviceQueryHelper.Object);

      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);

      IFactoryOutboundEventTypeDescriptor factoryTypeDescriptor = processor.GetDigitalSwitchConfigurationEvent(device.Object);
      Assert.IsTrue(factoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IDigitalSwitchConfigurationEvent"));
      Assert.AreEqual(expectedEndpointDescriptors.First().ContentType, factoryTypeDescriptor.Destinations.First().ContentType);
      Assert.AreEqual(expectedEndpointDescriptors.First().Id, factoryTypeDescriptor.Destinations.First().Id);
      Assert.AreEqual(expectedEndpointDescriptors.First().Name, factoryTypeDescriptor.Destinations.First().Name);
      Assert.AreEqual(expectedEndpointDescriptors.First().EncryptedPwd, factoryTypeDescriptor.Destinations.First().EncryptedPwd);
      Assert.AreEqual(expectedEndpointDescriptors.First().Url, factoryTypeDescriptor.Destinations.First().Url);
      Assert.AreEqual(expectedEndpointDescriptors.First().Username, factoryTypeDescriptor.Destinations.First().Username);

      factoryTypeDescriptor = processor.GetDisableMaintenanceModeEvent(device.Object);
      Assert.IsTrue(factoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IDisableMaintenanceModeEvent"));
      Assert.AreEqual(expectedEndpointDescriptors.First().ContentType, factoryTypeDescriptor.Destinations.First().ContentType);
      Assert.AreEqual(expectedEndpointDescriptors.First().Id, factoryTypeDescriptor.Destinations.First().Id);
      Assert.AreEqual(expectedEndpointDescriptors.First().Name, factoryTypeDescriptor.Destinations.First().Name);
      Assert.AreEqual(expectedEndpointDescriptors.First().EncryptedPwd, factoryTypeDescriptor.Destinations.First().EncryptedPwd);
      Assert.AreEqual(expectedEndpointDescriptors.First().Url, factoryTypeDescriptor.Destinations.First().Url);
      Assert.AreEqual(expectedEndpointDescriptors.First().Username, factoryTypeDescriptor.Destinations.First().Username);

      factoryTypeDescriptor = processor.GetDiscreteInputConfigurationEvent(device.Object);
      Assert.IsTrue(factoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IDiscreteInputConfigurationEvent"));
      Assert.AreEqual(expectedEndpointDescriptors.First().ContentType, factoryTypeDescriptor.Destinations.First().ContentType);
      Assert.AreEqual(expectedEndpointDescriptors.First().Id, factoryTypeDescriptor.Destinations.First().Id);
      Assert.AreEqual(expectedEndpointDescriptors.First().Name, factoryTypeDescriptor.Destinations.First().Name);
      Assert.AreEqual(expectedEndpointDescriptors.First().EncryptedPwd, factoryTypeDescriptor.Destinations.First().EncryptedPwd);
      Assert.AreEqual(expectedEndpointDescriptors.First().Url, factoryTypeDescriptor.Destinations.First().Url);
      Assert.AreEqual(expectedEndpointDescriptors.First().Username, factoryTypeDescriptor.Destinations.First().Username);

      factoryTypeDescriptor = processor.GetEnableMaintenanceModeEvent(device.Object);
      Assert.IsTrue(factoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEnableMaintenanceModeEvent"));
      Assert.AreEqual(expectedEndpointDescriptors.First().ContentType, factoryTypeDescriptor.Destinations.First().ContentType);
      Assert.AreEqual(expectedEndpointDescriptors.First().Id, factoryTypeDescriptor.Destinations.First().Id);
      Assert.AreEqual(expectedEndpointDescriptors.First().Name, factoryTypeDescriptor.Destinations.First().Name);
      Assert.AreEqual(expectedEndpointDescriptors.First().EncryptedPwd, factoryTypeDescriptor.Destinations.First().EncryptedPwd);
      Assert.AreEqual(expectedEndpointDescriptors.First().Url, factoryTypeDescriptor.Destinations.First().Url);
      Assert.AreEqual(expectedEndpointDescriptors.First().Username, factoryTypeDescriptor.Destinations.First().Username);

      factoryTypeDescriptor = processor.GetFirstDailyReportStartTimeUtcChangedEvent(device.Object);
      Assert.IsTrue(factoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IFirstDailyReportStartTimeUtcChangedEvent"));
      Assert.AreEqual(expectedEndpointDescriptors.First().ContentType, factoryTypeDescriptor.Destinations.First().ContentType);
      Assert.AreEqual(expectedEndpointDescriptors.First().Id, factoryTypeDescriptor.Destinations.First().Id);
      Assert.AreEqual(expectedEndpointDescriptors.First().Name, factoryTypeDescriptor.Destinations.First().Name);
      Assert.AreEqual(expectedEndpointDescriptors.First().EncryptedPwd, factoryTypeDescriptor.Destinations.First().EncryptedPwd);
      Assert.AreEqual(expectedEndpointDescriptors.First().Url, factoryTypeDescriptor.Destinations.First().Url);
      Assert.AreEqual(expectedEndpointDescriptors.First().Username, factoryTypeDescriptor.Destinations.First().Username);

      factoryTypeDescriptor = processor.GetHourMeterModifiedEvent(device.Object);
      Assert.IsTrue(factoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IHourMeterModifiedEvent"));
      Assert.AreEqual(expectedEndpointDescriptors.First().ContentType, factoryTypeDescriptor.Destinations.First().ContentType);
      Assert.AreEqual(expectedEndpointDescriptors.First().Id, factoryTypeDescriptor.Destinations.First().Id);
      Assert.AreEqual(expectedEndpointDescriptors.First().Name, factoryTypeDescriptor.Destinations.First().Name);
      Assert.AreEqual(expectedEndpointDescriptors.First().EncryptedPwd, factoryTypeDescriptor.Destinations.First().EncryptedPwd);
      Assert.AreEqual(expectedEndpointDescriptors.First().Url, factoryTypeDescriptor.Destinations.First().Url);
      Assert.AreEqual(expectedEndpointDescriptors.First().Username, factoryTypeDescriptor.Destinations.First().Username);

      factoryTypeDescriptor = processor.GetMovingCriteriaConfigurationChangedEvent(device.Object);
      Assert.IsTrue(factoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IMovingCriteriaConfigurationChangedEvent"));
      Assert.AreEqual(expectedEndpointDescriptors.First().ContentType, factoryTypeDescriptor.Destinations.First().ContentType);
      Assert.AreEqual(expectedEndpointDescriptors.First().Id, factoryTypeDescriptor.Destinations.First().Id);
      Assert.AreEqual(expectedEndpointDescriptors.First().Name, factoryTypeDescriptor.Destinations.First().Name);
      Assert.AreEqual(expectedEndpointDescriptors.First().EncryptedPwd, factoryTypeDescriptor.Destinations.First().EncryptedPwd);
      Assert.AreEqual(expectedEndpointDescriptors.First().Url, factoryTypeDescriptor.Destinations.First().Url);
      Assert.AreEqual(expectedEndpointDescriptors.First().Username, factoryTypeDescriptor.Destinations.First().Username);

      factoryTypeDescriptor = processor.GetOdometerModifiedEvent(device.Object);
      Assert.IsTrue(factoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IOdometerModifiedEvent"));
      Assert.AreEqual(expectedEndpointDescriptors.First().ContentType, factoryTypeDescriptor.Destinations.First().ContentType);
      Assert.AreEqual(expectedEndpointDescriptors.First().Id, factoryTypeDescriptor.Destinations.First().Id);
      Assert.AreEqual(expectedEndpointDescriptors.First().Name, factoryTypeDescriptor.Destinations.First().Name);
      Assert.AreEqual(expectedEndpointDescriptors.First().EncryptedPwd, factoryTypeDescriptor.Destinations.First().EncryptedPwd);
      Assert.AreEqual(expectedEndpointDescriptors.First().Url, factoryTypeDescriptor.Destinations.First().Url);
      Assert.AreEqual(expectedEndpointDescriptors.First().Username, factoryTypeDescriptor.Destinations.First().Username);

      factoryTypeDescriptor = processor.SetStartModeEvent(device.Object);
      Assert.IsTrue(factoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.ISetStartModeEvent"));
      Assert.AreEqual(expectedEndpointDescriptors.First().ContentType, factoryTypeDescriptor.Destinations.First().ContentType);
      Assert.AreEqual(expectedEndpointDescriptors.First().Id, factoryTypeDescriptor.Destinations.First().Id);
      Assert.AreEqual(expectedEndpointDescriptors.First().Name, factoryTypeDescriptor.Destinations.First().Name);
      Assert.AreEqual(expectedEndpointDescriptors.First().EncryptedPwd, factoryTypeDescriptor.Destinations.First().EncryptedPwd);
      Assert.AreEqual(expectedEndpointDescriptors.First().Url, factoryTypeDescriptor.Destinations.First().Url);
      Assert.AreEqual(expectedEndpointDescriptors.First().Username, factoryTypeDescriptor.Destinations.First().Username);

      factoryTypeDescriptor = processor.GetStartModeEvent(device.Object);
      Assert.IsTrue(factoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IGetStartModeEvent"));
      Assert.AreEqual(expectedEndpointDescriptors.First().ContentType, factoryTypeDescriptor.Destinations.First().ContentType);
      Assert.AreEqual(expectedEndpointDescriptors.First().Id, factoryTypeDescriptor.Destinations.First().Id);
      Assert.AreEqual(expectedEndpointDescriptors.First().Name, factoryTypeDescriptor.Destinations.First().Name);
      Assert.AreEqual(expectedEndpointDescriptors.First().EncryptedPwd, factoryTypeDescriptor.Destinations.First().EncryptedPwd);
      Assert.AreEqual(expectedEndpointDescriptors.First().Url, factoryTypeDescriptor.Destinations.First().Url);
      Assert.AreEqual(expectedEndpointDescriptors.First().Username, factoryTypeDescriptor.Destinations.First().Username);

      factoryTypeDescriptor = processor.SetTamperLevelEvent(device.Object);
      Assert.IsTrue(factoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.ISetTamperLevelEvent"));
      Assert.AreEqual(expectedEndpointDescriptors.First().ContentType, factoryTypeDescriptor.Destinations.First().ContentType);
      Assert.AreEqual(expectedEndpointDescriptors.First().Id, factoryTypeDescriptor.Destinations.First().Id);
      Assert.AreEqual(expectedEndpointDescriptors.First().Name, factoryTypeDescriptor.Destinations.First().Name);
      Assert.AreEqual(expectedEndpointDescriptors.First().EncryptedPwd, factoryTypeDescriptor.Destinations.First().EncryptedPwd);
      Assert.AreEqual(expectedEndpointDescriptors.First().Url, factoryTypeDescriptor.Destinations.First().Url);
      Assert.AreEqual(expectedEndpointDescriptors.First().Username, factoryTypeDescriptor.Destinations.First().Username);

      factoryTypeDescriptor = processor.GetTamperLevelEvent(device.Object);
      Assert.IsTrue(factoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IGetTamperLevelEvent"));
      Assert.AreEqual(expectedEndpointDescriptors.First().ContentType, factoryTypeDescriptor.Destinations.First().ContentType);
      Assert.AreEqual(expectedEndpointDescriptors.First().Id, factoryTypeDescriptor.Destinations.First().Id);
      Assert.AreEqual(expectedEndpointDescriptors.First().Name, factoryTypeDescriptor.Destinations.First().Name);
      Assert.AreEqual(expectedEndpointDescriptors.First().EncryptedPwd, factoryTypeDescriptor.Destinations.First().EncryptedPwd);
      Assert.AreEqual(expectedEndpointDescriptors.First().Url, factoryTypeDescriptor.Destinations.First().Url);
      Assert.AreEqual(expectedEndpointDescriptors.First().Username, factoryTypeDescriptor.Destinations.First().Username);
    }

    [TestMethod]
    public void Test_SetDailyReportFrequencyEvent_ForA5N2_Success()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();
      Mock<IDeviceHandlerParameters> _mockDeviceHandlerParameters = new Mock<IDeviceHandlerParameters>();
      Mock<IDeviceQueryHelper> _mockDeviceQueryHelper = new Mock<IDeviceQueryHelper>();

      IEnumerable<EndpointDescriptor> expectedEndpointDescriptors = GetTestEndpointDescriptors();
      _mockDeviceQueryHelper.Setup(o => o.GetDeviceType(It.IsAny<IDeviceQuery>(), _mockStorage.Object)).Returns(DeviceTypeEnum.PL141);
      _mockStorage.Setup(o => o.GetEndpointDescriptorsForNames(It.IsAny<IEnumerable<string>>()))
        .Returns(expectedEndpointDescriptors);
      _mockDeviceHandlerParameters.SetupGet(o => o.DeviceHandlers).Returns(GetA5N2TestDeviceHandlers());
      _mockDeviceHandlerParameters.SetupGet(o => o.UnknownDeviceHandler).Returns(new UnknownDeviceHandler());
      DeviceConfigProcessor processor = new DeviceConfigProcessor(_mockStorage.Object, _mockDeviceHandlerParameters.Object, _mockDeviceQueryHelper.Object);

      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);

      IFactoryOutboundEventTypeDescriptor factoryTypeDescriptor = processor.SetDailyReportFrequencyEvent(device.Object);
      Assert.IsTrue(factoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.ISetDailyReportFrequencyEvent"));
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
      DeviceConfigProcessor processor = new DeviceConfigProcessor(_mockStorage.Object, _mockDeviceHandlerParameters.Object, _mockDeviceQueryHelper.Object);

      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);
      try
      {
        processor.GetDigitalSwitchConfigurationEvent(device.Object);
        Assert.Fail("Expected NotImplementedException");
      }
      catch (NotImplementedException)
      {
      }
      try
      {
        processor.GetDisableMaintenanceModeEvent(device.Object);
        Assert.Fail("Expected NotImplementedException");
      }
      catch (NotImplementedException)
      {
      }
      try
      {
        processor.GetDiscreteInputConfigurationEvent(device.Object);
        Assert.Fail("Expected NotImplementedException");
      }
      catch (NotImplementedException)
      {
      }
      try
      {
        processor.GetEnableMaintenanceModeEvent(device.Object);
        Assert.Fail("Expected NotImplementedException");
      }
      catch (NotImplementedException)
      {
      }
      try
      {
        processor.GetFirstDailyReportStartTimeUtcChangedEvent(device.Object);
        Assert.Fail("Expected NotImplementedException");
      }
      catch (NotImplementedException)
      {
      }
      try
      {
        processor.GetHourMeterModifiedEvent(device.Object);
        Assert.Fail("Expected NotImplementedException");
      }
      catch (NotImplementedException)
      {
      }
      try
      {
        processor.GetMovingCriteriaConfigurationChangedEvent(device.Object);
        Assert.Fail("Expected NotImplementedException");
      }
      catch (NotImplementedException)
      {
      }
      try
      {
        processor.GetOdometerModifiedEvent(device.Object);
        Assert.Fail("Expected NotImplementedException");
      }
      catch (NotImplementedException)
      {
      }
      try
      {
        processor.GetStartModeEvent(device.Object);
        Assert.Fail("Expected NotImplementedException");
      }
      catch (NotImplementedException)
      {
      }
      try
      {
        processor.SetStartModeEvent(device.Object);
        Assert.Fail("Expected NotImplementedException");
      }
      catch (NotImplementedException)
      {
      }
      try
      {
        processor.GetTamperLevelEvent(device.Object);
        Assert.Fail("Expected NotImplementedException");
      }
      catch (NotImplementedException)
      {
      }
      try
      {
        processor.SetTamperLevelEvent(device.Object);
        Assert.Fail("Expected NotImplementedException");
      }
      catch (NotImplementedException)
      {
      }
      try
      {
        processor.SetDailyReportFrequencyEvent(device.Object);
        Assert.Fail("Expected NotImplementedException");
      }
      catch (NotImplementedException)
      {
      }

      try
      {
        processor.EnableRapidReportingEvent(device.Object);
        Assert.Fail("Expected NotImplementedException");
      }
      catch (NotImplementedException)
      {
      }

      try
      {
        processor.DisableRapidReportingEvent(device.Object);
        Assert.Fail("Expected NotImplementedException");
      }
      catch (NotImplementedException)
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
      DeviceConfigProcessor processor = new DeviceConfigProcessor(_mockStorage.Object, _mockDeviceHandlerParameters.Object, _mockDeviceQueryHelper.Object);
      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);
      try
      {
        processor.GetDigitalSwitchConfigurationEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetDisableMaintenanceModeEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetDiscreteInputConfigurationEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetEnableMaintenanceModeEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetFirstDailyReportStartTimeUtcChangedEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetHourMeterModifiedEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetMovingCriteriaConfigurationChangedEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetOdometerModifiedEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetStartModeEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.SetStartModeEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetTamperLevelEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.SetTamperLevelEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.SetDailyReportFrequencyEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }

      try
      {
        processor.EnableRapidReportingEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }

      try
      {
        processor.DisableRapidReportingEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
    }


    [TestMethod]
    public void Test_ForNullDevice_ThrowsException()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();
      Mock<IDeviceHandlerParameters> _mockDeviceHandlerParameters = new Mock<IDeviceHandlerParameters>();
      Mock<IDeviceQueryHelper> _mockDeviceQueryHelper = new Mock<IDeviceQueryHelper>();

      _mockDeviceQueryHelper.Setup(o => o.GetDeviceType(It.IsAny<IDeviceQuery>(), _mockStorage.Object)).Returns((DeviceTypeEnum?)null);
      _mockStorage.Setup(o => o.GetEndpointDescriptorsForNames(It.IsAny<IEnumerable<string>>()))
        .Returns(new List<EndpointDescriptor>());
      _mockDeviceHandlerParameters.SetupGet(o => o.DeviceHandlers)
        .Returns(new Dictionary<DeviceTypeEnum, IDeviceHandlerStrategy>());
      _mockDeviceHandlerParameters.SetupGet(o => o.UnknownDeviceHandler).Returns(new UnknownDeviceHandler());
      DeviceConfigProcessor processor = new DeviceConfigProcessor(_mockStorage.Object, _mockDeviceHandlerParameters.Object, _mockDeviceQueryHelper.Object);
      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);
      try
      {
        processor.GetDigitalSwitchConfigurationEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetDisableMaintenanceModeEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetDiscreteInputConfigurationEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetEnableMaintenanceModeEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetFirstDailyReportStartTimeUtcChangedEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetHourMeterModifiedEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetMovingCriteriaConfigurationChangedEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetOdometerModifiedEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetStartModeEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.SetStartModeEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.GetTamperLevelEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.SetTamperLevelEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.SetDailyReportFrequencyEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.EnableRapidReportingEvent(device.Object);
        Assert.Fail("Expected UnknownDeviceException");
      }
      catch (UnknownDeviceException)
      {
      }
      try
      {
        processor.DisableRapidReportingEvent(device.Object);
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
      handlers.Add(DeviceTypeEnum.PL141, new PL141DeviceHandler(Helpers.GetTestEndpointNames().Where(o => o.Contains("CAT"))));
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
