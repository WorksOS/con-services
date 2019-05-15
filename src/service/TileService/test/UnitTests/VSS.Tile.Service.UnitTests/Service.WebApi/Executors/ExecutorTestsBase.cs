using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.DataOcean.Client;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.WebApi.Common;

namespace VSS.Tile.Service.UnitTests.Service.WebApi.Executors
{
  public class ExecutorTestsBase
  {
    private const string loggerRepoName = "UnitTestLogTest";

    public static IServiceProvider ServiceProvider;

    protected IDictionary<string, string> CustomHeaders = new Dictionary<string, string>();

    protected ITPaaSApplicationAuthentication authn;
    protected IConfigurationStore Config;
    protected IDataOceanClient DataOceanClient;
    protected ILoggerFactory Logger;

    public ExecutorTestsBase()
    {
      var serviceCollection = new ServiceCollection();

      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(loggerRepoName, "log4nettest.xml");
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, ProjectErrorCodesProvider>()
      //  .AddTransient<IDataOceanClient, DataOceanClient>()
        .AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>();

      ServiceProvider = serviceCollection.BuildServiceProvider();
      Config = ServiceProvider.GetRequiredService<IConfigurationStore>();
 //     DataOceanClient = ServiceProvider.GetRequiredService<IDataOceanClient>();
      Logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
    }
  }
}
