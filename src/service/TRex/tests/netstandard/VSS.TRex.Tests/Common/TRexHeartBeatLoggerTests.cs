using System;
using System.Threading;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Common
{
  public class TRexHeartBeatLoggerTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Creation_FailWithShortInterval()
    {
      Action act = () => new TRexHeartBeatLogger(10);
      act.Should().Throw< ArgumentException>().WithMessage("Heart beat logger interval cannot be < 100 milliseconds");
    }

    [Fact]
    public void Heartbeat()
    {
      var configMock = DIContext.Obtain<Mock<IConfigurationStore>>();
      configMock.Setup(x => x.GetValueInt("HEARTBEAT_LOGGER_INTERVAL")).Returns(100);
      configMock.Setup(x => x.GetValueInt("HEARTBEAT_LOGGER_INTERVAL", It.IsAny<int>())).Returns(100);
      DIBuilder.Continue().Add(x => x.AddSingleton(configMock.Object)).Complete();

      var logger = new TRexHeartBeatLogger();

      var testContext = new object();
      logger.AddContext(testContext);

      Thread.Sleep(200);

      logger.RemoveContext(testContext);
      logger.Stop();
    }
  }
}
