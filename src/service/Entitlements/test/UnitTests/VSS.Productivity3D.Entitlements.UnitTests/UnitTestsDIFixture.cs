using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Entitlements.Abstractions.Interfaces;
using VSS.Serilog.Extensions;
using VSS.WebApi.Common;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace VSS.Productivity3D.Entitlements.UnitTests
{
  public class UnitTestsDIFixture<T> : IDisposable
  {
    public IServiceProvider ServiceProvider;
    protected Mock<IConfigurationStore> mockConfigStore = new Mock<IConfigurationStore>();
    protected Mock<IEmsClient> mockEmsClient = new Mock<IEmsClient>();
    protected Mock<ITPaaSApplicationAuthentication> mockAuthn = new Mock<ITPaaSApplicationAuthentication>();

    public UnitTestsDIFixture()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Productivity3D.Entitlements.UnitTests.log"));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddSingleton(mockConfigStore.Object)
        .AddSingleton(mockEmsClient.Object)
        .AddSingleton(mockAuthn.Object);

      ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    public void Dispose()
    { }
  }
}
