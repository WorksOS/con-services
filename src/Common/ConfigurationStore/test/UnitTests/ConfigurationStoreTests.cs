using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Serilog.Extensions;
using Xunit;

namespace VSS.ConfigurationStore.UnitTests
{
  public class ConfigurationStoreTests
  {
    public IServiceProvider ServiceProvider;

    public ConfigurationStoreTests()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.ConfigurationStore.UnitTests.log"));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();
      ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public void CanCreateConfigStore()
    {
      Assert.NotNull(ServiceProvider.GetRequiredService<IConfigurationStore>());
      Assert.IsType<GenericConfiguration>(ServiceProvider.GetRequiredService<IConfigurationStore>());
    }

    [Fact]
    public void CanGetConnectionString()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.False(string.IsNullOrEmpty(configuration.GetConnectionString("VSPDB")));
    }

    [Fact]
    public void ThrowsWhenInvalidConnectionName()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.Throws<InvalidOperationException>(() => configuration.GetConnectionString("VSPDB2"));
    }

    [Fact]
    public void CanGetString()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.False(string.IsNullOrEmpty(configuration.GetValueString("KAFKA_TOPIC_NAME_SUFFIX")));
    }

    [Fact]
    public void CanGetDefaultString()
    {
      var defaultValue = "gotThisInstead";
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.Equal(defaultValue, configuration.GetValueString("UNKNOWN_KEY", defaultValue));
    }

    [Fact]
    public void CanGetInt()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.Equal(1, configuration.GetValueInt("KAFKA_STACKSIZE"));
    }

    [Fact]
    public void CanGetDefaultInt()
    {
      var defaultValue = 9988;
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.Equal(defaultValue, configuration.GetValueInt("UNKNOWN_KEY", defaultValue));
    }

    [Fact]
    public void ReturnsNegativeWhenInvalidInteger()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.Equal(int.MinValue, configuration.GetValueInt("KAFKA_PORT"));
    }

    [Fact]
    public void ReturnsNullWhenInvalidBool()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Assert.Null(configuration.GetValueBool("KAFKA_PORT"));
    }

    [Fact]
    public void CanGetBool()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var value = configuration.GetValueBool("KAFKA_AUTO_COMMIT");
      Assert.True(value.HasValue);
      Assert.False(value.Value);
    }

    [Fact]
    public void CanGetDefaultBool()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var value = configuration.GetValueBool("UNKNOWN_KEY", false);
      Assert.False(value);
    }

    [Fact]
    public void CanGetTimeSpan()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var value = configuration.GetValueTimeSpan("AWS_PRESIGNED_URL_EXPIRY");
      Assert.True(value.HasValue);
      Assert.Equal(TimeSpan.FromDays(7), value.Value);
    }

    [Fact]
    public void CanGetDefaultTimeSpan()
    {
      var defaultValue = new TimeSpan(5, 4, 3, 2);
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var value = configuration.GetValueTimeSpan("UNKNOWN_KEY", defaultValue);
      Assert.Equal(defaultValue, value);
    }

    [Fact]
    public void CanGetGuid()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var value = configuration.GetValueGuid("PEGASUS_GEOTIFF_PROCEDURE_ID");
      Assert.Equal(new Guid("f61c965b-0828-40b6-8980-26c7ee164566"), value);
    }

    [Fact]
    public void CanGetDefaultGuid()
    {
      var defaultValue = Guid.NewGuid();
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var value = configuration.GetValueGuid("UNKNOWN_KEY", defaultValue);
      Assert.Equal(defaultValue, value);
    }

    [Fact]
    public void CanGetLoggingConfig()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var section = configuration.GetLoggingConfig();
      Assert.NotNull(section);
    }
  }
}
