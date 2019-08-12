using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.Serilog.Extensions;
using VSS.TCCFileAccess;

namespace WebApiTests.Executors
{
  public class ExecutorBaseTests : IDisposable
  {
    public IConfigurationStore configStore;
    public IServiceProvider serviceProvider;

    public ExecutorBaseTests()
    {
      serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.FileAccess.UnitTests.log")))
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddSingleton<IFileRepository, FileRepository>()
        .BuildServiceProvider();

      configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
    }

    public void Dispose()
    { }
  }
}
