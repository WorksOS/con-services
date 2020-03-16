using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Tpaas.Client.Abstractions;
using VSS.Tpaas.Client.Clients;
using VSS.Tpaas.Client.RequestHandlers;
using VSS.TRex.ConnectedSite.Gateway.Abstractions;
using VSS.TRex.DI;
using VSS.WebApi.Common;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi
{
  public class Startup : BaseStartup
  {
    private const string CONNECTED_SITE_URL_ENV_KEY = "CONNECTED_SITE_URL";

    public override string ServiceName => "Works manager tag file processor";
    public override string ServiceDescription => "Processes tag files and sends them to WorksManager";
    public override string ServiceVersion => "v1";

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    /// </summary>
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {

      services.AddHttpClient<ITPaaSClient, TPaaSClient>(client =>
        client.BaseAddress = new Uri(Configuration.GetValueString(TPaaSClient.TPAAS_AUTH_URL_ENV_KEY))
      ).ConfigurePrimaryHttpMessageHandler(() => new TPaaSApplicationCredentialsRequestHandler
      {
        TPaaSToken = Configuration.GetValueString(TPaaSApplicationCredentialsRequestHandler.TPAAS_APP_TOKEN_ENV_KEY),
        InnerHandler = new HttpClientHandler()
      });

      services.AddTransient(context => new TPaaSAuthenticatedRequestHandler
      {
        TPaaSClient = context.GetService<ITPaaSClient>()
      });

      services.AddHttpClient<IConnectedSiteClient, ConnectedSiteClient>(client =>
        client.BaseAddress = new Uri(Configuration.GetValueString(CONNECTED_SITE_URL_ENV_KEY))
      ).AddHttpMessageHandler<TPaaSAuthenticatedRequestHandler>();

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });

      //Set up logging etc. for TRex
      DIContext.Inject(services.BuildServiceProvider());
    }

    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
    { }
  }
}
