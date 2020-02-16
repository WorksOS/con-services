using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.NHDataSvc.Consumers;
using System.Collections.Generic;
using VSS.Nighthawk.NHDataSvc.Interfaces.Events;
using MassTransit;

namespace NHDataSvc.Tests.Consumers
{
  [TestClass]
  public class RuntimeAdjustmentCreatedEventConsumerTests
  {
    [TestMethod]
    public void ContextIsNullSoNothingHappens()
    {
      Mock<INHDataProcessor> processor = new Mock<INHDataProcessor>();

      RuntimeAdjustmentCreatedEventConsumer consumer = new RuntimeAdjustmentCreatedEventConsumer(processor.Object);
      consumer.Consume(null);
      processor.Verify(e => e.Process(It.IsAny<List<NHDataWrapper>>()), Times.Never());
    }

    [TestMethod]
    public void MessageIsNullSoNothingHappens()
    {
      Mock<INHDataProcessor> processor = new Mock<INHDataProcessor>();

      var mockConsumeContext = new Mock<IConsumeContext<IRuntimeAdjustmentCreatedEvent>>();
      mockConsumeContext.SetupGet(o => o.Message).Returns((IRuntimeAdjustmentCreatedEvent)null);

      RuntimeAdjustmentCreatedEventConsumer consumer = new RuntimeAdjustmentCreatedEventConsumer(processor.Object);
      consumer.Consume(mockConsumeContext.Object);
      processor.Verify(e => e.Process(It.IsAny<List<NHDataWrapper>>()), Times.Never());
    }

    [TestMethod]
    public void WhenProcessorThrowsAnExceptionMessageisRetriedLater()
    {
      Mock<INHDataProcessor> processor = new Mock<INHDataProcessor>();
      processor.Setup(e => e.Process(It.IsAny<List<NHDataWrapper>>())).Throws(new Exception());

      var mockConsumeContext = new Mock<IConsumeContext<IRuntimeAdjustmentCreatedEvent>>();
      mockConsumeContext.SetupGet(o => o.Message).Returns(new RuntimeAdjustmentCreatedEvent());

      RuntimeAdjustmentCreatedEventConsumer consumer = new RuntimeAdjustmentCreatedEventConsumer(processor.Object);
      consumer.Consume(mockConsumeContext.Object);
      processor.Verify(e => e.Process(It.IsAny<List<NHDataWrapper>>()), Times.Once());
      mockConsumeContext.Verify(e => e.RetryLater(), Times.Once());
    }

    [TestMethod]
    public void ProcessorIsCalledWhenRuntimeAdjustmentCreatedEventIsInContext()
    {
      Mock<INHDataProcessor> processor = new Mock<INHDataProcessor>();

      var mockConsumeContext = new Mock<IConsumeContext<IRuntimeAdjustmentCreatedEvent>>();
      mockConsumeContext.SetupGet(o => o.Message).Returns(new RuntimeAdjustmentCreatedEvent());

      RuntimeAdjustmentCreatedEventConsumer consumer = new RuntimeAdjustmentCreatedEventConsumer(processor.Object);
      consumer.Consume(mockConsumeContext.Object);
      processor.Verify(e => e.Process(It.IsAny<List<NHDataWrapper>>()), Times.Once());
      mockConsumeContext.Verify(e => e.RetryLater(), Times.Never());
    }

    private class RuntimeAdjustmentCreatedEvent : IRuntimeAdjustmentCreatedEvent
    {
      public double RuntimeBeforeHours { get; set; }

      public double RuntimeAfterHours { get; set; }

      public string GpsDeviceID { get; set; }

      public DeviceTypeEnum DeviceType { get; set; }

      public long? AssetID { get; set; }

      public DateTime EventUTC { get; set; }

      public long SourceMsgID { get; set; }

      public long DebugRefID { get; set; }

      public DimSourceEnum Source { get; set; }

      public DateTime InsertUtc { get; set; }


      public long Id { get; set; }

      public double DeltaHours { get; set; }
    }

  }
}
