using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Serilog.Extensions;

namespace VSS.Productivity3D.Push.UnitTests
{
  public class NotificationCacheInvalidationFixture : IDisposable
  {
    public IServiceProvider ServiceProvider;
    public Mock<IDataCache> MockCache;

    public NotificationCacheInvalidationFixture()
    {
      MockCache = new Mock<IDataCache>();

      ServiceProvider = new ServiceCollection()
                        .AddLogging()
                        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Push.UnitTests.log")))
                        .AddSingleton(new Mock<IConfigurationStore>().Object)
                        .AddSingleton(new Mock<IServiceResolution>().Object)
                        .AddTransient<INotificationHubClient, NotificationHubClient>()
                        .AddSingleton(MockCache.Object)
                        .BuildServiceProvider();
    }

    public void Dispose()
    { }
  }
}
