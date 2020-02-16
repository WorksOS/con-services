using MassTransit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceBusExtensions = VSS.Nighthawk.DeviceCapabilityService.Interfaces.ServiceBusExtensions;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests
{
  [TestClass]
  public class ServiceBusExtensionsTests
  {
    #region Supporting Types
    public interface ITestBase {}
    public interface ITestFirstChild : ITestBase {}
    public interface ITestSecondChild : ITestFirstChild {}
    public class TestBaseImplemenation : ITestBase { }
    public class TestSecondChildImplementation : ITestSecondChild { }
    public class TestFirstChildImplementation : ITestFirstChild { }
    #endregion

    [TestMethod]
    public void PublishSpecificOfBaseImplementation_Success()
    {
      Mock<IServiceBus> mockServiceBus = new Mock<IServiceBus>();
      var testMessage = (ITestBase)new TestBaseImplemenation();

      ServiceBusExtensions.PublishSpecificOf(mockServiceBus.Object, testMessage);
      mockServiceBus.Verify(o => o.Publish(It.IsAny<ITestBase>(), typeof(ITestBase)), Times.Once());
    }

    [TestMethod]
    public void PublishSpecificOfDerivedImplmentationCorrect_Success()
    {
      Mock<IServiceBus> mockServiceBus = new Mock<IServiceBus>();
      var testMessage = (ITestBase)new TestSecondChildImplementation();

      ServiceBusExtensions.PublishSpecificOf(mockServiceBus.Object, testMessage);
      mockServiceBus.Verify(o => o.Publish(It.IsAny<ITestSecondChild>(), typeof(ITestSecondChild)), Times.Once());
    }

    [TestMethod]
    public void PublishSpecificOfDerivedImplmentationIncorrect_Success()
    {
      Mock<IServiceBus> mockServiceBus = new Mock<IServiceBus>();
      var testMessage = (ITestBase)new TestSecondChildImplementation();

      ServiceBusExtensions.PublishSpecificOf(mockServiceBus.Object, testMessage);
      mockServiceBus.Verify(o => o.Publish(It.IsAny<ITestSecondChild>(), typeof(ITestFirstChild)), Times.Never());
    }
  }
}
