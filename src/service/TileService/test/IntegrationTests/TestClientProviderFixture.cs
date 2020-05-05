using System.IO;
using System.Net.Http;
using CCSS.IntegrationTests.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Tile.Service.WebApi;
using VSS.WebApi.Common;

namespace CCSS.Tile.Service.IntegrationTests
{
  public class TestClientProviderFixture
  {
    public ServiceProvider ServiceProvider { get; }
    public IRestClient RestClient { get; }

    public TestClientProviderFixture()
    {
      var httpClient = CreateHostBuilder().Start()
                                          .GetTestClient();

      ServiceProvider = ConfigureServices(httpClient).BuildServiceProvider();

      RestClient = ServiceProvider.GetService<IRestClient>();

    }

    /// <summary>
    /// The same as the Web APIs Program.cs, except we're including UseTestServer() to add the TestServer implementation.
    /// </summary>
    public static IHostBuilder CreateHostBuilder() =>
      Host.CreateDefaultBuilder()
          .ConfigureWebHostDefaults(webBuilder =>
          {
            webBuilder.UseLibuv(opts => opts.ThreadCount = 32)
                      .BuildKestrelWebHost()
                      .UseStartup<Startup>()
                      .UseTestServer();
          });


    private static IServiceCollection ConfigureServices(HttpClient client)
    {
      var config = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                   .Build();

      Log.Logger = new LoggerConfiguration()
                   .ReadFrom.Configuration(config)
                   .CreateLogger();

      return new ServiceCollection()
             .AddMemoryCache()
             .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger))
             .AddSingleton<IConfiguration>(config)
             .AddSingleton<IRestClient>(s => new RestClient(s.GetService<ILoggerFactory>(), s.GetService<IConfiguration>(), client));
    }
  }
}
