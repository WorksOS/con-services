using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.ServiceDiscovery;
using VSS.Productivity3D.TagFileAuth.Abstractions.Interfaces;
using VSS.Productivity3D.TagFileAuth.Proxy;
using VSS.TRex.Alignments;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.WebApi.Common;

namespace VSS.TRex.Mutable.Gateway.WebApi
{
  public class Startup : BaseStartup
  {
    public override string ServiceName => "TRex Mutable Gateway API";
    public override string ServiceDescription => "TRex Mutable Gateway API";
    public override string ServiceVersion => "v1";

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    /// </summary>

    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      DIBuilder.New(services)
         .Build()
         .Add(x => x.AddSingleton<IConvertCoordinates>(new ConvertCoordinates()))
         .Add(VSS.TRex.IO.DIUtilities.AddPoolCachesToDI)
         .Add(TRexGridFactory.AddGridFactoriesToDI)
         .Build()
         .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels()))

         .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))
         .Add(x => x.AddSingleton<ISiteModelMetadataManager>(factory => new SiteModelMetadataManager(StorageMutability.Mutable)))

         .Add(x => x.AddSingleton<ISurveyedSurfaceManager>(factory => new SurveyedSurfaceManager(StorageMutability.Mutable)))
         .Add(x => x.AddTransient<IDesigns>(factory => new Designs.Storage.Designs()))
         .Add(x => x.AddSingleton<IDesignManager>(factory => new DesignManager(StorageMutability.Mutable)))
         .Add(x => x.AddSingleton<IMutabilityConverter>(new MutabilityConverter()))
         .Add(x => x.AddSingleton<ISiteModelAttributesChangedEventSender>(new SiteModelAttributesChangedEventSender()))
         .Add(x => x.AddSingleton<ISiteModelAttributesChangedEventListener>(new SiteModelAttributesChangedEventListener(TRexGrids.MutableGridName())))
         .Add(ExistenceMaps.ExistenceMaps.AddExistenceMapFactoriesToDI)
         .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces.SurveyedSurfaces()))
         .Add(x => x.AddTransient<IAlignments>(factory => new Alignments.Alignments()))
         .Add(x => x.AddSingleton<IAlignmentManager>(factory => new AlignmentManager(StorageMutability.Mutable)))
         .Build();

      services.AddServiceDiscovery();
      services.AddSingleton<ITagFileAuthProjectProxy, TagFileAuthProjectV2ServiceDiscoveryProxy>();
      services.AddTransient<ITransferProxy>(sp => new TransferProxy(sp.GetRequiredService<IConfigurationStore>(), "AWS_DESIGNIMPORT_BUCKET_NAME"));

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<IMutableClientServer>(new MutableClientServer(ServerRoles.TAG_PROCESSING_NODE_CLIENT)))
        .Add(x => x.AddSingleton<ImmutableClientServer>(new ImmutableClientServer("WEBAPI-CLIENT")))
        .Complete();
    }

    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory factory)
    { }

    public Startup(IHostingEnvironment env) : base(env, null, useSerilog: true)
    { }
  }
}
