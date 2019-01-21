using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TCCFileAccess;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ExecutorBaseTests
  {
    protected IConfigurationStore configStore;

    public IServiceProvider serviceProvider = null;
    protected static ContractExecutionStatesEnum contractExecutionStatesEnum = new ContractExecutionStatesEnum();
    private readonly string loggerRepoName = "UnitTestLogTest";

    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();
      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(loggerRepoName, "log4nettest.xml");

      serviceCollection.AddLogging();
      serviceCollection
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddSingleton<IFileRepository, FileRepository>();

      serviceProvider = serviceCollection.BuildServiceProvider();
      configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
    }
  }
}
