using System;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using Magnum.Extensions;
using Topshelf;
using Topshelf.Runtime;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Data;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Service;
using VSS.Nighthawk.ReferenceIdentifierService.Workers;
using VSS.Nighthawk.ReferenceIdentifierService.Encryption;

namespace VSS.Nighthawk.ReferenceIdentifierService
{
  public class Program
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static IContainer _container { get; set; }

    static void Main(string[] args)
    {
      TopshelfExitCode exitCode = HostFactory.Run(hostConfig =>
      {
        hostConfig.SetServiceName("_NHReferenceIdentifierSvc");
        hostConfig.SetDisplayName("_NHReferenceIdentifierSvc");
        hostConfig.SetDescription("Service to get and create reference identifiers for customers, assets, devices, and services");
        hostConfig.RunAsLocalSystem();
        hostConfig.StartAutomatically();
        hostConfig.EnableServiceRecovery(svcRecoveryConfig =>
        {
          svcRecoveryConfig.RestartService(1);
          svcRecoveryConfig.RestartService(1);
          svcRecoveryConfig.RestartService(1);
        }
          );
        hostConfig.Service<RestService>(svcConfig =>
        {
          svcConfig.ConstructUsing(ServiceFactory);
          svcConfig.WhenStarted(restService =>
          {
            restService.Start();
            Log.IfInfo("ReferenceIdentifierService started");
          }
          );
          svcConfig.WhenStopped(restService =>
          {
            restService.Stop();
            Log.IfInfo("ReferenceIdentifierService stopped");
          }
          );
        }
        );
        hostConfig.UseLog4Net();
      });

      switch (exitCode)
      {
        case TopshelfExitCode.Ok:
          Log.IfInfoFormat("_NHReferenceIdentifierSvc: Service Exited");
          break;
        case TopshelfExitCode.AbnormalExit:
          Log.IfDebugFormat("_NHReferenceIdentifierSvc: Service AbnormalExit");
          break;
        case TopshelfExitCode.SudoRequired:
          Log.IfDebugFormat("_NHReferenceIdentifierSvc: Service SudoRequired");
          break;
        case TopshelfExitCode.ServiceAlreadyInstalled:
          Log.IfDebugFormat("_NHReferenceIdentifierSvc: ServiceAlreadyInstalled");
          break;
        case TopshelfExitCode.ServiceNotInstalled:
          Log.IfDebugFormat("_NHReferenceIdentifierSvc: ServiceNotInstalled");
          break;
        case TopshelfExitCode.StartServiceFailed:
          Log.IfDebugFormat("_NHReferenceIdentifierSvc: StartServiceFailed");
          break;
        case TopshelfExitCode.StopServiceFailed:
          Log.IfDebugFormat("_NHReferenceIdentifierSvc: StopServiceFailed");
          break;
      }
    }

    public static RestService ServiceFactory(HostSettings settings)
    {
      var builder = new ContainerBuilder();

      builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

      builder.Register(c => new RestService(new Uri(Config.Default.BaseUri)))
        .As<RestService>()
        .SingleInstance();

      builder.RegisterType<NHOpContextFactory>()
      .As<INHOpContextFactory>()
      .SingleInstance();

      builder.Register(c => new CacheManagerConfig("LocalCacheManager"))
        .As<ICacheManagerConfig>()
        .SingleInstance();

      builder.RegisterType<CacheManager>()
        .As<ICacheManager>()
        .SingleInstance();

      builder.Register(context => new Storage(
        context.Resolve<INHOpContextFactory>(),
        context.Resolve<ICacheManager>(),
        context.Resolve<IStringEncryptor>(),
        Config.Default.CustomerCacheLifetimeInMinutes,
        Config.Default.AssetCacheLifetimeInMinutes,
        Config.Default.DeviceCacheLifetimeInMinutes,
        Config.Default.ServiceCacheLifetimeInMinutes
        ))
        .As<IStorage>()
        .SingleInstance();

      builder.Register(context => new CredentialManager(context.Resolve<IStorage>()))
        .As<ICredentialManager>()
        .SingleInstance();

      builder.Register(context => new CustomerIdentifierManager(context.Resolve<IStorage>()))
        .As<ICustomerIdentifierManager>()
        .SingleInstance();

      builder.Register(context => new AssetIdentifierManager(context.Resolve<IStorage>()))
        .As<IAssetIdentifierManager>()
        .SingleInstance();

      builder.Register(context => new DeviceIdentifierManager(context.Resolve<IStorage>()))
        .As<IDeviceIdentifierManager>()
        .SingleInstance();

      builder.Register(context => new ServiceIdentifierManager(context.Resolve<IStorage>()))
        .As<IServiceIdentifierManager>()
        .SingleInstance();

      builder.Register(context => new CustomerLookupManager(context.Resolve<IStorage>()))
        .As<ICustomerLookupManager>()
        .SingleInstance();

      builder.Register(context => new OemLookupManager(context.Resolve<IStorage>()))
        .As<IOemLookupManager>()
        .SingleInstance();

      builder.Register(context => new ServiceLookupManager(context.Resolve<IStorage>()))
        .As<IServiceLookupManager>()
        .SingleInstance();

      builder.Register(context => new StoreLookupManager(context.Resolve<IStorage>()))
        .As<IStoreLookupManager>()
        .SingleInstance();

      builder.RegisterType<StringEncryptor>().As<IStringEncryptor>().SingleInstance();

      builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
        .Where(t => t.Implements<ApiController>())
        .AsSelf();

      _container = builder.Build();

      var restService = _container.Resolve<RestService>();
      restService.Configure(new AutofacWebApiDependencyResolver(_container));

      return restService;
    }
  }
}
