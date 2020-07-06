using CoreX.Interfaces;
using CoreX.Wrapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.Alignments;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Gateway.WebApi.ActionServices;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Servers.Client;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Models;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.WebApi.Common;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.Gateway.WebApi
{
  public class Startup : BaseStartup
  {
    /// <inheritdoc/>
    public override string ServiceName => "TRex Gateway API";

    /// <inheritdoc/>
    public override string ServiceDescription => "TRex Gateway API";

    /// <inheritdoc/>
    public override string ServiceVersion => "v1";


    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    /// </summary>

    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      DIBuilder.New(services)
        .Build()
        .Add(x => x.AddSingleton<IConvertCoordinates>(new ConvertCoordinates(new CoreX.Wrapper.CoreX())))
        .Add(IO.DIUtilities.AddPoolCachesToDI)
        .Add(TRexGridFactory.AddGridFactoriesToDI)
        .Add(Storage.Utilities.DIUtilities.AddProxyCacheFactoriesToDI)
        .Build()
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels(StorageMutability.Immutable)))
        .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))

        .Add(x => x.AddSingleton<ISurveyedSurfaceManager>(factory => new SurveyedSurfaceManager(StorageMutability.Immutable)))
        .Add(x => x.AddTransient<IDesigns>(factory => new Designs.Storage.Designs()))
        .Add(x => x.AddSingleton<IDesignManager>(factory => new DesignManager(StorageMutability.Immutable)))
        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces.SurveyedSurfaces()))
        .Add(x => x.AddTransient<IAlignments>(factory => new Alignments.Alignments()))
        .Add(x => x.AddSingleton<IAlignmentManager>(factory => new AlignmentManager(StorageMutability.Immutable)))
        //Monitor number of notifications from this. If too many, go through ignite to get data rather than directly from the site model.
        .Add(x => x.AddSingleton<ISiteModelAttributesChangedEventListener>(new SiteModelAttributesChangedEventListener(TRexGrids.ImmutableGridName())))
        .Build();

      services.AddTransient<IErrorCodesProvider, ContractExecutionStatesEnum>();//Replace with custom error codes provider if required
      services.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddTransient<IReportDataValidationUtility, ReportDataValidationUtility>();
      services.AddTransient<ICoordinateServiceUtility, CoordinateServiceUtility>();
      services.AddSingleton<IProductionEventsFactory>(new ProductionEventsFactory());
      services.AddSingleton<ITransferProxyFactory, TransferProxyFactory>();

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });

      DIBuilder.Continue()
        .Add(x => x.AddSingleton<IImmutableClientServer>(new ImmutableClientServer("TRexIgniteClient-DotNetStandard")))
        .Complete();
    }

    /// <inheritdoc/>
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
    { }
  }
}
