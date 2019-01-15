﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.WebApi.Common;
using VSS.TRex.DI;
using VSS.TRex.Exports.Surfaces.Requestors;
using VSS.TRex.GridFabric.Servers.Client;
using VSS.TRex.SiteModels;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.Gateway.WebApi
{
  public class Startup
  {
    /// <summary>
    /// The name of this service for swagger etc.
    /// </summary>
    private const string SERVICE_TITLE = "TRex Gateway API";
    /// <summary>
    /// The logger repository name
    /// </summary>
    public const string LOGGER_REPO_NAME = "WebApi";

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
      // Add framework services.
      var storageProxyFactory = new StorageProxyFactory();

      DIBuilder.New(services)
        .Add(x => x.AddSingleton<ITRexGridFactory>(new TRexGridFactory()))
        .Add(x => x.AddSingleton<IStorageProxyFactory>(storageProxyFactory))
        .Add(VSS.TRex.Storage.Utilities.DIUtilities.AddProxyCacheFactoriesToDI)
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels(() => storageProxyFactory.ImmutableGridStorage())))

        .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))
        .Add(x => x.AddTransient<ITINSurfaceExportRequestor>(factory => new TINSurfaceExportRequestor()))

        .Add(x => x.AddSingleton<ISurveyedSurfaceManager>(factory => new SurveyedSurfaceManager()))
        .Add(x => x.AddTransient<IDesigns>(factory => new Designs.Storage.Designs()))
        .Add(x => x.AddSingleton<IDesignManager>(factory => new DesignManager()));

      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<IErrorCodesProvider, ContractExecutionStatesEnum>();//Replace with custom error codes provider if required
      services.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>();

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });

      services.AddJaeger(SERVICE_TITLE);

      //services.AddMemoryCache();
      services.AddCommon<Startup>(SERVICE_TITLE, "API for TRex Gateway");

      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

      //Set up logging etc. for TRex
      var serviceProvider = services.BuildServiceProvider();
      var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
      Logging.Logger.Inject(loggerFactory);
      DIContext.Inject(serviceProvider);

      services.AddSingleton(new ImmutableClientServer("TRexIgniteClient-DotNetStandard"));
      serviceProvider = services.BuildServiceProvider();
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
  }
}
