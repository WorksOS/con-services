using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Servers.Client;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.WebApi.Common;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.SiteModels;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.Mutable.Gateway.WebApi
{
  public class Startup
  {
    /// <summary>
    /// The name of this service for swagger etc.
    /// </summary>
    private const string SERVICE_TITLE = "TRex Mutable Gateway API";
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

      services.AddSingleton<ITRexGridFactory>(new TRexGridFactory());
      services.AddSingleton<IStorageProxyFactory>(storageProxyFactory);
      services.AddSingleton<ISiteModels>(new SiteModels.SiteModels(() => storageProxyFactory.MutableGridStorage()));
      services.AddSingleton<ISiteModelFactory>(new SiteModelFactory());
      services.AddSingleton<ISiteModelMetadataManager>(factory => new SiteModelMetadataManager());
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddSingleton<ITagFileAuthProjectProxy, TagFileAuthProjectProxy>();
      services.AddTransient<IErrorCodesProvider, ContractExecutionStatesEnum>();//Replace with custom error codes provider if required
      services.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddTransient<IDesigns>(factory => new Designs.Storage.Designs());
      services.AddSingleton<IDesignManager>(factory => new DesignManager());
      services.AddSingleton<IMutabilityConverter>(new MutabilityConverter());
      services.AddSingleton<ISiteModelAttributesChangedEventSender>(new SiteModelAttributesChangedEventSender());
      services.AddSingleton<IExistenceMaps>(factory => new ExistenceMaps.ExistenceMaps());
      services.AddTransient<ITransferProxy>(sp => new TransferProxy(sp.GetRequiredService<IConfigurationStore>(), "AWS_DESIGNIMPORT_BUCKET_NAME"));

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });

      services.AddJaeger(SERVICE_TITLE);

      services.AddCommon<Startup>(SERVICE_TITLE, "API for TRex Mutable Gateway");

      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

      //Set up logging etc. for TRex
      var serviceProvider = services.BuildServiceProvider();
      var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
      Logging.Logger.Inject(loggerFactory);
      DIContext.Inject(serviceProvider);

      services.AddSingleton<IMutableClientServer>(new MutableClientServer(ServerRoles.TAG_PROCESSING_NODE_CLIENT));
      services.AddSingleton<ImmutableClientServer>(new ImmutableClientServer("WEBAPI-CLIENT"));

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
