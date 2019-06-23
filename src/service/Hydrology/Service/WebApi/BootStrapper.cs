#if NET_4_7
using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Morph.Core.Utility;
using Morph.Services.Engine.Modules;
using Prism.Logging;
using Prism.Mef;

namespace VSS.Hydrology.WebApi
{
  class BootStrapper : MefBootstrapper
  {
    private readonly IServiceProvider _serviceProvider;

    public BootStrapper(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

    protected override void ConfigureAggregateCatalog()
    {
      base.ConfigureAggregateCatalog();
      this.AggregateCatalog.Catalogs.Add((ComposablePartCatalog) new AssemblyCatalog(typeof(Program).Assembly));
//      this.AggregateCatalog.Catalogs.Add((ComposablePartCatalog)new AssemblyCatalog(typeof(InterfaceTypes).Assembly));
      this.AggregateCatalog.Catalogs.Add((ComposablePartCatalog) new AssemblyCatalog(typeof(UnitConverter).Assembly));
//      this.AggregateCatalog.Catalogs.Add((ComposablePartCatalog)new AssemblyCatalog(typeof(ServicesModule).Assembly));
      this.AggregateCatalog.Catalogs.Add((ComposablePartCatalog) new AssemblyCatalog(typeof(EngineService).Assembly));
    }

    protected override ILoggerFacade CreateLogger()
    {
      return new LoggerFacade(_serviceProvider.GetService<ILoggerFactory>().CreateLogger<MefBootstrapper>());
    }

    public void Init()
    {
      this.Logger = this.CreateLogger();

      base.Run();
      this.ConfigureServiceLocator();
    }
  }
}
#endif
