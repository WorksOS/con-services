using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.Serilog.Extensions;

namespace RepositoryLandfillTests
{
  public class TestControllerBase
  {
    protected IServiceProvider ServiceProvider;
    

    public void SetupLogging()
    {
      ServiceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("RepositoryLandfill.Tests.log")))
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .BuildServiceProvider();

      Assert.IsNotNull(ServiceProvider.GetService<ILoggerFactory>());
    }
  }
}
