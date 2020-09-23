using System.Net.Http;
using CCSS.IntegrationTests.Utils;
using CCSS.WorksOS.Healthz;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Serilog;
using VSS.Serilog.Extensions;

namespace IntegrationTests
{
  public class TestFixture
  {
    public ServiceProvider ServiceProvider { get; }
    public IIntegrationTestRestClient RestClient { get; }

    private const string TARGET_ENVIRONMENT = "IntegrationTests";

    public TestFixture()
    {
      var configurationRoot = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .AddJsonFile($"appsettings.{TARGET_ENVIRONMENT}.json", optional: false, reloadOnChange: false)
        .AddEnvironmentVariables()
        .Build();

      var host = CreateHostBuilder(configurationRoot).Build();

      host.Start();

      var mockIHttpClientFactory = new Mock<IHttpClientFactory>();

      mockIHttpClientFactory
        .Setup(_ => _.CreateClient(It.IsAny<string>()))
        .Returns(host.GetTestClient());

      ServiceProvider = ConfigureServices(configurationRoot, mockIHttpClientFactory.Object)
        .BuildServiceProvider();

      RestClient = ServiceProvider.GetRequiredService<IIntegrationTestRestClient>();
    }

    /// <summary>
    /// The same as the Web APIs Program.cs, except we're including UseTestServer() to add the TestServer implementation.
    /// </summary>
    public static IHostBuilder CreateHostBuilder(IConfigurationRoot configurationRoot)
    {
      Log.Logger = SerilogExtensions
        .Configure(logFilename: $"Healthz.IntegrationTests.{TARGET_ENVIRONMENT}.log", config: configurationRoot);

      return Host
       .CreateDefaultBuilder()
       .UseSerilog()
       .ConfigureWebHostDefaults(webBuilder =>
       {
         webBuilder
         .UseConfiguration(configurationRoot)
         .UseKestrel()
         .UseEnvironment(TARGET_ENVIRONMENT)
         .UseStartup<Startup>()
         .UseTestServer();
       });
    }

    private static IServiceCollection ConfigureServices(IConfigurationRoot configurationRoot, IHttpClientFactory httpClientFactory)
    {
      var services = new ServiceCollection()
        .AddMemoryCache()
        .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger))
        .AddSingleton<IConfiguration>(configurationRoot)
        .AddSingleton(httpClientFactory)
        .AddSingleton<IIntegrationTestRestClient, IntegrationTestRestClient>();

      services.AddHttpClient("configured-disable-automatic-cookies")
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
          return new SocketsHttpHandler()
          {
            UseCookies = false,
          };
        });

      return services;
    }
  }
}
