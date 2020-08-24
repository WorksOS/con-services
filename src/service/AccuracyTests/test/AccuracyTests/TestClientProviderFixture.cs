using System.Net.Http;
using CCSS.IntegrationTests.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AccuracyTests
{
  public class TestClientProviderFixture
  {
    public ServiceProvider ServiceProvider { get; }
    public IRestClient RestClient { get; }
    public TestDataGenerator DataGenerator { get; }

    public TestClientProviderFixture()
    {
      var configurationRoot = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .AddJsonFile("appsettings.accuracytests.json", optional: false, reloadOnChange: false)
        .AddEnvironmentVariables()
        .Build();

      ServiceProvider = ConfigureServices(configurationRoot).BuildServiceProvider();

      RestClient = ServiceProvider.GetService<IRestClient>();
      DataGenerator = ServiceProvider.GetService<TestDataGenerator>();
    }

    private static IServiceCollection ConfigureServices(IConfigurationRoot configurationRoot) =>
      new ServiceCollection()
      .AddMemoryCache()
      .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger))
      .AddSingleton<IConfiguration>(configurationRoot)
      .AddHttpClient()
      .AddSingleton<IRestClient>(s => new RestClient(
        s.GetService<ILoggerFactory>(),
        s.GetService<IConfiguration>(),
        s.GetService<IHttpClientFactory>().CreateClient()))
      .AddSingleton<TestDataGenerator>();
  }
}
