using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
//using VSS.Hosted.VLCommon.Events;
using  VSS.BaseEvents;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Factories;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.InterfaceTests
{  
  /// <summary>
  /// Summary description for LocationStatusUpdateRequestedEventFactoryTests
  /// </summary>
  [TestClass]
  public class SiteAdministrationEventFactoryTests
  {
    [TestMethod]
    public void BuildSiteDispatchedEventForDevice_Success()
    {
      var mockEventTypeHelper = new Mock<IEventTypeHelper>();
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockEvent = new Mock<DataOut.Interfaces.Events.ISiteDispatchedEvent>();

      mockEventTypeHelper.Setup(o =>
                                o.QueryServiceForTypeAndBuildInstance<ISiteDispatchedEvent>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object)).Returns(mockEvent.Object);

      PrivateObject target = new PrivateObject(typeof(SiteAdministrationEventFactory));
      target.SetFieldOrProperty("_deviceCapabilitySvcUri", string.Empty);
      target.SetFieldOrProperty("_eventTypeHelper", mockEventTypeHelper.Object);

      ISiteDispatchedEvent actualEvent =
        (ISiteDispatchedEvent)target.Invoke("BuildSiteDispatchedEventForDevice", new object[] { mockDeviceQuery.Object });

      Assert.AreEqual(mockEvent.Object, actualEvent);
    }

    [TestMethod]
    public void BuildSiteRemovedEventForDevice_Success()
    {
      var mockEventTypeHelper = new Mock<IEventTypeHelper>();
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockEvent = new Mock<DataOut.Interfaces.Events.ISiteRemovedEvent>();

      mockEventTypeHelper.Setup(o =>
                                o.QueryServiceForTypeAndBuildInstance<ISiteRemovedEvent>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object)).Returns(mockEvent.Object);

      PrivateObject target = new PrivateObject(typeof(SiteAdministrationEventFactory));
      target.SetFieldOrProperty("_deviceCapabilitySvcUri", string.Empty);
      target.SetFieldOrProperty("_eventTypeHelper", mockEventTypeHelper.Object);

      ISiteRemovedEvent actualEvent =
        (ISiteRemovedEvent)target.Invoke("BuildSiteRemovedEventForDevice", new object[] { mockDeviceQuery.Object });

      Assert.AreEqual(mockEvent.Object, actualEvent);
    }
  }
}
