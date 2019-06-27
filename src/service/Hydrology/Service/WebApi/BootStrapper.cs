#if NET_4_7
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions;
using Microsoft.Practices.Prism.Modularity;
using Morph.Contracts;
using Morph.Core.Utility;
using Morph.Module.Services;
using Morph.Services.Engine.Modules;
using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Windows;

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
      AggregateCatalog.Catalogs.Add((ComposablePartCatalog) new AssemblyCatalog(typeof(Program).Assembly));
      AggregateCatalog.Catalogs.Add((ComposablePartCatalog) new AssemblyCatalog(typeof(InterfaceTypes).Assembly));
      AggregateCatalog.Catalogs.Add((ComposablePartCatalog) new AssemblyCatalog(typeof(UnitConverter).Assembly));
      AggregateCatalog.Catalogs.Add((ComposablePartCatalog) new AssemblyCatalog(typeof(ServicesModule).Assembly));
      AggregateCatalog.Catalogs.Add((ComposablePartCatalog) new AssemblyCatalog(typeof(EngineService).Assembly));
    }

    protected override DependencyObject CreateShell()
    {
      return null;
    }

    public void Init()
    {
      Logger = CreateLogger();
      if (Logger == null)
        throw new InvalidOperationException("Resources.NullLoggerFacadeException");
      Logger.Log("Resources.LoggerWasCreatedSuccessfully", Category.Debug, Priority.Low);
      Logger.Log("Resources.CreatingModuleCatalog", Category.Debug, Priority.Low);
      ModuleCatalog = CreateModuleCatalog();
      if (ModuleCatalog == null)
        throw new InvalidOperationException("Resources.NullModuleCatalogException");
      Logger.Log("Resources.ConfiguringModuleCatalog", Category.Debug, Priority.Low);
      ConfigureModuleCatalog();
      Logger.Log("Resources.CreatingCatalogForMEF", Category.Debug, Priority.Low);
      AggregateCatalog = this.CreateAggregateCatalog();
      Logger.Log("Resources.ConfiguringCatalogForMEF", Category.Debug, Priority.Low);
      ConfigureAggregateCatalog();
      RegisterDefaultTypesIfMissing();
      Logger.Log("Resources.CreatingMefContainer", Category.Debug, Priority.Low);
      Container = CreateContainer();
      if (Container == null)
        throw new InvalidOperationException("Resources.NullCompositionContainerException");
      Logger.Log("Resources.ConfiguringMefContainer", Category.Debug, Priority.Low);
      ConfigureContainer();
      Logger.Log("Resources.ConfiguringServiceLocatorSingleton", Category.Debug, Priority.Low);
      ConfigureServiceLocator();
      var exports = Container.GetExports(typeof(IModuleManager), (Type) null, (string) null);

      if (exports.Any())
      {
        Logger.Log("Resources.InitializingModules", Category.Debug, Priority.Low);
        InitializeModules();
      }

      Logger.Log("Resources.BootstrapperSequenceCompleted", Category.Debug, Priority.Low);
    }
  }
}
#endif
