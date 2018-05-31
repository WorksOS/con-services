using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class ExecutorBaseTests
  {
    public IServiceProvider serviceProvider;
    public IServiceExceptionHandler serviceExceptionHandler;
    private readonly string loggerRepoName = "UnitTestLogTest";

    [TestInitialize]
    public virtual void InitTest()
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
        .AddTransient<ICustomerProxy, CustomerProxy>()
        .AddTransient<IProjectListProxy, ProjectListProxy>()
        .AddTransient<IRaptorProxy, RaptorProxy>()
        .AddTransient<IErrorCodesProvider, FilterErrorCodesProvider>()
        .AddTransient<IRepository<IFilterEvent>, FilterRepository>();

      serviceProvider = serviceCollection.BuildServiceProvider();

      this.serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
    }
  }
}