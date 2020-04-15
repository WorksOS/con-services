using System;
using System.Net.Http;
using FileAccess.IntegrationTests.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VSS.TCCFileAccess;
using VSS.WebApi.Common;
using Startup = VSS.Productivity3D.FileAccess.WebAPI.Startup;

namespace FileAccess.IntegrationTests
{
  public class TestClientProviderFixture : IDisposable
  {
    public HttpClient Client { get; }

    public TestClientProviderFixture()
    {
      Client = CreateHostBuilder().Start()
                                  .GetTestClient();
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
                      .UseTestServer()
                      .ConfigureTestServices(services => services.AddTransient<IFileRepository, MockFileRepository>());
          });

    public void Dispose()
    {
      Client?.Dispose();
    }
  }
}
