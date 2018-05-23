using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Log4Net.Extensions;

namespace VSS.ConfigurationStore.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  [TestClass]
  public class ConfigurationStoreTests
  {

    public IServiceProvider ServiceProvider;


    /// <summary>
    /// Initializes the test.
    /// </summary>
    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();

      string loggerRepoName = "UnitTestLogTest";
      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(loggerRepoName, "log4nettest.xml");

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();
      ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    [TestMethod]
    public void CanCreateConfigStore()
    {
      Assert.IsNotNull(ServiceProvider.GetRequiredService<IConfigurationStore>());
      Assert.IsInstanceOfType(ServiceProvider.GetRequiredService<IConfigurationStore>(),typeof(GenericConfiguration));
    }


    [TestMethod]
    public void CanGetConnectionString()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.IsFalse(String.IsNullOrEmpty(configuration.GetConnectionString("VSPDB")));
    }


    [TestMethod]
    public void ThrowsWhenInvalidConnectionName()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.ThrowsException<InvalidOperationException>(()=>configuration.GetConnectionString("VSPDB2"));
    }

    [TestMethod]
    public void CanGetString()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.IsFalse(String.IsNullOrEmpty(configuration.GetValueString("KAFKA_TOPIC_NAME_SUFFIX")));
    }

    [TestMethod]
    public void CanGetInt()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.AreEqual(1,configuration.GetValueInt("KAFKA_STACKSIZE"));
    }

    [TestMethod]
    public void ReturnsNegativeWhenInvalidInteger()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.AreEqual(-1,configuration.GetValueInt("KAFKA_PORT"));
    }

    [TestMethod]
    public void ReturnsNullWhenInvalidBool()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.IsNull(configuration.GetValueBool("KAFKA_PORT"));
    }

    [TestMethod]
    public void CanGetBool()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var value = configuration.GetValueBool("KAFKA_AUTO_COMMIT");
      Assert.IsTrue(value.HasValue);
      Assert.IsFalse(value.Value);
    }

    [TestMethod]
    public void CanGetTimeSpan()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var value = configuration.GetValueTimeSpan("AWS_PRESIGNED_URL_EXPIRY");
      Assert.IsTrue(value.HasValue);
      Assert.AreEqual(TimeSpan.FromDays(7), value.Value);
    }


    [TestMethod]
    public void CanGetLoggingConfig()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var section = configuration.GetLoggingConfig();
      Assert.IsNotNull(section);
    }
  }
}
