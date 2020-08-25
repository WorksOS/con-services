using System.IO;
using System.Net.Http;
using CCSS.CWS.Client;
using CCSS.CWS.Client.MockClients;
using CCSS.IntegrationTests.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Cache.MemoryCache;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.FlowJSHandler;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI;
using VSS.MasterData.Project.WebAPI.Middleware;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Entitlements.Abstractions.Interfaces;
using VSS.Productivity3D.Entitlements.Proxy;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.TRex.Gateway.Common.Proxy;
using VSS.WebApi.Common;

namespace VSS.MasterData.ProjectTests.FlowJS
{
  public class ProjectTestAuthentication : ProjectAuthentication
  {
    public ProjectTestAuthentication(RequestDelegate next,
      ICwsAccountClient cwsAccountClient,
      IConfigurationStore store,
      ILoggerFactory logger,
      IEntitlementProxy entitlementProxy,
      IServiceExceptionHandler serviceExceptionHandler) : base(next, cwsAccountClient, store, logger, entitlementProxy, serviceExceptionHandler)
    { }

    public override bool InternalConnection(HttpContext context)
    {
      return true;
    }
  }

  public class MockStartup
  {
    public void ConfigureServices(IServiceCollection services)
    { }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseFilterMiddleware<ProjectTestAuthentication>();

    }
  }

  public class TestClientProviderFixture
  {
    public ServiceProvider ServiceProvider { get; }
    public IRestClient RestClient { get; }

    public TestClientProviderFixture()
    {
      var mockIHttpClientFactory = new Mock<IHttpClientFactory>();

      var hostBuilder = CreateHostBuilder(mockIHttpClientFactory.Object).Start();

      mockIHttpClientFactory
        .Setup(_ => _.CreateClient(It.IsAny<string>()))
        .Returns(hostBuilder.GetTestClient());



      ServiceProvider = ConfigureServices(mockIHttpClientFactory.Object)
        .BuildServiceProvider();

      RestClient = ServiceProvider.GetService<IRestClient>();
    }

    /// <summary>
    /// The same as the Web APIs Program.cs, except we're including UseTestServer() to add the TestServer implementation.
    /// </summary>
    public static IHostBuilder CreateHostBuilder(IHttpClientFactory mockHttpClientFactory) =>
      Host.CreateDefaultBuilder()
      .UseEnvironment("IntegrationTests")
      .ConfigureWebHostDefaults(webBuilder =>
      {
        webBuilder.UseLibuv(opts => opts.ThreadCount = 32)
        .BuildKestrelWebHost()
        .ConfigureLogging((hostContext, loggingBuilder) =>
        {
          loggingBuilder.AddProvider(
            p => new SerilogLoggerProvider(
              SerilogExtensions.Configure("VSS.TagFileAuth.WebAPI.log", httpContextAccessor: p.GetService<IHttpContextAccessor>())));
        })
        .UseStartup<Startup>()
        .UseTestServer()
        .ConfigureServices(services =>
        {
          // Services required by the running application, Project.WebAPI in this case.
          services
          .AddMemoryCache()
          .AddSingleton<IConfigurationStore, GenericConfiguration>()
          .AddSingleton<IWebRequest, GracefulWebRequest>()
          .AddHttpClient()
          // We need to HttpClientFactory to return our TestServer HttpClient when .CreateClient() is called in GracefulWebRequest.
          .AddSingleton<IHttpClientFactory>(mockHttpClientFactory)
          .AddSingleton<IDataCache, InMemoryDataCache>()
          .AddSingleton<IServiceResolution, InternalServiceResolver>()
          .AddSingleton<IEntitlementProxy, EntitlementProxy>()
          .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
          .AddTransient<IErrorCodesProvider, ContractExecutionStatesEnum>()
          .AddTransient<IFlowJsRepo, FlowJsRepo>()
          .AddCwsClient<ICwsAccountClient, CwsAccountClient, MockCwsAccountClient>(CwsClientMockExtensionMethods.MOCK_ACCOUNT_KEY);
        });
      });

    private static IServiceCollection ConfigureServices(IHttpClientFactory mockIHttpClientFactory)
    {
      var config = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.IntegrationTests.json", optional: true, reloadOnChange: false)
                   .Build();

      Log.Logger = new LoggerConfiguration()
                   .ReadFrom.Configuration(config)
                   .CreateLogger();

      var serviceCollection = new ServiceCollection()
        .AddMemoryCache()
        .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger))
        .AddSingleton<IConfiguration>(config)
        //        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        //        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
        .AddSingleton<IWebRequest, GracefulWebRequest>()
        .AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>()
        .AddTransient<ITPaasProxy, TPaasProxy>()
        .AddTransient<IProjectInternalProxy, ProjectInternalV6Proxy>()
        .AddTransient<IDeviceInternalProxy, DeviceInternalV1Proxy>()
        .AddTransient<ITRexCompactionDataProxy, TRexCompactionDataV1Proxy>()
        .AddSingleton<IHttpClientFactory>(mockIHttpClientFactory)
        .AddHttpClient()
        .AddSingleton<IRestClient>(s => new RestClient(
          s.GetService<ILoggerFactory>(),
          s.GetService<IConfiguration>(),
          s.GetService<IHttpClientFactory>()));
      //.AddCwsClient<ICwsAccountClient, CwsAccountClient, MockCwsAccountClient>(CwsClientMockExtensionMethods.MOCK_ACCOUNT_KEY);

      return serviceCollection;
    }
  }
}
