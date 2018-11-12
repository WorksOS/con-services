using System;
using System.Configuration;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.HttpClients.Clients;
using VSS.TRex.HttpClients.RequestHandlers;
using VSS.TRex.DI;
using VSS.WebApi.Common;
using VSS.TRrex.HttpClients.Abstractions;
using VSS.TRex.ConnectedSite.Gateway.WebApi.Abstractions;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi
{
  public class Startup
  {
    /// <summary>
    /// The name of this service for swagger etc.
    /// </summary>
    private const string SERVICE_TITLE = "TRex Connected Site Gateway API";
    /// <summary>
    /// The logger repository name
    /// </summary>
    public const string LOGGER_REPO_NAME = "WebApi";


    private const string CONNECTED_SITE_URL_ENV_KEY = "CONNECTED_SITE_URL";

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
      // Add framework services.
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<IErrorCodesProvider, ContractExecutionStatesEnum>();//Replace with custom error codes provider if required
      services.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddTransient<TPaaSAuthenticatedRequestHandler>();
      services.AddHttpClient<ITPaaSClient, TPaaSClient>(client =>
        client.BaseAddress = new Uri(GetRequiredEnvironmentVariable(TPaaSClient.TPAAS_AUTH_URL_ENV_KEY))
      ).ConfigurePrimaryHttpMessageHandler(() =>
      {
        return new TPaaSApplicationCredentialsRequestHandler()
        {
          TPaaSToken = GetRequiredEnvironmentVariable(TPaaSApplicationCredentialsRequestHandler.TPAAS_APP_TOKEN__ENV_KEY),
          InnerHandler = new HttpClientHandler()
        };
      });
      services.AddHttpClient<IConnectedSiteClient, ConnectedSiteClient>(client =>
          client.BaseAddress = new Uri(GetRequiredEnvironmentVariable(CONNECTED_SITE_URL_ENV_KEY))
      ).AddHttpMessageHandler<TPaaSAuthenticatedRequestHandler>();

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });

      services.AddJaeger(SERVICE_TITLE);

      services.AddCommon<Startup>(SERVICE_TITLE, "API for TRex Connected Site Gateway");

      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

      //Set up logging etc. for TRex
      var serviceProvider = services.BuildServiceProvider();
      var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
      Logging.Logger.Inject(loggerFactory);
      DIContext.Inject(serviceProvider);

    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseCommon(SERVICE_TITLE);
      app.UseMvc();
    }

    private string GetRequiredEnvironmentVariable(string key)
    {
      string value = string.Empty;
      value = Environment.GetEnvironmentVariable(key);
      if (string.IsNullOrEmpty(value))
      {
        throw new ConfigurationErrorsException($"Missing Envar {key}");
      }
      else
      {
        return value;
      }
    }
  }
}
