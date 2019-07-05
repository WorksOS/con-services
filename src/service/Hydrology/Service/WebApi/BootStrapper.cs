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
      AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));
      AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(InterfaceTypes).Assembly));
      AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(UnitConverter).Assembly));
      AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ServicesModule).Assembly));
      AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(EngineService).Assembly));
    }

    protected override DependencyObject CreateShell()
    {
      return null;
    }

    public void Init()
    {
      Logger = CreateLogger();
      if (Logger == null)
        throw new InvalidOperationException($"{nameof(BootStrapper)} Unable to create logger.");
      Logger.Log($"{nameof(BootStrapper)} Logger created.", Category.Debug, Priority.Low);

      ModuleCatalog = CreateModuleCatalog();
      if (ModuleCatalog == null)
        throw new InvalidOperationException($"{nameof(BootStrapper)} Unable to create module catalog.");
      ConfigureModuleCatalog();
      Logger.Log($"{nameof(BootStrapper)} module catalog configured for MEF", Category.Debug, Priority.Low);

      AggregateCatalog = this.CreateAggregateCatalog();
      Logger.Log($"{nameof(BootStrapper)} aggregate catalog created for MEF", Category.Debug, Priority.Low);
      ConfigureAggregateCatalog();
      RegisterDefaultTypesIfMissing();
      Logger.Log($"{nameof(BootStrapper)} aggregate catalog configured for MEF", Category.Debug, Priority.Low);

      Container = CreateContainer();
      if (Container == null)
        throw new InvalidOperationException($"{nameof(BootStrapper)} Unable to compose MEF container.");
      Logger.Log($"{nameof(BootStrapper)} MEF container composed.", Category.Debug, Priority.Low);
      ConfigureContainer();
      Logger.Log($"{nameof(BootStrapper)} MEF container configured.", Category.Debug, Priority.Low);
      ConfigureServiceLocator();

      var exports = Container.GetExports(typeof(IModuleManager), (Type) null, null);
      var exportList = exports.ToList();
      if (exportList.Any())
      {
        Logger.Log($"{nameof(BootStrapper)} Initializing exports in modules. export count: {exportList.Count}", Category.Debug, Priority.Low);
        InitializeModules();
      }

      Logger.Log($"{ nameof(BootStrapper)} Completed", Category.Debug, Priority.Low);
    }
  }
}
#endif
