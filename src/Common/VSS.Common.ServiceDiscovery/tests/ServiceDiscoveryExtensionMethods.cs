using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.ServiceDiscovery.Resolvers;
using VSS.Common.ServiceDiscovery.UnitTests.Mocks;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;

namespace VSS.Common.ServiceDiscovery.UnitTests
{
  [TestClass]
  public class ServiceDiscoveryExtensionMethods
  {
    private IServiceCollection serviceCollection;

    private MockConfiguration mockConfiguration = new MockConfiguration();

    [TestInitialize]
    public void InitTest()
    {
      serviceCollection = new ServiceCollection();

      string loggerRepoName = "UnitTestLogTest";
      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(loggerRepoName, "log4nettest.xml");

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug().AddConsole();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore>(mockConfiguration);
    }

    [TestMethod]
    public void TestKubernetesIsntAdded()
    {
      serviceCollection.AddServiceDiscovery(false);

      var resolvers = serviceCollection.BuildServiceProvider().GetServices<IServiceResolver>();
      foreach (var serviceResolver in resolvers)
      {
        Assert.IsNotInstanceOfType(serviceResolver, typeof(KubernetesServiceResolver));
      }
    }

    [TestMethod]
    public void TestKubernetesIsAdded()
    {
      serviceCollection.AddServiceDiscovery(true);

      var found = false;
      var resolvers = serviceCollection.BuildServiceProvider().GetServices<IServiceResolver>();
      foreach (var serviceResolver in resolvers)
      {
        if (serviceResolver is KubernetesServiceResolver)
        {
          found = true;
          break;
        }
      }

      Assert.IsTrue(found, $"Count not find {nameof(KubernetesServiceResolver)} in Resolvers");
    }
  }
}
