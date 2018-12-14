using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Log4Net.Extensions;

namespace VSS.ConfigurationStore.UnitTests
{
  [TestClass]
  public class ConfigurationStoreTests
  {
    public IServiceProvider ServiceProvider;

    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();

      string loggerRepoName = "UnitTestLogTest";
      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(loggerRepoName, "log4nettest.xml");

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();
      ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    [TestMethod]
    public void CanCreateConfigStore()
    {
      Assert.IsNotNull(ServiceProvider.GetRequiredService<IConfigurationStore>());
      Assert.IsInstanceOfType(ServiceProvider.GetRequiredService<IConfigurationStore>(), typeof(GenericConfiguration));
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
      Assert.ThrowsException<InvalidOperationException>(() => configuration.GetConnectionString("VSPDB2"));
    }

    [TestMethod]
    public void CanGetString()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.IsFalse(String.IsNullOrEmpty(configuration.GetValueString("KAFKA_TOPIC_NAME_SUFFIX")));
    }

    [TestMethod]
    public void CanGetDefaultString()
    {
      string defaultValue = "gotThisInstead";
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.AreEqual(defaultValue, configuration.GetValueString("UNKNOWN_KEY", defaultValue));
    }

    [TestMethod]
    public void CanGetInt()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.AreEqual(1, configuration.GetValueInt("KAFKA_STACKSIZE"));
    }

    [TestMethod]
    public void CanGetDefaultInt()
    {
      int defaultValue = 9988;
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.AreEqual(defaultValue, configuration.GetValueInt("UNKNOWN_KEY", defaultValue));
    }

    [TestMethod]
    public void ReturnsNegativeWhenInvalidInteger()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.AreEqual(-2147483648, configuration.GetValueInt("KAFKA_PORT"));
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
    public void CanGetDefaultBool()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var value = configuration.GetValueBool("UNKNOWN_KEY", false);
      Assert.AreEqual(false, value);
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
    public void CanGetDefaultTimeSpan()
    {
      var defaultValue = new TimeSpan(5, 4, 3, 2);
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var value = configuration.GetValueTimeSpan("UNKNOWN_KEY", defaultValue);
      Assert.AreEqual(defaultValue, value);
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
