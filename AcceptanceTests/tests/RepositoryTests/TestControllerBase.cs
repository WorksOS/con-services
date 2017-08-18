using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;

namespace RepositoryTests
{
  public class TestControllerBase
  {
    protected IServiceProvider serviceProvider;
    protected IConfigurationStore configStore;
    //protected FilterRepository filterRepo;

    public void SetupDI()
    {
      const string loggerRepoName = "UnitTestLogTest";
      var logPath = Directory.GetCurrentDirectory();

      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(loggerFactory)
          .AddSingleton<IConfigurationStore, GenericConfiguration>()
          //.AddTransient<IRepository<IFilterEvent>, FilterRepository>()
          .AddMemoryCache() 
        .BuildServiceProvider();

      configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      //filterRepo = serviceProvider.GetRequiredService<IRepository<IFilterEvent>>() as FilterRepository;
      Assert.IsNotNull(serviceProvider.GetService<ILoggerFactory>());
  
    }
  }
}
 