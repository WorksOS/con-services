using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CCSS.CWS.Client.UnitTests.Staging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.ConfigurationStore;
using VSS.Serilog.Extensions;
using VSS.WebApi.Common;
using ILogger = Serilog.ILogger;

namespace CCSS.CWS.Client.UnitTests
{
  [TestClass]
  public abstract class BaseTestClass
  {
    private IServiceCollection serviceCollection;

    protected IServiceProvider ServiceProvider { get; private set; }

    protected ILogger Log { get; set; }

    protected string userId;

    [TestInitialize]
    public async Task SetupServices()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure($"Tests::{ GetType().Name}.log"));
      serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();

      var provider = serviceCollection.BuildServiceProvider();
      var config = provider.GetService<IConfigurationStore>();
      SetupTestServices(serviceCollection, config);

      ServiceProvider = serviceCollection.BuildServiceProvider();

      var pretest = await PretestChecks();
      if (!pretest)
      {
        Assert.Inconclusive("Pretest checks for test failed");
      }
    }

    protected abstract IServiceCollection SetupTestServices(IServiceCollection services, IConfigurationStore configuration);

    protected virtual Task<bool> PretestChecks() => Task.FromResult(true);

    protected Dictionary<string, string> CustomHeaders() =>    
      new Dictionary<string, string>
      {
        {"Content-Type", ContentTypeConstants.ApplicationJson},
        {"Authorization", $"Bearer {ServiceProvider.GetService<ITPaaSApplicationAuthentication>().GetApplicationBearerToken()}"}
      };
  }
}
